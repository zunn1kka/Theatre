using System;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using Npgsql;

namespace Theatre.Data.SqlQuery
{
    public class UserQuaries
    {
        public const string connectionString = "Host=localhost;Port=5432;Database=Users;Username=postgres;Password=Mailgame5250580;";
        public const string SqlInsertUser = @"INSERT INTO users(login, password, email, id_type_user) 
                            VALUES(@login, @password, @email, @Id_type_user)";

        public const string SqlCheckUser = @"SELECT COUNT(*) FROM users WHERE login = @login AND password = @password";

        public static async Task SaveUserAsync(string login, string password, string email, int typeUser)
        {
            using (var connection = new NpgsqlConnection(connectionString))
            {
                await connection.OpenAsync();

                using (var command = new NpgsqlCommand(SqlInsertUser, connection))
                {
                    command.Parameters.AddWithValue("@login", login);
                    command.Parameters.AddWithValue("@password", password);
                    command.Parameters.AddWithValue("@email", email);
                    command.Parameters.AddWithValue("@Id_type_user", typeUser);

                    await command.ExecuteNonQueryAsync();
                }
            }
        }

        public static string HashPassword(string password)
        {
            using (var sha256 = SHA256.Create())
            {
                var hashed = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
                return BitConverter.ToString(hashed).Replace("-", "").ToLower();
            }
        }

        public virtual async Task<bool> CheckUser(string login, string password)
        {
            using (var connection = new NpgsqlConnection(connectionString))
            {
                await connection.OpenAsync();

                using (var command = new NpgsqlCommand(SqlCheckUser, connection))
                {
                    // Указываем параметры с именами $1 и $2
                    command.Parameters.AddWithValue("@login", login);
                    command.Parameters.AddWithValue("@password",password);

                    int count = Convert.ToInt32(await command.ExecuteScalarAsync());
                    return count > 0;
                }
            }
        }
    }
}