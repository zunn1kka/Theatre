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
using Theatre.Business.Validators;

namespace Theatre.Pages
{
    /// <summary>
    /// Логика взаимодействия для CaptchaPage.xaml
    /// </summary>
    public partial class CaptchaPage : Page
    {
        private readonly CaptchaValidator _captchaValidator;
        private string _captcha;
        public CaptchaPage()
        {
            InitializeComponent();
            _captchaValidator = new CaptchaValidator();
            GenerateNewCaptcha();
        }
        private void GenerateNewCaptcha()
        {
            _captcha = _captchaValidator.GenerateCaptcha();
            CaptchaTextBlock.Text = _captcha;
        }
        private void BtnCheck_Click(object sender, RoutedEventArgs e)
        {
            string userInput = CaptchaInput.Text;
            if (userInput == _captcha)
            {
                if (NavigationService != null)
                {
                    NavigationService.Navigate(new MainPage());
                }
                else
                {
                    Debug.WriteLine("NavigationService is null. Cannot navigate to MainPage.");

                }
            }
            else
            {
                MessageBox.Show("Попробуйте еще раз", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                GenerateNewCaptcha();
                CaptchaInput.Clear();
            }
        }
    }
}
