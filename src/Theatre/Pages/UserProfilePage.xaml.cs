using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;
using Theatre.ViewModels;

namespace Theatre.Pages
{
    /// <summary>
    /// Логика взаимодействия для UserProfilePage.xaml
    /// </summary>
    public partial class UserProfilePage : Page
    {
        public UserProfilePage(int userId)
        {
            InitializeComponent();
            DataContext = new ProfileViewModel(userId);
        }

        private void GoBackButton_Click(object sender, RoutedEventArgs e)
        {
            NavigationService.GoBack();
        }
        private void Exit_Click(object sender, RoutedEventArgs e)
        {
            NavigationService.Navigate(new AuthorizationPage());
        }
    }
}
