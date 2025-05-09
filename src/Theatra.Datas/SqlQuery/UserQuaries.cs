using Npgsql;
using System.Windows;
using Theatre.Core.Interfaces;
using Theatre.Core.Models;
namespace Theatre.Data.SqlQuery
{
    public class UserQuaries : IUserService
    {
        private readonly string _connectionString;
        const string connectionString = "Host=localhost;Port=5432;Database=Theatre;Username=postgres;Password=Kabinet21;";
        public User User { get; set; }
        public UserQuaries()
        {

        }
        public UserQuaries(string connectionString)
        {
            _connectionString = connectionString;
           
        }
        public async Task<User?> GetUserByLoginAsync(string login)
        {
            await using var connection = new NpgsqlConnection(connectionString);
            await connection.OpenAsync();
            const string SqlGetUser = @"SELECT id_user, login, password, email, id_type_user, balance FROM users WHERE login = @login";
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
                    TypeId = reader.GetInt32(4),
                    Balance = reader.GetDecimal(5)
                };
            }

            return null;
        }
        public async Task<UserData> GetUserForProfileAsync(string login)
        {
            var user = await GetUserByLoginAsync(login);
            return user != null ? new UserData
            {
                Id = user.Id,
                Login = user.Login,
                Email = user.Email,
                TypeId = user.TypeId,
                Balance = user.Balance
            } : null;
        }
        public async Task<bool> CheckUserWithEmail(string login, string email)
        {
            try
            {
                await using var connection = new NpgsqlConnection(connectionString);
                await connection.OpenAsync();
                const string SqlCheckUserWithEmail = @"SELECT COUNT(*) FROM users WHERE login = @login OR email = @email";
                await using var command = new NpgsqlCommand(SqlCheckUserWithEmail, connection);
                command.Parameters.AddWithValue("@login", login);
                command.Parameters.AddWithValue("@email", email);

                var result = await command.ExecuteScalarAsync();
                return Convert.ToInt32(result) > 0;
            }
            catch (Exception ex)
            {
                Console.WriteLine("Ошибка при проверке пользователя: " + ex.Message);
                return false;
            }
        }
        public async Task<User> GetUserById(int userId)
        {
            using var connection = new NpgsqlConnection(connectionString);
            await connection.OpenAsync();

            const string sql = "SELECT id_user, login, balance FROM users WHERE id_user = @id";

            await using var command = new NpgsqlCommand(sql, connection);
            command.Parameters.AddWithValue("@id", userId);

            await using var reader = await command.ExecuteReaderAsync();

            if (await reader.ReadAsync())
            {
                return new User
                {
                    Id = reader.GetInt32(0),
                    Login = reader.GetString(1),
                    Balance = reader.GetDecimal(2)
                };
            }

            return null;
        }
        public async Task<decimal> GetBalance(int userId)
        {
            using var connection = new NpgsqlConnection(connectionString);
            await connection.OpenAsync();

            const string sql = "SELECT balance FROM users WHERE id_user = @id";

            await using var command = new NpgsqlCommand(sql, connection);
            command.Parameters.AddWithValue("@id", userId);

            var result = await command.ExecuteScalarAsync();
            return result != null ? Convert.ToDecimal(result) : 0;
        }

        public async Task AddToBalance(int userId, decimal amount)
        {
            using var connection = new NpgsqlConnection(connectionString);
            await connection.OpenAsync();

            await using var transaction = await connection.BeginTransactionAsync();

            try
            {
                // Проверяем текущий баланс
                var currentBalance = await GetBalance(userId);

                // Обновляем баланс
                const string sql = @"
                UPDATE users 
                SET balance = @newBalance 
                WHERE id_user = @id";

                await using var command = new NpgsqlCommand(sql, connection, transaction);
                command.Parameters.AddWithValue("@newBalance", currentBalance + amount);
                command.Parameters.AddWithValue("@id", userId);

                await command.ExecuteNonQueryAsync();
                await transaction.CommitAsync();
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
        }
        public void LoadUserData()
        {
            try
            {
                const string connectionString = "Host=localhost;Port=5432;Database=Theatre;Username=postgres;Password=Kabinet21;";
                using var connection = new NpgsqlConnection(connectionString);
                connection.Open();

                const string sql = @"
                    SELECT login, email
                    FROM users 
                    WHERE id_user = @id_user";
                using var command = new NpgsqlCommand(sql, connection);
                command.Parameters.AddWithValue("@id_user", User.Id);

                using var reader = command.ExecuteReader();
                if (reader.Read())
                {
                    User.Login = reader.GetString(0);
                    User.Email = reader.GetString(1);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка при загрузке данных пользователя: " + ex.Message);
            }
        }
    }
}