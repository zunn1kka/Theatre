using System.Windows;
namespace Theatre.Windows
{
    /// <summary>
    /// Логика взаимодействия для InpuntBalanceDialog.xaml
    /// </summary>
    public partial class InpuntBalanceDialog : Window
    {
        public decimal Amount { get; private set; }
        public InpuntBalanceDialog()
        {
            InitializeComponent();
        }
        private void OkButton_Click(object sender, RoutedEventArgs e)
        {
            if (decimal.TryParse(AmountTextBox.Text, out decimal amount))
            {
                Amount = amount;
                DialogResult = true;
            }
            else
            {
                MessageBox.Show("Введите корректную сумму", "Ошибка",
                              MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}
