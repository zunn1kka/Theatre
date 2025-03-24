using Microsoft.Win32;
using System;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using Theatre.Core.Models;

namespace Theatre.Pages
{
    public partial class AddSpectaclePage : Page
    {
        public Spectacles NewSpectacle { get; set; } = new Spectacles();
        public event Action<Spectacles, int, decimal> SpectacleAdded;

        public AddSpectaclePage()
        {
            InitializeComponent();
            DataContext = this; // Устанавливаем DataContext
        }

        private void SelectImageButton_Click(object sender, RoutedEventArgs e)
        {
            var openFileDialog = new OpenFileDialog
            {
                Filter = "Image files (*.jpg, *.jpeg, *.png)|*.jpg;*.jpeg;*.png|All files (*.*)|*.*"
            };

            if (openFileDialog.ShowDialog() == true)
            {
                var imagePath = openFileDialog.FileName;
                NewSpectacle.Image = File.ReadAllBytes(imagePath);
            }
        }

        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(NameTextBox.Text) || string.IsNullOrWhiteSpace(QuantityActorsTextBox.Text) ||
                string.IsNullOrWhiteSpace(SeatCountTextBox.Text) || string.IsNullOrWhiteSpace(SeatPriceTextBox.Text))
            {
                MessageBox.Show("Заполните все поля.");
                return;
            }

            if (!int.TryParse(QuantityActorsTextBox.Text, out int quantityActors))
            {
                MessageBox.Show("Введите корректное количество актеров.");
                return;
            }

            if (!int.TryParse(SeatCountTextBox.Text, out int seatCount))
            {
                MessageBox.Show("Введите корректное количество мест.");
                return;
            }

            if (!decimal.TryParse(SeatPriceTextBox.Text, out decimal seatPrice))
            {
                MessageBox.Show("Введите корректную цену за место.");
                return;
            }

            NewSpectacle = new Spectacles
            {
                Name = NameTextBox.Text,
                QuantityActors = quantityActors,
                Image = NewSpectacle?.Image
            };

            // Передаем количество мест и цену за место
            SpectacleAdded?.Invoke(NewSpectacle, seatCount, seatPrice);

            if (NavigationService != null)
            {
                NavigationService.GoBack();
            }
            else
            {
                MessageBox.Show("Ошибка: Навигация недоступна.");
            }
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            if (NavigationService != null)
            {
                NavigationService.GoBack();
            }
            else
            {
                MessageBox.Show("Ошибка: Навигация недоступна.");
            }
        }
    }
}