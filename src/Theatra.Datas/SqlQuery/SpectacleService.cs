using Npgsql;
using System.Diagnostics;
using Theatre.Core.Models;
using Dapper;
using System.Windows;
using System.Windows.Media;
using PdfSharp.Pdf;
using PdfSharp.Drawing;
using System.IO;

namespace Theatra.Datas.SqlQuery
{
    public class SpectacleService
    {
        private readonly string _connectionString;
        public Spectacles Spectacle { get; set; }
        public User User { get; set; }
        public SpectacleService(string connectionString)
        {
            _connectionString = connectionString;
        }

        public async Task DeleteSpectacle(int id)
        {
            using var connection = new NpgsqlConnection(_connectionString);
            await connection.OpenAsync();
            using var transaction = await connection.BeginTransactionAsync();

            try
            {
                // 1. Получаем полную информацию о спектакле
                var spectacle = await GetSpectacleById(id);

                // 2. Проверяем наличие купленных билетов
                const string checkTicketsSql = @"
            SELECT COUNT(*) FROM ticket 
            WHERE number_seat IN (
                SELECT id_seat FROM seat 
                WHERE id_spectacle = @id AND isbooked = true
            )";

                var bookedTicketsCount = await connection.ExecuteScalarAsync<int>(checkTicketsSql, new { id }, transaction);

                if (bookedTicketsCount > 0)
                {
                    throw new InvalidOperationException(
                        $"Невозможно удалить спектакль '{spectacle.Name}'. Имеются купленные билеты.");
                }

                // 3. Удаляем все зависимости в правильном порядке
                var dependencies = new Dictionary<string, string>
                {
                    ["ticket"] = "number_seat IN (SELECT id_seat FROM seat WHERE id_spectacle = @id)",
                    ["seat"] = "id_spectacle = @id",
                    ["spectacle_actor"] = "id_spectacle = @id",
                    ["producer_spectacle"] = "id_spectacle = @id",
                    ["spectacle_genre"] = "id_spectacle = @id"
                };

                foreach (var dep in dependencies)
                {
                    await ExecuteDeleteCommand(
                        connection,
                        transaction,
                        $"DELETE FROM {dep.Key} WHERE {dep.Value}",
                        id,
                        spectacle.Name);
                }

                // 4. Удаляем сам спектакль
                const string deleteSql = "DELETE FROM spectacles WHERE id_spectacle = @id";
                await ExecuteDeleteCommand(connection, transaction, deleteSql, id, spectacle.Name);

                await transaction.CommitAsync();
            }
            catch (Exception ex)
            {
               
                await transaction.RollbackAsync();
                throw new Exception(ex.Message);
            }
        }

        private async Task ExecuteDeleteCommand(
            NpgsqlConnection connection,
            NpgsqlTransaction transaction,
            string sql,
            int id,
            string spectacleName)
        {
            try
            {
                await using var command = new NpgsqlCommand(sql, connection, transaction);
                command.Parameters.AddWithValue("@id", id);
                int affected = await command.ExecuteNonQueryAsync();

                Debug.WriteLine($"Удалено {affected} записей для спектакля '{spectacleName}' ({sql})");
            }
            catch (Exception ex)
            {
                throw new Exception($"Ошибка при выполнении: {sql}. {ex.Message}");
            }
        }


        public async Task<Spectacles> UpdateSpectacle(Spectacles spectacle)
        {
            using var connection = new NpgsqlConnection(_connectionString);
            await connection.OpenAsync();
            using var transaction = await connection.BeginTransactionAsync();

            try
            {
                // 1. Обновляем основные данные спектакля
                const string updateSql = @"
        UPDATE spectacles 
        SET name_spectacle = @name_spectacle, 
            quantity_actors = @quantity_actors,
            photo_spectacle = @photo_spectacle,
            premieredate = @premieredate,
            showtime = @showtime        
        WHERE id_spectacle = @id_spectacle
        RETURNING id_spectacle, name_spectacle, quantity_actors, photo_spectacle, premieredate, showtime";

                Spectacles updatedSpectacle;
                using (var cmd = new NpgsqlCommand(updateSql, connection, transaction))
                {
                    cmd.Parameters.AddWithValue("@name_spectacle", spectacle.Name);
                    cmd.Parameters.AddWithValue("@quantity_actors", spectacle.QuantityActors);
                    cmd.Parameters.AddWithValue("@photo_spectacle", spectacle.Image ?? (object)DBNull.Value);
                    cmd.Parameters.AddWithValue("@premieredate", spectacle.PremiereDate ?? (object)DBNull.Value);
                    cmd.Parameters.AddWithValue("@showtime", spectacle.ShowTime ?? (object)DBNull.Value);
                    cmd.Parameters.AddWithValue("@id_spectacle", spectacle.Id);

                    using var reader = await cmd.ExecuteReaderAsync();
                    if (!await reader.ReadAsync())
                    {
                        throw new Exception("Не удалось обновить спектакль");
                    }

                    updatedSpectacle = new Spectacles
                    {
                        Id = reader.GetInt32(0),
                        Name = reader.GetString(1),
                        QuantityActors = reader.GetInt32(2),
                        Image = reader.IsDBNull(3) ? null : (byte[])reader[3],
                        PremiereDate = reader.IsDBNull(4) ? null : reader.GetDateTime(4),
                        ShowTime = reader.IsDBNull(5) ? null : reader.GetDateTime(5)
                    };
                }

                // 2. Обновляем жанры
                await UpdateSpectacleGenres(connection, transaction, spectacle);

                // 3. Полностью пересоздаем места
                await RecreateAllSeats(connection, transaction, spectacle);

                await transaction.CommitAsync();
                return updatedSpectacle;
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                Debug.WriteLine($"Ошибка при обновлении спектакля: {ex}");
                throw;
            }
        }

        private async Task RecreateAllSeats(NpgsqlConnection connection, NpgsqlTransaction transaction, Spectacles spectacle)
        {
            // 1. Удаляем все старые места (включая связанные билеты)
            const string deleteTicketsSql = "DELETE FROM ticket WHERE number_seat IN (SELECT id_seat FROM seat WHERE id_spectacle = @specId)";
            await using (var cmd = new NpgsqlCommand(deleteTicketsSql, connection, transaction))
            {
                cmd.Parameters.AddWithValue("@specId", spectacle.Id);
                await cmd.ExecuteNonQueryAsync();
            }

            const string deleteSeatsSql = "DELETE FROM seat WHERE id_spectacle = @specId";
            await using (var cmd = new NpgsqlCommand(deleteSeatsSql, connection, transaction))
            {
                cmd.Parameters.AddWithValue("@specId", spectacle.Id);
                await cmd.ExecuteNonQueryAsync();
            }

            // 2. Создаем новые места
            const string insertSql = @"
        INSERT INTO seat (number_seat, price, isbooked, id_spectacle)
        VALUES (@number, @price, @booked, @specId)";

            foreach (var seat in spectacle.Seats)
            {
                await using var cmd = new NpgsqlCommand(insertSql, connection, transaction);
                cmd.Parameters.AddWithValue("@number", seat.SeatNumber);
                cmd.Parameters.AddWithValue("@price", seat.Price);
                cmd.Parameters.AddWithValue("@booked", seat.IsBooked);
                cmd.Parameters.AddWithValue("@specId", spectacle.Id);
                await cmd.ExecuteNonQueryAsync();
            }
        }

        private async Task UpdateSpectacleGenres(NpgsqlConnection connection, NpgsqlTransaction transaction, Spectacles spectacle)
        {
            // Удаляем старые жанры
            const string deleteSql = "DELETE FROM spectacle_genre WHERE id_spectacle = @specId";
            await using (var cmd = new NpgsqlCommand(deleteSql, connection, transaction))
            {
                cmd.Parameters.AddWithValue("@specId", spectacle.Id);
                await cmd.ExecuteNonQueryAsync();
            }

            // Проверяем существование жанров в БД
            const string checkGenreSql = "SELECT COUNT(1) FROM genre WHERE id_genre = @genreId";
            foreach (var genre in spectacle.Genres)
            {
                await using (var checkCmd = new NpgsqlCommand(checkGenreSql, connection, transaction))
                {
                    checkCmd.Parameters.AddWithValue("@genreId", genre.GenreId);
                    var exists = (long)(await checkCmd.ExecuteScalarAsync() ?? 0) > 0;

                    if (!exists)
                    {
                        throw new Exception($"Жанр с ID {genre.GenreId} не найден в БД");
                    }
                }

                // Добавляем связь
                const string insertSql = "INSERT INTO spectacle_genre (id_spectacle, id_genre) VALUES (@specId, @genreId)";
                foreach (var genres in spectacle.Genres.Where(g => g.GenreId > 0)) // Фильтруем некорректные ID
                {
                    await using var cmd = new NpgsqlCommand(insertSql, connection, transaction);
                    cmd.Parameters.AddWithValue("@specId", spectacle.Id);
                    cmd.Parameters.AddWithValue("@genreId", genre.GenreId);
                    await cmd.ExecuteNonQueryAsync();
                }
            }
        }


        public async Task<Spectacles> GetSpectacleById(int id)
        {
            using var connection = new NpgsqlConnection(_connectionString);
            await connection.OpenAsync();

            const string sql = @"
    SELECT s.id_spectacle, s.name_spectacle, s.quantity_actors, s.photo_spectacle, 
           s.premieredate, s.showtime,
           g.id_genre, g.name_genre,
           st.id_seat, st.number_seat, st.price, st.isbooked
    FROM spectacles s
    LEFT JOIN spectacle_genre sg ON s.id_spectacle = sg.id_spectacle
    LEFT JOIN genre g ON sg.id_genre = g.id_genre
    LEFT JOIN seat st ON s.id_spectacle = st.id_spectacle
    WHERE s.id_spectacle = @id_spectacle";

            await using var command = new NpgsqlCommand(sql, connection);
            command.Parameters.AddWithValue("@id_spectacle", id);

            await using var reader = await command.ExecuteReaderAsync();

            Spectacles spectacle = null;
            var genres = new List<SpectacleGenre>();
            var seats = new List<Seat>();

            while (await reader.ReadAsync())
            {
                if (spectacle == null)
                {
                    spectacle = new Spectacles
                    {
                        Id = reader.GetInt32(0),
                        Name = reader.GetString(1),
                        QuantityActors = reader.GetInt32(2),
                        Image = reader.IsDBNull(3) ? null : (byte[])reader[3],
                        PremiereDate = reader.IsDBNull(4) ? null : reader.GetDateTime(4),
                        ShowTime = reader.IsDBNull(5) ? null : reader.GetDateTime(5)
                    };
                }

                if (!reader.IsDBNull(6))
                {
                    genres.Add(new SpectacleGenre
                    {
                        Genre = new Genre
                        {
                            Id = reader.GetInt32(6),
                            Name = reader.GetString(7)
                        }
                    });
                }

                if (!reader.IsDBNull(8))
                {
                    seats.Add(new Seat
                    {
                        IdSeat = reader.GetInt32(8),
                        SeatNumber = reader.GetInt32(9),
                        Price = reader.GetDecimal(10),
                        IsBooked = reader.GetBoolean(11)
                    });
                }
            }

            if (spectacle == null)
            {
                throw new KeyNotFoundException($"Spectacle with id {id} not found");
            }

            spectacle.Genres = genres;
            spectacle.Seats = seats;

            return spectacle;
        }

        public async Task<List<Genre>> GetAllGenres()
        {
            using var connection = new NpgsqlConnection(_connectionString);
            await connection.OpenAsync();

            const string sql = "SELECT id_genre, name_genre FROM genre";
            await using var command = new NpgsqlCommand(sql, connection);
            await using var reader = await command.ExecuteReaderAsync();
            var genres = new List<Genre>();
            while (await reader.ReadAsync())
            {
                genres.Add(new Genre
                {
                    Id = reader.GetInt32(0),
                    Name = reader.GetString(1)
                });
            }
            return genres;
        }
        public async Task<List<Spectacles>> GetAllSpectacles()
        {
            using var connection = new NpgsqlConnection(_connectionString);
            await connection.OpenAsync();

            const string sql = @"
SELECT s.id_spectacle, s.name_spectacle, s.quantity_actors, s.photo_spectacle,
       s.premieredate, s.showtime,
       g.id_genre, g.name_genre,
       st.id_seat, st.number_seat, st.price, st.isbooked
FROM spectacles s
LEFT JOIN spectacle_genre sg ON s.id_spectacle = sg.id_spectacle
LEFT JOIN genre g ON sg.id_genre = g.id_genre
LEFT JOIN seat st ON s.id_spectacle = st.id_spectacle";

            await using var command = new NpgsqlCommand(sql, connection);
            await using var reader = await command.ExecuteReaderAsync();

            var spectacles = new Dictionary<int, Spectacles>();
            var genres = new Dictionary<int, List<SpectacleGenre>>();
            var seats = new Dictionary<int, List<Seat>>();

            while (await reader.ReadAsync())
            {
                var spectacleId = reader.GetInt32(0);

                if (!spectacles.ContainsKey(spectacleId))
                {
                    spectacles[spectacleId] = new Spectacles
                    {
                        Id = spectacleId,
                        Name = reader.GetString(1),
                        QuantityActors = reader.GetInt32(2),
                        Image = reader.IsDBNull(3) ? null : (byte[])reader[3],
                        PremiereDate = reader.IsDBNull(4) ? null : reader.GetDateTime(4),
                        ShowTime = reader.IsDBNull(5) ? null : reader.GetDateTime(5),
                        Genres = new List<SpectacleGenre>(),
                        Seats = new List<Seat>()
                    };
                }

                if (!reader.IsDBNull(6)) // Жанры
                {
                    var genreId = reader.GetInt32(6);
                    if (!genres.ContainsKey(spectacleId))
                    {
                        genres[spectacleId] = new List<SpectacleGenre>();
                    }

                    genres[spectacleId].Add(new SpectacleGenre
                    {
                        Genre = new Genre
                        {
                            Id = genreId,
                            Name = reader.GetString(7)
                        }
                    });
                }

                if (!reader.IsDBNull(8)) // Места
                {
                    var seatId = reader.GetInt32(8);
                    if (!seats.ContainsKey(spectacleId))
                    {
                        seats[spectacleId] = new List<Seat>();
                    }

                    seats[spectacleId].Add(new Seat
                    {
                        IdSeat = seatId,
                        SeatNumber = reader.GetInt32(9),
                        Price = reader.GetDecimal(10),
                        IsBooked = reader.GetBoolean(11)
                    });
                }
            }

            // Объединяем данные
            foreach (var spectacle in spectacles.Values)
            {
                if (genres.TryGetValue(spectacle.Id, out var spectacleGenres))
                {
                    spectacle.Genres = spectacleGenres;
                }

                if (seats.TryGetValue(spectacle.Id, out var spectacleSeats))
                {
                    spectacle.Seats = spectacleSeats;
                }
            }

            return spectacles.Values.ToList();
        }
        public async Task<Spectacles> AddSpectacleWithGenres(Spectacles spectacle, int seatCount, decimal seatPrice)
        {
            using var connection = new NpgsqlConnection(_connectionString);
            await connection.OpenAsync();
            using var transaction = await connection.BeginTransactionAsync();

            try
            {
                // 1. Добавляем спектакль
                const string sqlSpectacle = @"
        INSERT INTO spectacles (name_spectacle, quantity_actors, photo_spectacle, premieredate, showtime) 
        VALUES (@name, @quantity_actors, @image, @premieredate, @showtime)
        RETURNING id_spectacle";

                var spectacleId = await connection.ExecuteScalarAsync<int>(sqlSpectacle, new
                {
                    name = spectacle.Name,
                    quantity_actors = spectacle.QuantityActors,
                    image = spectacle.Image ?? (object)DBNull.Value,
                    premieredate = spectacle.PremiereDate ?? (object)DBNull.Value,
                    showtime = spectacle.ShowTime ?? (object)DBNull.Value
                }, transaction);

                // 2. Добавляем жанры
                if (spectacle.Genres?.Any() == true)
                {
                    const string sqlGenre = @"
                INSERT INTO spectacle_genre (id_spectacle, id_genre)
                VALUES (@spectacleId, @genreId)";

                    foreach (var genre in spectacle.Genres)
                    {
                        await connection.ExecuteAsync(sqlGenre, new
                        {
                            spectacleId,
                            genreId = genre.GenreId
                        }, transaction);
                    }
                }

                // 3. Добавляем места
                const string sqlSeat = @"
            INSERT INTO seat (number_seat, price, isbooked, id_spectacle) 
            VALUES (@number, @price, false, @spectacleId)";

                for (int i = 1; i <= seatCount; i++)
                {
                    await connection.ExecuteAsync(sqlSeat, new
                    {
                        number = i,
                        price = seatPrice,
                        spectacleId
                    }, transaction);
                }

                await transaction.CommitAsync();

                // 4. Получаем полный объект с жанрами
                return await GetSpectacleById(spectacleId);
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
        }
        public async Task LoadSpectacleData(int spectacleId)
        {
            try
            {
                const string connectionString = "Host=localhost;Port=5432;Database=Theatre;Username=postgres;Password=Kabinet21;";
                await using var connection = new NpgsqlConnection(connectionString);
                await connection.OpenAsync();

                const string sql = @"
    SELECT s.id_spectacle, s.name_spectacle, s.premieredate, 
           s.photo_spectacle, s.quantity_actors
    FROM spectacles s
    WHERE s.id_spectacle = @id";

                await using var command = new NpgsqlCommand(sql, connection);
                command.Parameters.AddWithValue("@id", spectacleId);

                await using var reader = await command.ExecuteReaderAsync();

                if (await reader.ReadAsync())
                {
                    Spectacle.Name = reader.GetString(1);
                    Spectacle.PremiereDate = reader.IsDBNull(2) ? null : reader.GetDateTime(2);
                    Spectacle.Image = reader.IsDBNull(3) ? null : (byte[])reader[3];
                    Spectacle.QuantityActors = reader.GetInt32(4);

                    Debug.WriteLine($"Загружено: {Spectacle.Name}, Дата: {Spectacle.PremiereDate}");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки данных: {ex.Message}");
            }
        }
        public async Task LoadSeatsAsync(int spectacleId)
        {
            try
            {
                // Если Spectacle еще не создан, создаем его
                Spectacle = Spectacle ?? new Spectacles();
                const string connectionString = "Host=localhost;Port=5432;Database=Theatre;Username=postgres;Password=Kabinet21;";
                await using var connection = new NpgsqlConnection(connectionString);
                await connection.OpenAsync();
                const string sqlSeats = @"
            SELECT id_seat, number_seat, price, isbooked, id_spectacle 
            FROM seat 
            WHERE id_spectacle = @id_spectacle";
                await using var command = new NpgsqlCommand(sqlSeats, connection);
                command.Parameters.AddWithValue("@id_spectacle", spectacleId);
                await using var reader = await command.ExecuteReaderAsync();

                // Инициализируем Seats, если их еще нет
                Spectacle.Seats = Spectacle.Seats ?? new List<Seat>();
                Spectacle.Seats.Clear();

                while (await reader.ReadAsync())
                {
                    var seat = new Seat
                    {
                        IdSeat = reader.GetInt32(0),
                        SeatNumber = reader.GetInt32(1),
                        Price = reader.GetDecimal(2),
                        IsBooked = reader.GetBoolean(3),
                        SpectacleId = reader.GetInt32(4),
                    };
                    Spectacle.Seats.Add(seat);
                }

                Spectacle.OnPropertyChanged(nameof(Spectacle.PriceInfo));
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка при загрузке мест: " + ex.Message);
            }
        }
    }
}
