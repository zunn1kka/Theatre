using Npgsql;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using Theatre.Business.Services;
using Theatre.Core.Models;

namespace Theatre.Pages
{
    public partial class MainPage : Page
    {
        public ObservableCollection<Spectacles> Spectacles { get; set; }
        User UserData { get; set; } = new();
        public MainPage()
        {
            InitializeComponent();
            Spectacles = new ObservableCollection<Spectacles>();
            DataContext = this;
            IsAdmin();
            LoadSpectaclesAsync();
        }
        public bool IsAdmin() => UserData?.TypeId == 2;
        private async Task LoadSpectaclesAsync()
        {
            Spectacles = new ObservableCollection<Spectacles>();

            try
            {
                const string connectionString = "Host=localhost;Port=5432;Database=Users;Username=postgres;Password=Mailgame5250580;";
                await using var connection = new NpgsqlConnection(connectionString);
                await connection.OpenAsync();

                // Загрузка спектаклей
                const string sqlSpectacles = @"
            SELECT id_spectacle, name_spectacle, quantity_actors, photo_spectacle 
            FROM spectacles";

                await using var commandSpectacles = new NpgsqlCommand(sqlSpectacles, connection);
                await using var readerSpectacles = await commandSpectacles.ExecuteReaderAsync();

                while (await readerSpectacles.ReadAsync())
                {
                    var spectacle = new Spectacles
                    {
                        Id = readerSpectacles.GetInt32(0),
                        Name = readerSpectacles.GetString(1),
                        QuantityActors = readerSpectacles.GetInt32(2),
                        Image = readerSpectacles.IsDBNull(3) ? null : readerSpectacles.GetFieldValue<byte[]>(3),
                        Genres = new List<SpectacleGenre>(),
                        Seats = new List<Seat>()
                    };

                    Spectacles.Add(spectacle);
                }

                await readerSpectacles.CloseAsync();

                // Загрузка жанров
                const string sqlGenres = @"
                    SELECT sg.id_genre, sg.id_spectacle, g.name_genre 
                    FROM spectacle_genre sg
                    JOIN genre g ON sg.id_genre = g.id_genre";

                await using var commandGenres = new NpgsqlCommand(sqlGenres, connection);
                await using var readerGenres = await commandGenres.ExecuteReaderAsync();

                while (await readerGenres.ReadAsync())
                {
                    var genreId = readerGenres.GetInt32(0);
                    var spectacleId = readerGenres.GetInt32(1);
                    var genreName = readerGenres.GetString(2);

                    var spectacle = Spectacles.FirstOrDefault(s => s.Id == spectacleId);
                    spectacle?.Genres.Add(new SpectacleGenre
                        {
                            GenreId = genreId,
                            SpectacleId = spectacleId,
                            Genre = new Genre { Id = genreId, Name = genreName } // Инициализация Genre
                        });
                }

                await readerGenres.CloseAsync();

                // Загрузка мест
                const string sqlSeats = @"
                    SELECT number_seat, price, isbooked, id_spectacle
                    FROM seat";

                await using var commandSeats = new NpgsqlCommand(sqlSeats, connection);
                await using var readerSeats = await commandSeats.ExecuteReaderAsync();

                while (await readerSeats.ReadAsync())
                {
                    var seat = new Seat
                    {
                        SeatNumber = readerSeats.GetInt32(0),
                        Price = readerSeats.GetDecimal(1),
                        IsBooked = readerSeats.GetBoolean(2),
                        SpectacleId = readerSeats.GetInt32(3)
                    };

                    var spectacle = Spectacles.FirstOrDefault(s => s.Id == seat.SpectacleId);
                    spectacle?.Seats.Add(seat);
                }

                await readerSeats.CloseAsync();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка при загрузке данных: " + ex.Message);
            }
        }

        private void ApplyFilters()
        {
            if (Spectacles == null || SearchTextBox == null || FilterComboBox == null || SortComboBox == null)
                return;

            var searchText = SearchTextBox.Text;
            var selectedGenre = (FilterComboBox.SelectedItem as ComboBoxItem)?.Content?.ToString() ?? "Все";
            var selectedSort = (SortComboBox.SelectedItem as ComboBoxItem)?.Content?.ToString() ?? "По названию (А-Я)";

            var filteredSpectacles = Spectacles
                .Where(s => string.IsNullOrEmpty(searchText) || s.Name.Contains(searchText, StringComparison.OrdinalIgnoreCase))
                .Where(s => selectedGenre == "Все" || s.Genres.Any(g => g.Genre != null && g.Genre.Name == selectedGenre))
                .ToList();

            switch (selectedSort)
            {
                case "По названию (А-Я)":
                    filteredSpectacles = filteredSpectacles.OrderBy(s => s.Name).ToList();
                    break;
                case "По названию (Я-А)":
                    filteredSpectacles = filteredSpectacles.OrderByDescending(s => s.Name).ToList();
                    break;
            }

            if (ImagesListBox != null)
            {
                ImagesListBox.ItemsSource = filteredSpectacles;
            }
        }

        private void SearchTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            ApplyFilters();
        }

        private void FilterComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ApplyFilters();
        }

        private void SortComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ApplyFilters();
        }

        private void ProfileButton_Click(object sender, RoutedEventArgs e)
        {
            NavigationService.Navigate(new UserProfilePage(CurrentUser.Id));
        }

        private void OpenSpectaclePage_Click(object sender, RoutedEventArgs e)
        {
            // Получаем выбранный спектакль
            var button = sender as Button;
            var spectacle = button?.DataContext as Spectacles;

            if (spectacle != null)
            {
                // Создаем страницу SpectaclePage и передаем спектакль
                var spectaclePage = new SpectaclePage(spectacle, CurrentUser.Login);

                // Открываем страницу
                NavigationService.Navigate(spectaclePage);
            }
        }


        private void AddSpectacleButton_Click(object sender, RoutedEventArgs e)
        {
            var addSpectaclePage = new AddSpectaclePage();
            addSpectaclePage.SpectacleAdded += OnSpectacleAdded; 
            NavigationService.Navigate(addSpectaclePage);
        }

        private void OnSpectacleAdded(Spectacles newSpectacle, int seatCount, decimal seatPrice)
        {
            SaveSpectacleToDatabase(newSpectacle, seatCount, seatPrice);
            LoadSpectaclesAsync(); // Обновляем список спектаклей
        }

        private void SaveSpectacleToDatabase(Spectacles spectacle, int seatCount, decimal seatPrice)
        {
            try
            {
                const string connectionString = "Host=localhost;Port=5432;Database=Users;Username=postgres;Password=Mailgame5250580;";
                using (var connection = new NpgsqlConnection(connectionString))
                {
                    connection.Open();

                    // Сохраняем спектакль
                    const string sqlSpectacle = @"
                INSERT INTO spectacles (name_spectacle, quantity_actors, photo_spectacle) 
                VALUES (@name, @quantity_actors, @image)
                RETURNING id_spectacle"; // Возвращаем ID нового спектакля

                    int spectacleId;
                    using (var commandSpectacle = new NpgsqlCommand(sqlSpectacle, connection))
                    {
                        commandSpectacle.Parameters.AddWithValue("@name", spectacle.Name);
                        commandSpectacle.Parameters.AddWithValue("@quantity_actors", spectacle.QuantityActors);
                        commandSpectacle.Parameters.AddWithValue("@image", spectacle.Image ?? (object)DBNull.Value);

                        spectacleId = Convert.ToInt32(commandSpectacle.ExecuteScalar());
                    }

                    // Создаем места для спектакля
                    const string sqlSeat = @"
                INSERT INTO seat (number_seat, price, isbooked, id_spectacle) 
                VALUES (@number_seat, @price, @isbooked, @id_spectacle)";

                    for (int i = 1; i <= seatCount; i++)
                    {
                        using var commandSeat = new NpgsqlCommand(sqlSeat, connection);
                        commandSeat.Parameters.AddWithValue("@number_seat", i);
                        commandSeat.Parameters.AddWithValue("@price", seatPrice);
                        commandSeat.Parameters.AddWithValue("@isbooked", false);
                        commandSeat.Parameters.AddWithValue("@id_spectacle", spectacleId);

                        commandSeat.ExecuteNonQuery();
                    }
                }

                MessageBox.Show("Спектакль и места успешно добавлены!");
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка при добавлении спектакля: " + ex.Message);
            }
        }
    }
}