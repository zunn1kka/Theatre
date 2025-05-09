using MimeKit;
using Npgsql;
using System.Text.RegularExpressions;
namespace Theatra.Datas.SqlQuery
{
    public class EmailServices
    {
        public static Regex EmailRegex = new(@"^[a-zA-Z0-9._%+-]+@[a-zA-Z0-9.-]+\.[a-zA-Z]{2,}$", RegexOptions.Compiled);
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
            Random random = new();
            return random.Next(1, 999999).ToString();
        }


        public async Task SaveConfirmationCodeAsync(string email, string code)
        {
            string connectionString = "Host=localhost;Port=5432;Database=Theatre;Username=postgres;Password=Kabinet21;";
            using var connection = new NpgsqlConnection(connectionString);
            string InsertConfirmationCode = @"INSERT INTO confirmationcodes (code, email, expires_in)
                                         VALUES (@code, @email, @expires_in)";
            await connection.OpenAsync();
            using var command = new NpgsqlCommand(InsertConfirmationCode, connection);
            command.Parameters.AddWithValue("@code", code);
            command.Parameters.AddWithValue("@email", email);
            command.Parameters.AddWithValue("@expires_in", DateTime.UtcNow.AddMinutes(10));

            await command.ExecuteNonQueryAsync();
        }


        public async Task SendConfirmationEmailAsync(string email, string login, string code)
        {
            var message = new MimeMessage();
            message.From.Add(new MailboxAddress("TheatreWatch", "denis_demidov_07@mail.ru"));
            message.To.Add(new MailboxAddress("", email));
            message.Subject = "Код подтверждения";
            message.Body = new TextPart("html")
            {
                Text = $@"
    <!DOCTYPE html>
    <html lang='ru'>
    <head>
        <meta charset='UTF-8'>
        <meta name='viewport' content='width=device-width, initial-scale=1.0'>
        <title>Код подтверждения</title>
        <style>
            body {{
                font-family: 'Arial', sans-serif;
                line-height: 1.6;
                color: #333;
                max-width: 600px;
                margin: 0 auto;
                padding: 20px;
                background-color: #f9f9f9;
            }}
            .container {{
                background-color: #ffffff;
                border-radius: 8px;
                padding: 30px;
                box-shadow: 0 4px 6px rgba(0, 0, 0, 0.1);
            }}
            .header {{
                text-align: center;
                margin-bottom: 25px;
            }}
            .logo {{
                max-width: 150px;
                margin-bottom: 15px;
            }}
            .code-container {{
                background-color: #f0f7ff;
                border-radius: 6px;
                padding: 15px;
                text-align: center;
                margin: 25px 0;
                border: 1px dashed #4a90e2;
            }}
            .code {{
                font-size: 28px;
                font-weight: bold;
                letter-spacing: 3px;
                color: #2c3e50;
            }}
            .footer {{
                margin-top: 30px;
                font-size: 12px;
                color: #7f8c8d;
                text-align: center;
            }}
            .button {{
                display: inline-block;
                padding: 12px 24px;
                background-color: #4a90e2;
                color: white;
                text-decoration: none;
                border-radius: 4px;
                font-weight: bold;
                margin-top: 15px;
            }}
        </style>
    </head>
    <body>
        <div class='container'>
            <div class='header'>
                <!-- Вставьте URL вашего логотипа -->
                <img src='https://img.icons8.com/?size=48&id=h5VJ2aUGETKL&format=png' alt='TheatreWatch' class='logo'>
                <h2>Подтверждение регистрации</h2>
            </div>
            
            <p>Уважаемый(ая) {login},</p>
            <p>Благодарим вас за регистрацию в TheatreWatch. Для завершения процесса регистрации, пожалуйста, используйте следующий код подтверждения:</p>
            
            <div class='code-container'>
                <div class='code'>{code}</div>
            </div>
            
            <p>Этот код будет действителен в течение 10 минут. Если вы не запрашивали это письмо, пожалуйста, проигнорируйте его.</p>
            
            <div class='footer'>
                <p>© {DateTime.Now.Year} TheatreWatch. Все права защищены.</p>
                <p>Это письмо отправлено автоматически, пожалуйста, не отвечайте на него.</p>
            </div>
        </div>
    </body>
    </html>"
            };
            using var client = new MailKit.Net.Smtp.SmtpClient();
            await client.ConnectAsync("smtp.mail.ru", 465, true);
            await client.AuthenticateAsync("denis_demidov_07@mail.ru", "nErJ8B0VwPMYAZNNp4CE");
            await client.SendAsync(message);
            await client.DisconnectAsync(true);
        } 
        


        public async Task<bool> VerifyConfirmationCodeAsync(string email, string code, DateTime now)
        {
            string connectionString = "Host=localhost;Port=5432;Database=Theatre;Username=postgres;Password=Kabinet21;";

            using var connection = new NpgsqlConnection(connectionString);
            string SqlVerifyConfirmationCode = @"SELECT COUNT(*) FROM confirmationcodes
                                        WHERE email = @email AND code = @code AND expires_in > @now";
            await connection.OpenAsync();
            using var command = new NpgsqlCommand(SqlVerifyConfirmationCode, connection);
            command.Parameters.AddWithValue("@email", email);
            command.Parameters.AddWithValue("@code", code);
            command.Parameters.AddWithValue("@now", now);

            int count = Convert.ToInt32(await command.ExecuteScalarAsync());
            return count > 0;
        }
    }
}
