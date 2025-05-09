using Npgsql;
using Theatre.Core.Models;
using Theatre.Core.Services;

namespace Theatre.Data.SqlQuery
{
    public class AuthService 
    {
        private readonly string _connectionString;
        private readonly PasswordHasherService _passwordHasher;

        public AuthService(string connectionString)
        {
            _connectionString = connectionString;
            _passwordHasher = new PasswordHasherService();
        }

        public async Task<User?> RegisterAsync(string login, string email, string password)
        {
            var passwordHash = _passwordHasher.HashPassword(password);

            using var connection = new NpgsqlConnection(_connectionString);
            await connection.OpenAsync();

            const string sql = @"
                INSERT INTO users (login, email, password, id_type_user, balance)
                VALUES (@login, @email, @password, 1, 1000)
                RETURNING id_user";

            using var command = new NpgsqlCommand(sql, connection);
            command.Parameters.AddWithValue("@login", login);
            command.Parameters.AddWithValue("@email", email);
            command.Parameters.AddWithValue("@password", passwordHash);

            try
            {
                var userId = (int?)await command.ExecuteScalarAsync();
                if (userId.HasValue)
                {
                    return new User
                    {
                        Id = userId.Value,
                        Login = login,
                        Email = email,
                        TypeId = 1,
                        Balance = 1000
                    };
                }
                return null;
            }
            catch (PostgresException ex) when (ex.SqlState == "23505")
            {
                // Дупликат логина или почты
                return null;
            }
        }

        public async Task<User?> AuthenticateAsync(string username, string password)
        {
            using var connection = new NpgsqlConnection(_connectionString);
            await connection.OpenAsync();

            const string sql = @"
                SELECT id_user, login, email, password, id_type_user, balance
                FROM users
                WHERE login = @login";

            using var command = new NpgsqlCommand(sql, connection);
            command.Parameters.AddWithValue("@login", username);

            using var reader = await command.ExecuteReaderAsync();
            if (await reader.ReadAsync())
            {
                var user = new User
                {
                    Id = reader.GetInt32(0),
                    Login = reader.GetString(1),
                    Email = reader.GetString(2),
                    Password = reader.GetString(3),
                    TypeId = reader.GetInt32(4),
                    Balance = reader.GetDecimal(5)
                };

                if (_passwordHasher.VerifyPassword(user.Password, password))
                {
                    return user;
                }
            }
            return null;
        }
    }
}