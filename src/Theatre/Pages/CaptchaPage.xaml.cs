using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using Theatre.Business.Validators;

namespace Theatre.Pages
{
    /// <summary>
    /// Логика взаимодействия для CaptchaPage.xaml
    /// </summary>
    public partial class CaptchaPage : Page
    {
        private string _captcha;
        public CaptchaPage()
        {
            InitializeComponent();
            LoadCaptcha();
        }
        private void LoadCaptcha()
        {
            var (code, imageData) = CaptchaValidator.GenerateCaptcha();
            _captcha = code;
            using var stream = new MemoryStream(imageData);
            var bitmap = new BitmapImage();
            bitmap.BeginInit();
            bitmap.StreamSource = stream;
            bitmap.CacheOption = BitmapCacheOption.OnLoad;
            bitmap.EndInit();
            CaptchaImage.Source = bitmap;
        }
        private void VerifyButton_Click(object sender, RoutedEventArgs e)
        {
            if (CaptchaTextBox.Text == _captcha)
            {
                // Капча пройдена
                NavigationService.Navigate(new MainPage());
            }
            else
            {
                MessageBox.Show("Неверный код с изображения!", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Error);
                LoadCaptcha();
                CaptchaTextBox.Text = string.Empty;
            }
        }

        private void RefreshButton_Click(object sender, RoutedEventArgs e)
        {
            LoadCaptcha();
            CaptchaTextBox.Text = string.Empty;
        }
    }
}

