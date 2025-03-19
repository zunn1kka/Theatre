using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using Theatre.Business.Services;

namespace Theatre
{
    /// <summary>
    /// Логика взаимодействия для MainTheatre.xaml
    /// </summary>
    public partial class MainTheatre : Window
    {
        private FilterSearchSort _filterSearchSort;
        public MainTheatre()
        {
            InitializeComponent();
            _filterSearchSort = new FilterSearchSort();
            DataContext = _filterSearchSort;
            _filterSearchSort.LoadDataASync();
        }

        private void SearchTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            string search = SearchTextBox.Text;
            _filterSearchSort.SearchSpectacles(search);
        }

        private void FilterComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (FilterComboBox.SelectedItem is ComboBoxItem selectedItem)
            {
                string genre = selectedItem.Content.ToString();
                _filterSearchSort.FilterByGenre(genre);
            }
        }

        private void ProfileButton_Click(object sender, RoutedEventArgs e)
        {

        }

        private void SortComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (SortComboBox.SelectedItem is ComboBoxItem selectedItem)
            {
                string sortBy = selectedItem.Content.ToString();
                _filterSearchSort.SortSpectacles(sortBy);
            }
        }
    }
}
