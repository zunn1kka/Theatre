using System;
using System.Collections.Generic;
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
using Theatre.Business.Services;
using Theatre.Business.Validators;
using Theatre.Data;
using Theatre.Data.SqlQuery;

namespace Theatre
{
    /// <summary>
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        readonly Captcha captcha = new Captcha();
        readonly private AdvancedUserQueries _advancedUserQueries = new AdvancedUserQueries();
        public MainWindow()
        {
            InitializeComponent();
            AdminCheckBox.Checked += (s, e) => ShowAdminKey(true);
            AdminCheckBox.Unchecked += (s, e) => ShowAdminKey(false);
        }

        private async void LoginButton_Click(object sender, RoutedEventArgs e)
        {
            string login = LoginTextBox.Text;
            string password = PasswordBox.Password;

            if (string.IsNullOrEmpty(login) || string.IsNullOrEmpty(password))
            {
                MessageBox.Show("Заполните все поля!");
                return;
            }
            if(!await _advancedUserQueries.CheckUser(login, password))
            {
                MessageBox.Show("Пользователь не существует", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
            else
            {       
                captcha.Show();
                this.Close();
            }
        }

        private async void RegisterButton_Click(object sender, RoutedEventArgs e)
        {
            string login = RegLoginTextBox.Text;
            string email = EmailBox.Text;
            string password = RegPasswordBox.Text;
            string confirmPassword = RegConfirmPasswordBox.Text;
            bool iAmAdmin = AdminCheckBox.IsChecked == true;
            string adminKey = AdminKeyTextBox.Text;
            string code = CodeTextBox.Text;

            PasswordValidator passwordCheck = new PasswordValidator();
            LoginValidator loginCheck = new LoginValidator();
            EmailServices emailCheck = new EmailServices();

            try
            {
                // Проверка заполнения полей
                if (string.IsNullOrEmpty(login) || string.IsNullOrEmpty(email) || string.IsNullOrEmpty(password) || string.IsNullOrEmpty(confirmPassword))
                {
                    MessageBox.Show("Заполните все поля!", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                // Проверка сложности пароля
                string passwordCheckResult = passwordCheck.Check(password);
                if (passwordCheckResult == "Пароль простой")
                {
                    MessageBox.Show(passwordCheckResult, "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                // Проверка сложности логина
                string loginCheckResult = loginCheck.Check(login);
                if (loginCheckResult == "Логин простой")
                {
                    MessageBox.Show(loginCheckResult, "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                // Проверка совпадения паролей
                if (password != confirmPassword)
                {
                    MessageBox.Show("Пароли не совпадают!", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                // Проверка формата email
                if (!emailCheck.IsValidEmail(email))
                {
                    MessageBox.Show("Неверный формат email!", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                // Проверка существования пользователя
                if (await _advancedUserQueries.CheckUserWithEmail(login, email))
                {
                    MessageBox.Show("Пользователь существует", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                // Хеширование пароля
                string passwordHash = UserQuaries.HashPassword(password);

                // Определение типа пользователя
                int typeUser = iAmAdmin ? 2 : 1;

                // Проверка кода подтверждения
                bool isConfirmed = await emailCheck.VerifyConfirmationCodeAsync(email, code, DateTime.UtcNow);
                if (!isConfirmed)
                {
                    MessageBox.Show("Неверный код подтверждения!", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                // Проверка кода администратора (если нужно)
                if (iAmAdmin)
                {
                    bool isAdmin = await AdminQueries.CheckAdmin(adminKey);
                    if (!isAdmin)
                    {
                        MessageBox.Show("Неверный код администратора!", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                        return;
                    }
                }

                // Сохранение пользователя
                await UserQuaries.SaveUserAsync(login, passwordHash, email, typeUser);

                // Уведомление об успешной регистрации
                MessageBox.Show("Вы зарегистрированы!", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);

                // Открытие нового окна
                MainTheatre mainTheatre = new MainTheatre();
                mainTheatre.Show();
                this.Close();
        }
            catch (Exception ex)
            {
                // Логирование и отображение ошибки
                Console.WriteLine($"Ошибка: {ex.Message}");
                MessageBox.Show($"Ошибка: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
}

        private void ShowAdminKey(bool isVisible)
        {
            AdminKeyTextBox.Visibility = isVisible ? Visibility.Visible : Visibility.Collapsed;
        }

        private void GeneratePasswordButton_Click(object sender, RoutedEventArgs e)
        {
            PasswordValidator password = new PasswordValidator();
            string pass = password.GeneratePassword(15, true, true, true, true);
            RegPasswordBox.Text = pass;
            RegConfirmPasswordBox.Text = pass;
        }

        private void RegPasswordBox_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                GeneratePasswordButton.Visibility = Visibility.Visible;
            }
        }

        private async void SendCodeBtn_Click(object sender, RoutedEventArgs e)
        {
            string email = EmailBox.Text;
            EmailServices emailCode = new EmailServices();
            try
            {
                string code = emailCode.GenerateConfirmationCode();

                await emailCode.SaveConfirmationCodeAsync(email, code);

                await emailCode.SendConfirmationEmailAsync(email, code);

                MessageBox.Show("Код подтвреждения отправлен на ваш email.", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}
