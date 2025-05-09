using System.Windows;
using System.Windows.Input;
using Theatre.Commands;
using Theatre.Core.Models;
using Theatre.Pages;

namespace Theatre.ViewModels
{
    public class AuthorizationViewModel : BaseViewModel
    {
        public ICommand NavigateToRegistrationCommand { get; }
        public ICommand NavigateToAuthCommand { get; }
        
        public AuthorizationViewModel()
        {
            NavigateToRegistrationCommand = new RelayCommand(NavigateForRegistration);
            NavigateToAuthCommand = new RelayCommand(NavigateToAuth);
        }
        private User _currentUser;

        public User CurrentUser
        {
            get => _currentUser;
            set
            {
                _currentUser = value;
                OnPropertyChanged(nameof(CurrentUser));
                OnPropertyChanged(nameof(AddButtonVisibility));
                OnPropertyChanged(nameof(IsAdmin));
            }
        }
        public bool IsAdmin => CurrentUser?.TypeId == 2;
        public Visibility AddButtonVisibility =>
            CurrentUser?.TypeId == 2 ? Visibility.Visible : Visibility.Collapsed;

        private void NavigateForRegistration(object parameter)
        {
            var page = new RegistrationPage();
            var mainwindow = App.Current.MainWindow as MainWindow;
            mainwindow?.MainFrame.Navigate(page);
        }
        private void NavigateToAuth(object parameter) { 
            var page = new AuthorizationPage();
            var mainwindow = App.Current?.MainWindow as MainWindow;
            mainwindow?.MainFrame.Navigate(page);
        }
        
    }
}
