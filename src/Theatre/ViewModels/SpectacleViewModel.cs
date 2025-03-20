using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Windows.Media.Imaging;
using Theatre.Core.Models;

namespace Theatre.ViewModels
{
    public class SpectacleViewModel
    {
        private Spectacle _spectacle;
        private BitmapImage _spectaclePhoto;
        public string Name
        {
            get => _spectacle.Name;
            set
            {
                _spectacle.Name = value;
                OnPropertyChanged(nameof(Name));
            }
        }
        public BitmapImage SpectaclePhoto
        {
            get => _spectaclePhoto;
            set
            {
                _spectaclePhoto = value;
                OnPropertyChanged(nameof(SpectaclePhoto));
            }
        }

        public ObservableCollection<Seats> Seats { get; set; }
        ICommand BookSeatCommand { get; }


        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
