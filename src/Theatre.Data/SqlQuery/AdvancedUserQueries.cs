using Npgsql;
using System;
using System.Threading.Tasks;

namespace Theatre.Data.SqlQuery
{
    public class AdvancedUserQueries : UserQuaries
    {
        public const string SqlCheckUserWithEmail = @"SELECT COUNT(*) FROM users WHERE login = @login OR email = @email";

        public override async Task<bool> CheckUser(string login, string password)
        {
            return await base.CheckUser(login, password);
        }

        public async Task<bool> CheckUserWithEmail(string login, string email)
        {
            using (var connection = new NpgsqlConnection(connectionString))
            {
                await connection.OpenAsync();

                using (var command = new NpgsqlCommand(SqlCheckUserWithEmail, connection))
                {
                    command.Parameters.AddWithValue("@login", login);
                    command.Parameters.AddWithValue("@email", email);

                    int count = Convert.ToInt32(await command.ExecuteScalarAsync());
                    return count > 0;
                }
            }
        }
    }
}