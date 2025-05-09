using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Theatra.Datas.SqlQuery;
using Theatre.Business.Validators;
using Theatre.Core.Models;
using Theatre.Data.SqlQuery;
using Theatre.ViewModels;

namespace Theatre.Pages
{
    public partial class RegistrationPage : Page
    {
        private readonly UserQuaries _userQueries = new();
        private readonly AuthService _authService;
        private readonly EmailServices _emailCheck = new();

        public RegistrationPage()
        {
            InitializeComponent();
            _authService = new AuthService("Host=localhost;Port=5432;Database=Theatre;Username=postgres;Password=Kabinet21;");
            DataContext = new AuthorizationViewModel();
        }

        private async void RegisterButton_Click(object sender, RoutedEventArgs e)
        {
            string login = RegLoginTextBox.Text;
            string email = EmailBox.Text;
            string password = RegPasswordBox.Text;
            string confirmPassword = RegConfirmPasswordBox.Text;
           
            string code = CodeTextBox.Text;

            var passwordCheck = new PasswordValidator();
            var loginCheck = new LoginValidator();

            try
            {
                // Валидация данных
                if (string.IsNullOrWhiteSpace(login) || string.IsNullOrWhiteSpace(email) ||
                    string.IsNullOrWhiteSpace(password) || string.IsNullOrWhiteSpace(confirmPassword))
                {
                    ShowError("Заполните все поля!");
                    return;
                }

                if (passwordCheck.Check(password) == "Пароль простой")
                {
                    ShowError("Пароль слишком простой!");
                    return;
                }

                if (loginCheck.Check(login) == "Логин недопустимый")
                {
                    ShowError("Логин не соответствует требованиям!");
                    return;
                }

                if (password != confirmPassword)
                {
                    ShowError("Пароли не совпадают!");
                    return;
                }

                if (!_emailCheck.IsValidEmail(email))
                {
                    ShowError("Неверный формат email!");
                    return;
                }

                if (await _userQueries.CheckUserWithEmail(login, email))
                {
                    ShowError("Пользователь с таким логином или email уже существует!");
                    return;
                }

                if (!await _emailCheck.VerifyConfirmationCodeAsync(email, code, DateTime.UtcNow))
                {
                    ShowError("Неверный код подтверждения!");
                    return;
                }

                var userData = await _userQueries.GetUserForProfileAsync(login);

                // Регистрация через AuthService
                var user = await _authService.RegisterAsync(login, email, password);
                if (user != null)
                {
                    CurrentUser.Id = user.Id;
                    CurrentUser.Login = user.Login;
                    CurrentUser.Email = user.Email;
                    CurrentUser.TypeId = user.TypeId;
                    Users.Default.CurrentUserLogin = user.Login;
                    Users.Default.Save();
                    NavigationService.Navigate(new MainPage());
                }
                else
                {
                    ShowError("Ошибка регистрации. Возможно, пользователь уже существует.");
                }
            }
            catch (Exception ex)
            {
                ShowError($"Ошибка: {ex.Message}");
                Console.WriteLine($"Ошибка регистрации: {ex}");
            }
        }
       
        // Кнопка генерации пароля
        private void GeneratePasswordButton_Click(object sender, RoutedEventArgs e)
        {
            var password = new PasswordValidator().GeneratePassword(16, true, true, true, true);
            RegPasswordBox.Text = password;
            RegConfirmPasswordBox.Text = password;
        }

        private void RegPasswordBox_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                GeneratePasswordButton.Visibility = Visibility.Visible;
            }
        }

        // Кнопка отправки кода подтверждения
        private async void SendCodeBtn_Click(object sender, RoutedEventArgs e)
        {
            string login = RegLoginTextBox.Text;
            string email = EmailBox.Text;
            if (string.IsNullOrWhiteSpace(email))
            {
                ShowError("Введите email для отправки кода");
                return;
            }

            try
            {
                string code = _emailCheck.GenerateConfirmationCode();
                await _emailCheck.SaveConfirmationCodeAsync(email, code);
                await _emailCheck.SendConfirmationEmailAsync(email, login, code);

                MessageBox.Show("Код подтверждения отправлен на ваш email.",
                    "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                ShowError($"Ошибка отправки кода: {ex.Message}");
            }
        }
        private void ShowError(string message)
        {
            MessageBox.Show(message, "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
        }

        private void RegPasswordBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            var passwordCheck = new PasswordValidator();
            string password = RegPasswordBox.Text;
            string strengthLevel = passwordCheck.Check(password);
            UpdatePasswordIndicator(strengthLevel);
        }
        private void UpdatePasswordIndicator(string strengthLevel)
        {
            pwdWeak.Fill = System.Windows.Media.Brushes.LightGray;
            pwdMedium.Fill = System.Windows.Media.Brushes.LightGray;
            pwdStrong.Fill = System.Windows.Media.Brushes.LightGray;
            PasswordStrengthDescription.Text = string.Empty;

            switch (strengthLevel)
            {
                case "Пароль простой":
                    pwdWeak.Fill = System.Windows.Media.Brushes.Red;
                    PasswordStrengthDescription.Text = "Слабый пароль: используйте буквы разного регистра, цифры и спецсимволы";
                    PasswordStrengthDescription.Foreground = System.Windows.Media.Brushes.Red;
                    break;

                case "Пароль средний":
                    pwdWeak.Fill = System.Windows.Media.Brushes.Orange;
                    pwdMedium.Fill = System.Windows.Media.Brushes.Orange;
                    PasswordStrengthDescription.Text = "Средний пароль: можно усилить";
                    PasswordStrengthDescription.Foreground = System.Windows.Media.Brushes.Orange;
                    break;

                case "Пароль сложный":
                    pwdWeak.Fill = System.Windows.Media.Brushes.Green;
                    pwdMedium.Fill = System.Windows.Media.Brushes.Green;
                    pwdStrong.Fill = System.Windows.Media.Brushes.Green;
                    PasswordStrengthDescription.Text = "Надежный пароль!";
                    PasswordStrengthDescription.Foreground = System.Windows.Media.Brushes.Green;
                    break;

                case "empty":
                default:
                    // Ничего не делаем, оставляем серые индикаторы
                    break;
            }
        }
    }
}