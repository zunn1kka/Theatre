using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Theatra.Datas.SqlQuery;
using Theatre.Commands;
using Theatre.Core.Interfaces;
using Theatre.Core.Models;
using Theatre.Data.SqlQuery;
using Theatre.Windows;

namespace Theatre.Pages
{
    public partial class MainPage : Page
    {
        public ObservableCollection<Spectacles> Spectacles { get; set; }
        public User CurrentUser { get; set; }
        private readonly IUserService _userService;
        public ICommand AddFundsCommand { get; }
        public ICommand RefreshBalanceCommand { get; }
        public MainPage()
        {
            InitializeComponent();
            CurrentUser = new User { TypeId = 2 };
            LoadCurrentUser();
            _userService = new UserQuaries("Host=localhost;Port=5432;Database=Theatre;Username=postgres;Password=Kabinet21;");
            AddFundsCommand = new AsyncRelayCommand(AddFunds);
            RefreshBalanceCommand = new AsyncRelayCommand(RefreshBalance);
            this.DataContext = this;
            LoadSpectaclesAsync();
            Spectacles = new ObservableCollection<Spectacles>();
         
        }



        private async Task AddFunds()
        {
            var dialog = new InpuntBalanceDialog
            {
                Owner = Application.Current.MainWindow
            };

            if (dialog.ShowDialog() == true)
            {
                await _userService.AddToBalance(CurrentUser.Id, dialog.Amount);
                await RefreshBalance();
                MessageBox.Show($"Баланс пополнен на {dialog.Amount:N2} руб.", "Успех");
            }
        }

        private async Task RefreshBalance()
        {
            if (CurrentUser != null)
            {
                CurrentUser.Balance = await _userService.GetBalance(CurrentUser.Id);
            }
        }
        private async void LoadCurrentUser()
        {
            try
            {
                // Получаем логин текущего пользователя 
                string? currentUserLogin = Theatre.Users.Default.CurrentUserLogin;

                if (!string.IsNullOrEmpty(currentUserLogin))
                {
                    var userService = new UserQuaries();
                    CurrentUser = await userService.GetUserByLoginAsync(currentUserLogin);

                    
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки пользователя: {ex.Message}");
            }
        }

        private async void EditSpectacleButton_Click(object sender, RoutedEventArgs e)
        {
            if (!(sender is Button button) || !(button.CommandParameter is Spectacles originalSpectacle))
                return;

            var spectacleService = new SpectacleService("Host=localhost;Port=5432;Database=Theatre;Username=postgres;Password=Kabinet21;");

            try
            {
                // Загружаем свежие данные из БД
                var currentSpectacle = await spectacleService.GetSpectacleById(originalSpectacle.Id);

                var editPage = new EditSpectaclePage(currentSpectacle);
                editPage.SpectacleUpdated += async updatedSpectacle =>
                {
                    try
                    {
                        var result = await spectacleService.UpdateSpectacle(updatedSpectacle);
                        var fullyUpdatedSpectacle = await spectacleService.GetSpectacleById(result.Id);

                        Application.Current.Dispatcher.Invoke(() =>
                        {
                            var index = Spectacles.IndexOf(originalSpectacle);
                            if (index >= 0)
                            {
                                // Полная замена объекта
                                Spectacles[index] = fullyUpdatedSpectacle;

                                // обновление UI
                                fullyUpdatedSpectacle.RefreshGenres();
                            }
                        });
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Ошибка при обновлении: {ex.Message}");
                    }
                };

                NavigationService.Navigate(editPage);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при загрузке данных: {ex.Message}");
            }
        }




        private async void DeleteSpectacleButton_Click(object sender, RoutedEventArgs e)
        {
            var button = sender as Button;
            var spectacle = button?.CommandParameter as Spectacles;

            if (spectacle != null && CurrentUser.TypeId == 2)
            {
                var result = MessageBox.Show($"Вы уверены, что хотите удалить спектакль '{spectacle.Name}'?",
                                           "Подтверждение удаления",
                                           MessageBoxButton.YesNo,
                                           MessageBoxImage.Question);

                if (result == MessageBoxResult.Yes)
                {
                    try
                    {
                        var service = new SpectacleService("Host=localhost;Port=5432;Database=Theatre;Username=postgres;Password=Kabinet21;");
                        await service.DeleteSpectacle(spectacle.Id);

                        // Обновление коллекции 
                        Application.Current.Dispatcher.Invoke(() =>
                        {
                            Spectacles.Remove(spectacle);
                            ApplyFilters(); // Обновляем фильтрацию
                        });

                        MessageBox.Show("Спектакль успешно удален!");
                    }
                    catch (InvalidOperationException ex)
                    {
                        MessageBox.Show(ex.Message, "Ошибка удаления",
                            MessageBoxButton.OK, MessageBoxImage.Warning);
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Ошибка при удалении спектакля: {ex.Message}");
                    }
                }
            }
        }

        private async Task LoadSpectaclesAsync()
        {
            Spectacles = new ObservableCollection<Spectacles>();

            try
            {
                var service = new SpectacleService("Host=localhost;Port=5432;Database=Theatre;Username=postgres;Password=Kabinet21;");
                var allSpectacles = await service.GetAllSpectacles();

                foreach (var spectacle in allSpectacles)
                {
                    Spectacles.Add(spectacle);
                }
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
                case "По дате премьеры":
                    filteredSpectacles = filteredSpectacles.OrderBy(s => s.PremiereDate).ToList();
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
            if (FilterComboBox.SelectedIndex == 0)
            {
                ApplyFilters();
            }
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

        private async void OnSpectacleAdded(Spectacles newSpectacle, int seatCount, decimal seatPrice)
        {
            try
            {
                // 1. Сохраняем в БД и получаем полный объект с жанрами
                var spectacleService = new SpectacleService("Host=localhost;Port=5432;Database=Theatre;Username=postgres;Password=Kabinet21;");
                var insertedSpectacle = await spectacleService.AddSpectacleWithGenres(newSpectacle, seatCount, seatPrice);

                // 2. Добавляем в коллекцию с обновлением UI
                Application.Current.Dispatcher.Invoke(() =>
                {
                    Spectacles.Add(insertedSpectacle);
                    insertedSpectacle.RefreshGenres();
                    ApplyFilters();

                   
                });
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при добавлении спектакля: {ex.Message}");
            }
        }
    }
}