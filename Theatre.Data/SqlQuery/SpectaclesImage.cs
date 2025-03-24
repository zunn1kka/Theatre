using Microsoft.EntityFrameworkCore;
using System.Diagnostics;
using Theatre.Core.Models;
using Theatre.Data;
using System.IO;
using System.Drawing;
using Npgsql;
namespace Theatre.Data.SqlQuery
{
    public class SpectaclesImage : SqlQueries
    {
        public static async Task<List<(string SpectacleName, byte[] ImageData)>> GetImageFromDataBase()
        {
            var result = new List<(string, byte[])>();

            try
            {
                await using var connection = new NpgsqlConnection(connectionString);
                await connection.OpenAsync();
                await using var command = new NpgsqlCommand(SqlLoadImageFromDataBase, connection);
                await using var reader = await command.ExecuteReaderAsync();

                while (await reader.ReadAsync())
                {
                    var name = reader.GetString(0);
                    var imageData = reader.GetFieldValue<byte[]>(1);
                    result.Add((name, imageData));
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Ошибка при загрузке изображений: " + ex.Message);
            }

            return result;
        }
        public static async Task<List<Spectacles>> LoadSpectaclesAsync()
        {
            var spectacles = new List<Spectacles>();

            try
            {
                await using var connection = new NpgsqlConnection(connectionString);
                await connection.OpenAsync();

                // Загружаем спектакли
                await using var commandSpectacles = new NpgsqlCommand(SqlLoadSpectacles, connection);
                await using var readerSpectacles = await commandSpectacles.ExecuteReaderAsync();

                while (await readerSpectacles.ReadAsync())
                {
                    var spectacle = new Spectacles
                    {
                        Id = readerSpectacles.GetInt32(0),
                        Name = readerSpectacles.GetString(1),
                        QuantityActors = readerSpectacles.GetInt32(2),
                        Image = readerSpectacles.GetFieldValue<byte[]>(3),
                        Genres = new List<SpectacleGenre>(),
                        Seats = new List<Seat>()
                    };

                    spectacles.Add(spectacle);
                }

                // Загружаем жанры для каждого спектакля
                await using var commandGenres = new NpgsqlCommand(SqlGenres, connection);
                await using var readerGenres = await commandGenres.ExecuteReaderAsync();

                while (await readerGenres.ReadAsync())
                {
                    var genreId = readerGenres.GetInt32(0);
                    var spectacleId = readerGenres.GetInt32(1);

                    var spectacle = spectacles.FirstOrDefault(s => s.Id == spectacleId);
                    if (spectacle != null)
                    {
                        spectacle.Genres.Add(new SpectacleGenre
                        {
                            GenreId = genreId,
                            SpectacleId = spectacleId
                        });
                    }
                }

                // Загружаем места для каждого спектакля
                await using var commandSeats = new NpgsqlCommand(SqlSeats, connection);
                await using var readerSeats = await commandSeats.ExecuteReaderAsync();

                while (await readerSeats.ReadAsync())
                {
                    var seat = new Seat
                    {
                        SeatNumber = readerSeats.GetInt32(0),
                        Price = readerSeats.GetDecimal(1),
                        IsBooked = readerSeats.GetBoolean(2),
                        SpectacleId = readerSeats.GetInt32(3)
                    };

                    var spectacle = spectacles.FirstOrDefault(s => s.Id == seat.SpectacleId);
                    if (spectacle != null)
                    {
                        spectacle.Seats.Add(seat);
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Ошибка при загрузке спектаклей: " + ex.Message);
            }

            return spectacles;
        }

        //public static BitmapImage LoadImage(byte[] imageData)
        //{
        //    if (imageData == null || imageData.Length == 0)
        //    {
        //        Debug.WriteLine("Изображение не загружено или пустое.");
        //        return null;
        //    }

        //    var image = new BitmapImage();
        //    using (var stream = new MemoryStream(imageData))
        //    {
        //        stream.Position = 0;
        //        image.BeginInit();
        //        image.CacheOption = BitmapCacheOption.OnLoad;
        //        image.StreamSource = stream;
        //        image.EndInit();
        //    }
        //    Debug.WriteLine("Изображение успешно загружено.");
        //    return image;
        //}
    }
}

