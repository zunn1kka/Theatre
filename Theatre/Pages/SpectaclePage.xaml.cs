using Npgsql;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Theatre.Core.Models;
using Theatre.ViewModels;

namespace Theatre.Pages
{
    /// <summary>
    /// Логика взаимодействия для SpectaclePage.xaml
    /// </summary>
    public partial class SpectaclePage : Page
    {
        private Spectacles Spectacle { get; set; }
        public SpectaclePage(Spectacles spectacle, string userLogin)
        {
            InitializeComponent();
            Spectacle = spectacle;
            var viewModel = new SpectacleViewModel(spectacle, userLogin);
            DataContext = viewModel;
            LoadSeatsAsync(spectacle.Id);
            //Debug.WriteLine($"SpectaclePage created for: {spectacle.Name}");
        }

        private void BackButton_Click(object sender, RoutedEventArgs e)
        {
            Debug.WriteLine($"NavigationService is null: {NavigationService == null}");
            NavigationService?.GoBack();
        }
        private async Task LoadSeatsAsync(int spectacleId)
        {
            try
            {
                const string connectionString = "Host=localhost;Port=5432;Database=Users;Username=postgres;Password=Mailgame5250580;";
                await using var connection = new NpgsqlConnection(connectionString);
                await connection.OpenAsync();
                const string sqlSeats = @"
                    SELECT id_seat, number_seat, price, isbooked, id_spectacle 
                    FROM seat 
                    WHERE id_spectacle = @id_spectacle";
                await using var command = new NpgsqlCommand(sqlSeats, connection);
                command.Parameters.AddWithValue("@id_spectacle", spectacleId);
                await using var reader = await command.ExecuteReaderAsync();
                Spectacle.Seats.Clear();
                while (await reader.ReadAsync())
                {
                    var seat = new Seat
                    {
                        IdSeat = reader.GetInt32(0),
                        SeatNumber = reader.GetInt32(1),
                        Price = reader.GetDecimal(2),
                        IsBooked = reader.GetBoolean(3),
                        SpectacleId = reader.GetInt32(4)
                    };

                    Spectacle.Seats.Add(seat);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка при загрузке мест: " + ex.Message);
            }
        }
        private void BuyButton_Click(object sender, RoutedEventArgs e)
        {

        }
    }
}
