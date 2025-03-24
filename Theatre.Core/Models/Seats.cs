using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Theatre.Core.Models
{
    public class Seat : INotifyPropertyChanged
    {
        private bool _isBooked;
        [Key]
        public int IdSeat { get; set; }
        public int SeatNumber { get; set; }
        public decimal Price { get; set; }
        public bool IsBooked
        {
            get => _isBooked;
            set
            {
                if (_isBooked != value)
                {
                    _isBooked = value;
                    OnPropertyChanged(nameof(IsBooked));
                }
            }
        }
        public int SpectacleId { get; set; }
        public Spectacles Spectacle { get; set; }
        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
