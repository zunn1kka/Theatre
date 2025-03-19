using MimeKit;
using Npgsql;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Mail;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using MailKit.Net.Smtp;
namespace Theatre.Business.Services
{
    public class EmailServices
    {
        public static Regex EmailRegex = new Regex(@"^[a-zA-Z0-9._%+-]+@[a-zA-Z0-9.-]+\.[a-zA-Z]{2,}$", RegexOptions.Compiled);
        public bool IsValidEmail(string email)
        {
            if (string.IsNullOrWhiteSpace(email))
            {
                return false;
            }
            return EmailRegex.IsMatch(email);
        }
        public string GenerateConfirmationCode()
        {
            Random random = new Random();
            return random.Next(1, 999999).ToString();
        }


        public async Task SaveConfirmationCodeAsync(string email, string code)
        {
            string connectionString = "Host=localhost;Port=5432;Database=Users;Username=postgres;Password=Mailgame5250580;";
            using (var connection = new NpgsqlConnection(connectionString))
            {
                 string InsertConfirmationCode = @"INSERT INTO confirmationcodes (code, email, expires_in)
                                         VALUES (@code, @email, @expires_in)";
        await connection.OpenAsync();
                using (var command = new NpgsqlCommand(InsertConfirmationCode, connection))
                {
                    command.Parameters.AddWithValue("@code", code);
                    command.Parameters.AddWithValue("@email", email);
                    command.Parameters.AddWithValue("@expires_in", DateTime.UtcNow.AddMinutes(10));

                    await command.ExecuteNonQueryAsync();
                }
            }
        }


        public async Task SendConfirmationEmailAsync(string email, string code)
        {
            var message = new MimeMessage();
            message.From.Add(new MailboxAddress("Денис", "denis_demidov_07@mail.ru"));
            message.To.Add(new MailboxAddress("", email));
            message.Subject = "Код подтверждения";
            message.Body = new TextPart("plain")
            {
                Text = $"Ваш код подтверждения: {code}"
            };
            using (var client = new MailKit.Net.Smtp.SmtpClient())
            {
                await client.ConnectAsync("smtp.mail.ru", 465, true);
                await client.AuthenticateAsync("denis_demidov_07@mail.ru", "d2rb7AhCrD7CmaxVrZM5");
                await client.SendAsync(message);
                await client.DisconnectAsync(true);
            }
            using (var client = new MailKit.Net.Smtp.SmtpClient())
            {
                await client.ConnectAsync("smtp.yandex.com", 465, true);
                await client.AuthenticateAsync("denis_demidov_07@mail.ru", "d2rb7AhCrD7CmaxVrZM5");
                await client.SendAsync(message);
                await client.DisconnectAsync(true);
            }
            using (var client = new MailKit.Net.Smtp.SmtpClient())
            {
                await client.ConnectAsync("smtp.gmail.com", 465, true);
                await client.AuthenticateAsync("denis_demidov_07@mail.ru", "d2rb7AhCrD7CmaxVrZM5");
                await client.SendAsync(message);
                await client.DisconnectAsync(true);
            }
        }


        public async Task<bool> VerifyConfirmationCodeAsync(string email, string code, DateTime now)
        {
            string connectionString = "Host=localhost;Port=5432;Database=Users;Username=postgres;Password=Mailgame5250580;";

            using (var connection = new NpgsqlConnection(connectionString))
            {
                string SqlVerifyConfirmationCode = @"SELECT COUNT(*) FROM confirmationcodes
                                        WHERE email = @email AND code = @code AND expires_in > @now";
                await connection.OpenAsync();
                using (var command = new NpgsqlCommand(SqlVerifyConfirmationCode, connection))
                {
                    command.Parameters.AddWithValue("@email",email);
                    command.Parameters.AddWithValue("@code",code);
                    command.Parameters.AddWithValue("@now",now);

                    int count = Convert.ToInt32(await command.ExecuteScalarAsync());
                    return count > 0;
                }
            }
        }
    }
}
