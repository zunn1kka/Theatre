using System.ComponentModel;
using System.Runtime.CompilerServices;

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
        private List<SpectacleGenre> _genres;
        public List<SpectacleGenre> Genres
        {
            get => _genres;
            set
            {
                _genres = value;
                OnPropertyChanged(nameof(Genres));
                OnPropertyChanged(nameof(GenreDisplay)); // Уведомляем об изменении Genre
            }
        }

        // Для отображения жанров в виде строки
        public string GenreDisplay
        {
            get
            {
                if (Genres == null || !Genres.Any())
                    return "Жанр не указан";

                var uniqueGenres = Genres
                    .Where(g => g.Genre != null)
                    .Select(g => g.Genre.Name)
                    .Distinct()
                    .ToList();

                return string.Join(", ", uniqueGenres);
            }
        }

        public List<Seat> Seats { get; set; } = new List<Seat>();
        private DateTime? _premiereDate;
        public DateTime? PremiereDate
        {
            get => _premiereDate;
            set
            {
                _premiereDate = value;
                OnPropertyChanged(nameof(PremiereDate));
            }
        }

        private DateTime? _showTime;
        public DateTime? ShowTime
        {
            get => _showTime;
            set
            {
                _showTime = value;
                OnPropertyChanged(nameof(ShowTime));
            }
        }

        public string PriceInfo
        {
            get
            {
                if (Seats == null || Seats.Count == 0)
                    return "Цены не указаны";

                return $"{Seats.Min(s => s.Price):C}";
            }
        }
        public void RefreshGenres()
        {
            var temp = Genres;
            Genres = null;
            Genres = temp;
            OnPropertyChanged(nameof(Genres));
            OnPropertyChanged(nameof(GenreDisplay));

            
        }

        public event PropertyChangedEventHandler PropertyChanged;
        public virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
        
    }
}
