using System.Windows;
using Theatre.Core.Interfaces;

namespace Theatre
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        public static IUserService UserService { get; private set; }

        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            // Инициализация сервисов
            //UserService = new UserService("Host=localhost;Port=5432;Database=Theatre;Username=postgres;Password=Kabinet21;");

        }
    }
}
