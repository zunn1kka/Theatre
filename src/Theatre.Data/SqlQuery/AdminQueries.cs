using Npgsql;
using System;
using System.Threading.Tasks;

namespace Theatre.Data.SqlQuery
{
    public class AdminQueries
    {
        public const string connectionString = "Host=localhost;Port=5432;Database=Users;Username=postgres;Password=Mailgame5250580;";

        public const string CheckAdminKey = @"SELECT COUNT(*) FROM adminKey WHERE code = $1";

        public static async Task<bool> CheckAdmin(string code)
        {
            using (var connection = new NpgsqlConnection(connectionString))
            {
                await connection.OpenAsync();

                using (var command = new NpgsqlCommand(CheckAdminKey, connection))
                {
                    // Указываем параметр с именем $1
                    command.Parameters.AddWithValue("$1", code);

                    int count = Convert.ToInt32(await command.ExecuteScalarAsync());
                    return count > 0;
                }
            }
        }
    }
}