using Microsoft.EntityFrameworkCore;
using Npgsql;
using System;
using System.Threading.Tasks;
using Theatre.Core.Models;
using Theatre.Data;

namespace Theatre.Data.SqlQuery
{
    public class AdminQueries : SqlQueries
    {
        public static async Task<bool> CheckAdmin(string code)
        {
            try
            {
                await using var connection = new NpgsqlConnection(connectionString);
                await connection.OpenAsync();

                await using var command = new NpgsqlCommand(SqlCheckAdminKey, connection);
                command.Parameters.AddWithValue("@code", code);

                var result = await command.ExecuteScalarAsync();
                return Convert.ToInt32(result) > 0;
            }
            catch (Exception ex)
            {
                Console.WriteLine("Ошибка при проверке администратора: " + ex.Message);
                return false;
            }
        }
    }
}