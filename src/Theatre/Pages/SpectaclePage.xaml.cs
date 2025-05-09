using Npgsql;
using System;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using Theatra.Datas.SqlQuery;
using Theatre.Core.Models;
using Theatre.ViewModels;

namespace Theatre.Pages
{
    /// <summary>
    /// Логика взаимодействия для SpectaclePage.xaml
    /// </summary>
    public partial class SpectaclePage : Page
    {
        private Spectacles Spectacle { get; set; }
        private SpectacleService _spectacleService = new("Host=localhost;Port=5432;Database=Theatre;Username=postgres;Password=Kabinet21;");
        public SpectaclePage(Spectacles spectacle, string userLogin)
        {
            InitializeComponent();
            Spectacle = spectacle;
            var viewModel = new SpectacleViewModel(spectacle, userLogin);
            DataContext = viewModel;
            _spectacleService.LoadSeatsAsync(spectacle.Id);
            _spectacleService.LoadSpectacleData(spectacle.Id);
           
        }

        private void BackButton_Click(object sender, RoutedEventArgs e)
        {
            NavigationService?.GoBack();
        }
        
        
    }
}