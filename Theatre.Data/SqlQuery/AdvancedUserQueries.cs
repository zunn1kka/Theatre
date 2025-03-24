using Microsoft.EntityFrameworkCore;
using Npgsql;
using System;
using System.Threading.Tasks;
using Theatre.Data;

namespace Theatre.Data.SqlQuery
{
    public class AdvancedUserQueries : SqlQueries
    {
        public  async Task<bool> CheckUserWithEmail(string login, string email)
        {
            try
            {
                await using var connection = new NpgsqlConnection(connectionString);
                await connection.OpenAsync();

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
    }
}