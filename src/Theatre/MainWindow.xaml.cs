using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;
using Theatre.Pages;
namespace Theatre
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            MainFrame.Navigated += MainFrame_Navigated;
           LoadFirstPage(new AuthorizationPage());
        }
        private void MainFrame_Navigated(object sender, NavigationEventArgs e)
        {
            if(e.Content is Page page && !string.IsNullOrEmpty(page.Title))
            {
                this.Title = page.Title;
            }
        }
        private void LoadFirstPage(Page page)
        {
            var authPage = new AuthorizationPage();
            MainFrame.Navigate(authPage);
        }
    }
}