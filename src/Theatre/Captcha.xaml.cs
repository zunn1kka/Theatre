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
using System.Windows.Shapes;
using Theatre.Business;
using Theatre.Business.Validators;

namespace Theatre
{
    /// <summary>
    /// Логика взаимодействия для Captcha.xaml
    /// </summary>
    public partial class Captcha : Window
    {
        private readonly CaptchaValidator _captchaValidator;
        private string _captcha;
        public Captcha()
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
            if(userInput == _captcha) {
                MessageBox.Show("Успех");
                MainTheatre mainTheatre = new MainTheatre();
                mainTheatre.Show();
                this.Close();
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
