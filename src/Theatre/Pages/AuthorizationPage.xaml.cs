using System;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using Theatra.Datas.SqlQuery;
using Theatre.Core.Models;
using Theatre.Data.SqlQuery;
using Theatre.ViewModels;

namespace Theatre.Pages
{
    public partial class AuthorizationPage : Page
    {
        private readonly AuthService _authService;
        private readonly UserQuaries _userQuaries = new();
        public AuthorizationPage()
        {
            InitializeComponent();
            _authService = new AuthService("Host=localhost;Port=5432;Database=Theatre;Username=postgres;Password=Kabinet21;");
            DataContext = new AuthorizationViewModel();
        }

        private async void LoginButton_Click(object sender, RoutedEventArgs e)
        {
            string login = LoginTextBox.Text;
            string password = PasswordBox.Password;

            if (string.IsNullOrEmpty(login) || string.IsNullOrEmpty(password))
            {
                MessageBox.Show("Заполните все поля!", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            try
            {
                // Аутентификация через AuthService
                var user = await _authService.AuthenticateAsync(login, password);

                if (user == null)
                {
                    MessageBox.Show("Неверный логин или пароль", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

               

                // Получаем полные данные пользователя
                var userData = await _userQuaries.GetUserForProfileAsync(login);
                if (userData == null)
                    {
                        MessageBox.Show("Ошибка при получении данных пользователя", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                        return;
                    }

                // Сохранение данных в CurrentUser
                CurrentUser.Id = userData.Id;
                CurrentUser.Login = login;
                CurrentUser.Email = userData.Email;
                CurrentUser.TypeId = userData.TypeId;
                CurrentUser.Balance = userData.Balance;
                Users.Default.CurrentUserLogin = user.Login;
                Users.Default.Save();
                // Переход на страницу капчи
              NavigationService.Navigate(new CaptchaPage());
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка авторизации: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        

        private void Github_RequestNavigate(object sender, System.Windows.Navigation.RequestNavigateEventArgs e)
        {
            Process.Start(new ProcessStartInfo(e.Uri.AbsoluteUri) { UseShellExecute = true });
            e.Handled = true;
        }

        private void Tg_RequestNavigate(object sender, System.Windows.Navigation.RequestNavigateEventArgs e)
        {
            Process.Start(new ProcessStartInfo(e.Uri.AbsoluteUri) { UseShellExecute = true });
            e.Handled = true;
        }

        private void Vk_RequestNavigate(object sender, System.Windows.Navigation.RequestNavigateEventArgs e)
        {
            Process.Start(new ProcessStartInfo(e.Uri.AbsoluteUri) { UseShellExecute = true });
            e.Handled = true;
        }
        
    }
}