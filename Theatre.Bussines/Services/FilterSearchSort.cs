using System;
using System.Collections.Generic;
using System.Linq;
using Theatre.Core.Models;

namespace Theatre.Business.Services
{
    public class FilterSearchSort
    {
        public IEnumerable<Spectacles> ApplyFilters(
            IEnumerable<Spectacles> spectacles,
            string searchText,
            string selectedGenre,
            string selectedSort)
        {
            var filteredSpectacles = spectacles.AsEnumerable();

            // Фильтрация по поисковому запросу
            if (!string.IsNullOrEmpty(searchText))
            {
                filteredSpectacles = filteredSpectacles
                    .Where(s => s.Name.Contains(searchText, StringComparison.OrdinalIgnoreCase));
            }

            // Фильтрация по жанру
            if (selectedGenre != "Все")
            {
                filteredSpectacles = filteredSpectacles
                    .Where(s => s.Genres.Any(g => g.Genre.Name == selectedGenre));
            }

            // Сортировка
            switch (selectedSort)
            {
                case "По названию (А-Я)":
                    filteredSpectacles = filteredSpectacles.OrderBy(s => s.Name);
                    break;
                case "По названию (Я-А)":
                    filteredSpectacles = filteredSpectacles.OrderByDescending(s => s.Name);
                    break;
            }

            return filteredSpectacles.ToList();
        }
    }
}