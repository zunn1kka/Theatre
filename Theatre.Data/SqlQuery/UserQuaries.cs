using System.Security.Cryptography;
using System.Text;
using Npgsql;
using Theatre.Core.Models;

namespace Theatre.Data.SqlQuery
{
    public class UserQuaries : SqlQueries
    {
        public static async Task SaveUserAsync(string login, string password, string email, int typeUser)
        {
            try
            {
                // Создаем подключение к базе данных
                await using var connection = new NpgsqlConnection(connectionString);
                await connection.OpenAsync();


                // Создаем команду с параметрами
                await using var command = new NpgsqlCommand(SqlInsertUser, connection);
                command.Parameters.AddWithValue("@login", login);
                command.Parameters.AddWithValue("@password", password);
                command.Parameters.AddWithValue("@email", email);
                command.Parameters.AddWithValue("@Id_type_user", typeUser);

                // Выполняем команду
                int rowsAffected = await command.ExecuteNonQueryAsync();

                // Проверяем, была ли вставка успешной
                if (rowsAffected > 0)
                {
                    Console.WriteLine("Пользователь успешно сохранен.");
                }
                else
                {
                    Console.WriteLine("Не удалось сохранить пользователя.");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Ошибка: " + ex.Message);
                if (ex.InnerException != null)
                {
                    Console.WriteLine("Внутреннее исключение: " + ex.InnerException.Message);
                }
            }
        }
        public  string HashPassword(string password)
        {
            using var sha256 = SHA256.Create();
            var hashed = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
            return BitConverter.ToString(hashed).Replace("-", "").ToLower();
        }

        public static async Task<bool> CheckUser(string login, string password)
        {
            try
            {
                await using var connection = new NpgsqlConnection(connectionString);
                await connection.OpenAsync();

                const string sql = @"SELECT COUNT(*) FROM users WHERE login = @login AND password = @password";
                await using var command = new NpgsqlCommand(sql, connection);
                command.Parameters.AddWithValue("@login", login);
                command.Parameters.AddWithValue("@password", password);

                var result = await command.ExecuteScalarAsync();
                return Convert.ToInt32(result) > 0;
            }
            catch (Exception ex)
            {
                Console.WriteLine("Ошибка при проверке пользователя: " + ex.Message);
                return false;
            }
        }
        public async Task<User> GetUserByLoginAsync(string login)
        {
            await using var connection = new NpgsqlConnection(connectionString);
            await connection.OpenAsync();
            await using var command = new NpgsqlCommand(SqlGetUser, connection);
            command.Parameters.AddWithValue("@login", login);

            await using var reader = await command.ExecuteReaderAsync();
            if (await reader.ReadAsync())
            {
                return new User
                {
                    Id = reader.GetInt32(0),
                    Login = reader.GetString(1),
                    Password = reader.GetString(2),
                    Email = reader.GetString(3),
                    TypeId = reader.GetInt32(4)
                };
            }

            return null;
        }
        public async Task<UserData> GetUserForProfileAsync(string login)
        {
            await using var connection = new NpgsqlConnection(connectionString);
            await connection.OpenAsync();
            await using var command = new NpgsqlCommand(SqlGetUser, connection);
            command.Parameters.AddWithValue("@login", login);

            await using var reader = await command.ExecuteReaderAsync();
            if (await reader.ReadAsync())
            {
                return new UserData
                {
                    Id = reader.GetInt32(0),
                    Login = reader.GetString(1),
                    Password = reader.GetString(2),
                    Email = reader.GetString(3),
                    TypeId = reader.GetInt32(4)
                };
            }

            return null;
        }
    }
}