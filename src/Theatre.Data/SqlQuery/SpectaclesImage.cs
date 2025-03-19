using Npgsql;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;
using Theatre.Core.Models;

namespace Theatre.Data.SqlQuery
{
    public class SpectaclesImage
    {
        public const string connectionString = "Host=localhost;Port=5432;Database=Users;Username=postgres;Password=Mailgame5250580;";

        public const string SqlLoadImageFromDataBase = @"SELECT name_spectacle, photo_spectacle FROM spectacle";

        public const string SqlLoadSpectacles = @"SELECT id_spectacle, name_spectacle, quantity_actors, photo_spectacle FROM spectacle";

        public const string SqlLoadGenre = @"SELECT id_genre, name_genre FROM genre";

        public const string SqlLoadSpectacleGenre = @"SELECT id_genre, id_spectacle FROM spectacle_genre";

        public static async Task<List<(string SpectacleName, byte[] ImageData)>> GetImageFromDataBase()
        {
            var images = new List<(string, byte[])>();
            using (var connection = new NpgsqlConnection(connectionString))
            {

                await connection.OpenAsync();
                using (var command = new NpgsqlCommand(SqlLoadImageFromDataBase, connection))
                {
                    using (var reader = await command.ExecuteReaderAsync())
                    {
                        while (await reader.ReadAsync())
                        {
                            string spectacleName = reader.GetString(0);
                            byte[] imageSpectacle = (byte[])reader["photo_spectacle"];
                            images.Add((spectacleName, imageSpectacle));
                        }
                    }
                }
            }
            return images;
        }
        public static async Task<List<Spectacle>> LoadSpectaclesAsync()
        {
            var spectacles = new List<Spectacle>();
            var spectacleGenres = new List<SpectacleGenre>();
            var genres = new List<Genre>();

            using (var connection = new NpgsqlConnection(connectionString))
            {
                await connection.OpenAsync();

                // Загрузка спектаклей
                using (var command = new NpgsqlCommand(SqlLoadSpectacles, connection))
                {
                    using (var reader = await command.ExecuteReaderAsync())
                    {
                        while (await reader.ReadAsync())
                        {
                            byte[] imageData = reader["photo_spectacle"] as byte[];
                            var spectacle = new Spectacle
                            {
                                Id = reader.GetInt32(0),
                                Name = reader.GetString(1),
                                QuantityActors = reader.GetInt32(2),
                                Image = LoadImage(imageData)
                            };
                            spectacles.Add(spectacle);
                        }
                    }
                }

                // Загрузка жанров
                using (var command = new NpgsqlCommand(SqlLoadGenre, connection))
                {
                    using (var reader = await command.ExecuteReaderAsync())
                    {
                        while (await reader.ReadAsync())
                        {
                            var genre = new Genre
                            {
                                Id = reader.GetInt32(0),
                                Name = reader.GetString(1)
                            };
                            genres.Add(genre);
                        }
                    }
                }

                // Загрузка связей спектаклей и жанров
                using (var command = new NpgsqlCommand(SqlLoadSpectacleGenre, connection))
                {
                    using (var reader = await command.ExecuteReaderAsync())
                    {
                        while (await reader.ReadAsync())
                        {
                            var spectacleGenre = new SpectacleGenre
                            {
                                SpectacleId = reader.GetInt32(0),
                                GenreId = reader.GetInt32(1)
                            };
                            spectacleGenres.Add(spectacleGenre);
                        }
                    }
                }

                // Связывание спектаклей и жанров
                foreach (var sg in spectacleGenres)
                {
                    var spectacle = spectacles.FirstOrDefault(s => s.Id == sg.SpectacleId);
                    var genre = genres.FirstOrDefault(g => g.Id == sg.GenreId);
                    if (spectacle != null && genre != null)
                    {
                        spectacle.Genres.Add(genre);
                    }
                }
            }

            return spectacles;
        }

        public static BitmapImage LoadImage(byte[] imageData)
        {
            if (imageData == null || imageData.Length == 0)
            {
                Debug.WriteLine("Изображение не загружено или пустое.");
                return null;
            }

            var image = new BitmapImage();
            using (var stream = new MemoryStream(imageData))
            {
                stream.Position = 0;
                image.BeginInit();
                image.CacheOption = BitmapCacheOption.OnLoad;
                image.StreamSource = stream;
                image.EndInit();
            }
            Debug.WriteLine("Изображение успешно загружено.");
            return image;
        }
    }
}
