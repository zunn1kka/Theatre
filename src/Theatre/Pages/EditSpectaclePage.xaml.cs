using System;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;
using Microsoft.Win32;
using Theatre.Core.Models;
using System.Diagnostics;
using System.Globalization;
using Theatra.Datas.SqlQuery;

namespace Theatre.Pages
{
    public partial class EditSpectaclePage : Page
    {
        private readonly Spectacles _spectacle;
        private readonly SpectacleService _spectacleService;
        public event Action<Spectacles> SpectacleUpdated;

        public EditSpectaclePage(Spectacles spectacle)
        {
            InitializeComponent();
            _spectacle = spectacle ?? new Spectacles();
            _spectacleService = new SpectacleService("Host=localhost;Port=5432;Database=Theatre;Username=postgres;Password=Kabinet21;");

            // Устанавливаем DataContext на спектакль
            DataContext = _spectacle;

            InitializeGenresComboBox();
            LoadSpectacleData();
        }

        private async void InitializeGenresComboBox()
        {
            try
            {
                var allGenres = await _spectacleService.GetAllGenres();
                GenresComboBox.ItemsSource = allGenres;
                GenresComboBox.DisplayMemberPath = "Name";
                GenresComboBox.SelectedValuePath = "Id";

                // Очищаем выбранные элементы
                GenresComboBox.SelectedItems.Clear();

                // Устанавливаем выбранные жанры 
                if (_spectacle.Genres != null && _spectacle.Genres.Any())
                {
                    foreach (var genre in allGenres)
                    {
                        if (_spectacle.Genres.Any(g => g.GenreId == genre.Id))
                        {
                            GenresComboBox.SelectedItems.Add(genre);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки жанров: {ex.Message}");
            }
        }

        private void LoadSpectacleData()
        {
            try
            {
                Debug.WriteLine($"Дата премьеры из объекта: {_spectacle.PremiereDate}");
                // Основная информация
                NameTextBox.Text = _spectacle.Name;
                QuantityActorsTextBox.Text = _spectacle.QuantityActors.ToString();

                // Информация о местах
                if (_spectacle.Seats != null && _spectacle.Seats.Any())
                {
                    SeatCountTextBox.Text = _spectacle.Seats.Count.ToString();
                    SeatPriceTextBox.Text = _spectacle.Seats.First().Price.ToString("N2");
                }

                // Даты
                if (_spectacle.PremiereDate.HasValue)
                {
                    PremiereDatePicker.SelectedDate = _spectacle.PremiereDate.Value;
                    Debug.WriteLine($"Установлена дата в DatePicker: {_spectacle.PremiereDate.Value}");
                }
                else
                {
                    PremiereDatePicker.SelectedDate = null;
                    Debug.WriteLine("Дата премьеры не установлена (null)");
                }

                // Изображение
                if (_spectacle.Image != null)
                {
                    LoadImage(_spectacle.Image);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при загрузке данных: {ex.Message}");
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

        private async void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // Валидация жанров
                var selectedGenres = GenresComboBox.SelectedItems?.Cast<Genre>().ToList();
                if (selectedGenres == null || !selectedGenres.Any())
                {
                    MessageBox.Show("Выберите хотя бы один жанр!");
                    return;
                }
                if (!TimeSpan.TryParseExact(ShowTimeTextBox.Text, "hh\\:mm", CultureInfo.InvariantCulture, out _))
                {
                    MessageBox.Show("Пожалуйста, введите время в формате ЧЧ:ММ (24-часовой формат)");
                    return;
                }
                // Обновление данных
                _spectacle.Name = NameTextBox.Text;
                _spectacle.QuantityActors = int.Parse(QuantityActorsTextBox.Text);
                _spectacle.PremiereDate = PremiereDatePicker.SelectedDate;

                //  обновление жанров
                _spectacle.Genres = GenresComboBox.SelectedItems
        .Cast<Genre>()
        .Distinct() // Убираем дубликаты
        .Select(g => new SpectacleGenre
        {
            GenreId = g.Id,
            SpectacleId = _spectacle.Id,
            Genre = g
        })
        .ToList();

                // Обновление мест
                if (int.TryParse(SeatCountTextBox.Text, out int seatCount) &&
                    decimal.TryParse(SeatPriceTextBox.Text, out decimal seatPrice))
                {
                    _spectacle.Seats = Enumerable.Range(1, seatCount)
                        .Select(i => new Seat
                        {
                            SeatNumber = i,
                            Price = seatPrice,
                            IsBooked = false,
                            SpectacleId = _spectacle.Id
                        })
                        .ToList();
                }

                // Сохранение в БД
                var updatedSpectacle = await _spectacleService.UpdateSpectacle(_spectacle);

                // Обновление UI
                _spectacle.RefreshGenres();
                SpectacleUpdated?.Invoke(_spectacle);

                MessageBox.Show("Спектакль успешно обновлен!", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
                NavigationService.GoBack();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при сохранении: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            NavigationService.GoBack();
        }

        private void SelectImageButton_Click(object sender, RoutedEventArgs e)
        {
            var openFileDialog = new OpenFileDialog
            {
                Filter = "Image files (*.png;*.jpeg;*.jpg)|*.png;*.jpeg;*.jpg|All files (*.*)|*.*",
                Title = "Выберите изображение спектакля"
            };

            if (openFileDialog.ShowDialog() == true)
            {
                try
                {
                    var imageBytes = File.ReadAllBytes(openFileDialog.FileName);
                    _spectacle.Image = imageBytes;
                    LoadImage(imageBytes);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Ошибка при загрузке изображения: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }
        private void ShowTimeTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (sender is TextBox textBox)
            {
                if (TimeSpan.TryParseExact(textBox.Text, "hh\\:mm", CultureInfo.InvariantCulture, out var time))
                {
                    _spectacle.ShowTime = DateTime.Today.Add(time);
                }
                
            }
        }
    }
}
