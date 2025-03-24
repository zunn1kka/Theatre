using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Theatre.Core.Models
{
    public class Spectacles
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public int QuantityActors { get; set; }
        private byte[] _image;

        public byte[] Image
        {
            get => _image;
            set
            {
                _image = value;
                OnPropertyChanged(nameof(Image));
            }
        }
        public List<SpectacleGenre> Genres { get; set; } = [];
        public List<Seat> Seats { get; set; } = [];
        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
