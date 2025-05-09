using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace Theatre.Core.Models
{
    public class SpectacleGenre
    {
        public int SpectacleId { get; set; }
        public int GenreId { get; set; }
        private Genre _genre;
        public Genre Genre
        {
            get => _genre;
            set
            {
                _genre = value;
                OnPropertyChanged();
            }
        }
        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
