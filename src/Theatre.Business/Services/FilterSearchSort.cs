using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Theatre.Core.Models;
using Theatre.Data;
using Theatre.Data.SqlQuery;
namespace Theatre.Business.Services
{
    public class FilterSearchSort : INotifyPropertyChanged
    {
        private ObservableCollection<Spectacle> _filteredSpectacles;
        public ObservableCollection<Spectacle> FilteredSpectacles
        {
            get => _filteredSpectacles;
            set
            {
                _filteredSpectacles = value;
                OnPropertyChanged(nameof(FilteredSpectacles));
            }
        }

        public ObservableCollection<Spectacle> Spectacles { get; set; }

        public async void LoadDataASync()
        {
            var spectacles = await SpectaclesImage.LoadSpectaclesAsync();
            Debug.WriteLine($"Загружено {spectacles.Count} спектаклей.");

            foreach (var spectacle in spectacles)
            {
                Debug.WriteLine($"Спектакль: {spectacle.Name}, Изображение: {spectacle.Image != null}");
            }

            Spectacles = new ObservableCollection<Spectacle>(spectacles);
            FilteredSpectacles = new ObservableCollection<Spectacle>(Spectacles);

            Debug.WriteLine($"FilteredSpectacles содержит {FilteredSpectacles.Count} элементов.");
        }

        // Фильтрация по жанру
        public void FilterByGenre(string genre)
        {
            if (genre == "Все")
            {
                FilteredSpectacles = new ObservableCollection<Spectacle>(Spectacles);
            }
            else
            {
                var filtered = Spectacles
                    .Where(s => s.Genres.Any(g => g.Name == genre))
                    .ToList();
                FilteredSpectacles = new ObservableCollection<Spectacle>(filtered);
            }
        }

        // Сортировка по названию
        public void SortSpectacles(string sortBy)
        {
            List<Spectacle> sorted;
            switch (sortBy)
            {
                case "По названию (А-Я)":
                    sorted = FilteredSpectacles.OrderBy(s => s.Name).ToList();
                    break;
                case "По названию (Я-А)":
                    sorted = FilteredSpectacles.OrderByDescending(s => s.Name).ToList();
                    break;
                default:
                    sorted = FilteredSpectacles.ToList();
                    break;
            }

            FilteredSpectacles = new ObservableCollection<Spectacle>(sorted);
        }

        // Поиск по названию
        public void SearchSpectacles(string search)
        {
            if (string.IsNullOrEmpty(search))
            {
                FilteredSpectacles = new ObservableCollection<Spectacle>(Spectacles);
                return;
            }

            var searched = Spectacles
                .Where(s => s.Name.IndexOf(search, StringComparison.OrdinalIgnoreCase) >= 0)
                .ToList();
            FilteredSpectacles = new ObservableCollection<Spectacle>(searched);
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}

