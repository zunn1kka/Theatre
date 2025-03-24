using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Navigation;
using Theatre.Commands;
using Theatre.Core.Models;
using Theatre.Data.SqlQuery;
using Theatre.Pages;

namespace Theatre.ViewModels
{
    public class MainPageViewModel
    {

        private User _userData;
        UserQuaries _sqlQueries = new();
        public User UserData
        {
            get => _userData;
            set
            {
                _userData = value;
                OnPropertyChanged(nameof(UserData));
                OnPropertyChanged(nameof(IsAdmin));
            }
        }

        public bool IsAdmin => UserData?.TypeId == 2;

        //public MainPageViewModel()
        //{
        //    LoadUserDataAsync(); // Загрузите данные пользователя
        //}

        //private async Task LoadUserDataAsync()
        //{
        //    try
        //    {
        //        // Загрузите данные пользователя
        //        UserData = await _sqlQueries.GetUserForProfileAsync();
        //    }
        //    catch (Exception ex)
        //    {
        //        MessageBox.Show("Ошибка при загрузке данных пользователя: " + ex.Message);
        //    }
        //}

        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
