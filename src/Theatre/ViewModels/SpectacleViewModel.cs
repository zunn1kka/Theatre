using Npgsql;
using System;
using System.Windows;
using System.Windows.Input;
using Theatre.Core.Models;
using PdfSharp.Pdf;
using PdfSharp.Drawing;
using Theatre.Data.SqlQuery;
using System.Diagnostics;
using System.IO;
using Theatre.Commands;
using System.Threading.Tasks;
using System.Collections.Generic;
using Theatra.Datas.SqlQuery;

namespace Theatre.ViewModels
{
    public class SpectacleViewModel : BaseViewModel
    {
        public Spectacles Spectacle { get; set; }
        public User User { get; set; }
        public UserQuaries userQuaries = new();
        public ICommand BookSeatCommand { get; set; }
        public SpectacleViewModel(Spectacles spectacle, string userLogin)
        {
            Spectacle = spectacle;
            BookSeatCommand = new RelayCommand(BookSeat);
            LoadUserAsync(userLogin);
        }
        private async void LoadUserAsync(string login)
        {
            try
            {
                User = await userQuaries.GetUserByLoginAsync(login);
                if (User == null)
                {
                    MessageBox.Show("Пользователь не найден!");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка при загрузке пользователя: " + ex.Message);
            }
        }
        private async void BookSeat(object parameter)
        {
            if (parameter is Seat seat)
            {
                try
                {
                    if (User == null)
                    {
                        MessageBox.Show("Пользователь не загружен!");
                        return;
                    }
                    const string connectionString = "Host=localhost;Port=5432;Database=Theatre;Username=postgres;Password=Kabinet21;";
                    if (User.Balance < seat.Price)
                    {
                        MessageBox.Show("Недостаточно средств");
                        return;
                    }
                    User.Balance -= seat.Price;
                    const string updateBalanceSql = @"
                        UPDATE users
                        SET balance = @balance
                        WHERE id_user = @userId";
                    await using (var connectionS = new NpgsqlConnection(connectionString))
                    {
                        await connectionS.OpenAsync();
                        await using var command = new NpgsqlCommand(updateBalanceSql, connectionS);
                        command.Parameters.AddWithValue("@balance", User.Balance);
                        command.Parameters.AddWithValue("@userId", User.Id);
                        await command.ExecuteNonQueryAsync();
                    }
                    await using var connection = new NpgsqlConnection(connectionString);
                    await connection.OpenAsync();

                    // Проверяем, что место еще не забронировано
                    const string checkSeatSql = @"
                SELECT isbooked 
                FROM seat 
                WHERE id_seat = @id_seat";

                    await using var checkSeatCommand = new NpgsqlCommand(checkSeatSql, connection);
                    checkSeatCommand.Parameters.AddWithValue("@id_seat", seat.IdSeat);

                    var result = await checkSeatCommand.ExecuteScalarAsync();

                    if (result == null)
                    {
                        MessageBox.Show("Место не найдено в базе данных.");
                        return;
                    }

                    var isBooked = (bool)result;

                    if (isBooked)
                    {
                        MessageBox.Show("Место уже забронировано!");
                        return;
                    }

                    // Бронируем место
                    const string bookSeatSql = @"
                UPDATE seat 
                SET isbooked = true 
                WHERE id_seat = @id_seat";

                    await using var bookSeatCommand = new NpgsqlCommand(bookSeatSql, connection);
                    bookSeatCommand.Parameters.AddWithValue("@id_seat", seat.IdSeat);

                    await bookSeatCommand.ExecuteNonQueryAsync();

                    seat.IsBooked = true;

                    // Добавляем запись в таблицу ticket
                    const string addTicketSql = @"
                INSERT INTO ticket (number_seat, id_user, bookingtime) 
                VALUES (@number_seat, @id_user, @bookingtime)";

                    await using var addTicketCommand = new NpgsqlCommand(addTicketSql, connection);
                    addTicketCommand.Parameters.AddWithValue("@number_seat", seat.IdSeat);
                    addTicketCommand.Parameters.AddWithValue("@id_user", User.Id);
                    addTicketCommand.Parameters.AddWithValue("@bookingtime", DateTime.Now);

                    await addTicketCommand.ExecuteNonQueryAsync();

                    // Создаем PDF-чек
                    CreatePdfReceipt(seat);

                    MessageBox.Show($"Место {seat.SeatNumber} успешно забронировано! Новый баланс: {User.Balance:C}");
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Ошибка при бронировании места: " + ex.Message);
                }
            }
        }



        private void CreatePdfReceipt(Seat seat)
        {
            try
            {
                var document = new PdfDocument();
                var page = document.AddPage();
                var gfx = XGraphics.FromPdfPage(page);
                var font = new XFont("Arial", 12);
                gfx.DrawString("Чек на бронирование места", font, XBrushes.Black, new XPoint(50, 50));
                gfx.DrawString($"Спектакль: {Spectacle.Name}", font, XBrushes.Black, new XPoint(50, 70));
                gfx.DrawString($"Место: {seat.SeatNumber}", font, XBrushes.Black, new XPoint(50, 90));
                gfx.DrawString($"Цена: {seat.Price}", font, XBrushes.Black, new XPoint(50, 110));
                string premiereDate = Spectacle.PremiereDate.HasValue
                    ? Spectacle.PremiereDate.Value.ToString("dd.MM.yyyy")
            : "Дата не указана";
                gfx.DrawString($"Дата премьеры: {premiereDate}", font, XBrushes.Black, new XPoint(50, 130));
                // Информация о покупателе
                gfx.DrawString($"Покупатель: {User?.Login ?? "Гость"}", font, XBrushes.Black, new XPoint(50, 160));
                gfx.DrawString($"Дата бронирования: {DateTime.Now:dd.MM.yyyy HH:mm}", font, XBrushes.Black, new XPoint(50, 180));
                // Сохраняем в папку "Документы"
                var documentsPath = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
                var fileName = Path.Combine(documentsPath, $"Билет_{Spectacle.Name}_{seat.SeatNumber}.pdf");

                document.Save(fileName);
                MessageBox.Show($"Чек сохранен в файл: {fileName}");
                Process.Start("explorer.exe", Path.GetDirectoryName(fileName));
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка при создании PDF-чека: " + ex.Message);
            }
        }
    }
}
