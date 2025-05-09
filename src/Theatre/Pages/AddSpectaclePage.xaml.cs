using Microsoft.Win32;
using System;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;
using Theatra.Datas.SqlQuery;
using Theatre.Core.Models;

namespace Theatre.Pages
{
    public partial class AddSpectaclePage : Page
    {
        public Spectacles NewSpectacle { get; set; } = new Spectacles();
        public event Action<Spectacles, int, decimal> SpectacleAdded;
        private SpectacleService _spectacleService;
        public AddSpectaclePage()
        {
            InitializeComponent();
            _spectacleService = new SpectacleService("Host=localhost;Port=5432;Database=Theatre;Username=postgres;Password=Kabinet21;");      
            DataContext = this;
            InitializeGenresComboBox();
        }

        private void SelectImageButton_Click(object sender, RoutedEventArgs e)
        {
            var openFileDialog = new OpenFileDialog
            {
                Filter = "Image files (*.jpg, *.jpeg, *.png)|*.jpg;*.jpeg;*.png|All files (*.*)|*.*"
            };

            if (openFileDialog.ShowDialog() == true)
            {
                var imageBytes = File.ReadAllBytes(openFileDialog.FileName);
                NewSpectacle.Image = imageBytes;
                LoadImage(imageBytes);
            }
        }
        private void LoadImage(byte[] imageData)
        {
            var image = new BitmapImage();
            using (var mem = new MemoryStream(imageData))
            {
                mem.Position = 0;
                image.BeginInit();
                image.CreateOptions = BitmapCreateOptions.PreservePixelFormat;
                image.CacheOption = BitmapCacheOption.OnLoad;
                image.UriSource = null;
                image.StreamSource = mem;
                image.EndInit();
            }
            image.Freeze();
            SpectacleImage.Source = image;
        }
        private async void InitializeGenresComboBox()
        {
            try
            {
                var allGenres = await _spectacleService.GetAllGenres();
                GenresComboBox.ItemsSource = allGenres;
                GenresComboBox.DisplayMemberPath = "Name";
                GenresComboBox.SelectedValuePath = "Id";
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки жанров: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(NameTextBox.Text) || string.IsNullOrWhiteSpace(QuantityActorsTextBox.Text) ||
                string.IsNullOrWhiteSpace(SeatCountTextBox.Text) || string.IsNullOrWhiteSpace(SeatPriceTextBox.Text))
            {
                MessageBox.Show("Заполните все поля.");
                return;
            }

            if (!int.TryParse(QuantityActorsTextBox.Text, out int quantityActors))
            {
                MessageBox.Show("Введите корректное количество актеров.");
                return;
            }

            if (!int.TryParse(SeatCountTextBox.Text, out int seatCount))
            {
                MessageBox.Show("Введите корректное количество мест.");
                return;
            }

            if (!decimal.TryParse(SeatPriceTextBox.Text, out decimal seatPrice))
            {
                MessageBox.Show("Введите корректную цену за место.");
                return;
            }
            if (!TimeSpan.TryParseExact(ShowTimeTextBox.Text, "hh\\:mm", CultureInfo.InvariantCulture, out _))
            {
                MessageBox.Show("Пожалуйста, введите время в формате ЧЧ:ММ (24-часовой формат)");
                return;
            }
            NewSpectacle.Name = NameTextBox.Text;
            NewSpectacle.QuantityActors = quantityActors;
            NewSpectacle.PremiereDate = PremiereDatePicker.SelectedDate;
            if(GenresComboBox.SelectedItems != null && GenresComboBox.SelectedItems.Count > 0)
            {
                NewSpectacle.Genres = GenresComboBox.SelectedItems.Cast<Genre>()
                    .Select(g => new SpectacleGenre{ GenreId = g.Id})
                    .ToList();
            }

            // Передаем количество мест и цену за место
            SpectacleAdded?.Invoke(NewSpectacle, seatCount, seatPrice);

            if (NavigationService != null)
            {
                NavigationService.GoBack();
            }
            else
            {
                MessageBox.Show("Ошибка: Навигация недоступна.");
            }
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            if (NavigationService != null)
            {
                NavigationService.GoBack();
            }
            else
            {
                MessageBox.Show("Ошибка: Навигация недоступна.");
            }
        }
        private void ShowTimeTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (sender is TextBox textBox)
            {
                if (TimeSpan.TryParseExact(textBox.Text, "hh\\:mm", CultureInfo.InvariantCulture, out var time))
                {
                    NewSpectacle.ShowTime = DateTime.Today.Add(time);
                }
                else
                {
                    // Обработка неверного формата
                    Debug.WriteLine("Неверный формат времени");
                }
            }
        }
    }
}