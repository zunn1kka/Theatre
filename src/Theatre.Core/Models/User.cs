using System.ComponentModel;
namespace Theatre.Core.Models
{
    public class User : INotifyPropertyChanged
    {
        public int Id { get; set; }
        public string Login { get; set; }
        public string Password { get; set; }
        public string Email { get; set; }
        public int TypeId { get; set; }
        private decimal _balance;

        public decimal Balance
        {
            get => _balance;
            set
            {
                if (_balance != value)
                {
                    _balance = value;
                    OnPropertyChanged(nameof(Balance));
                    OnPropertyChanged(nameof(BalanceDisplay));
                }
            }
        }
        public string BalanceDisplay => $"{Balance:N2} руб.";
        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}

