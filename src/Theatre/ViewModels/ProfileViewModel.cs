using Npgsql;
using System.Windows;
using System;
using Theatre.Core.Models;
using Theatre.Data.SqlQuery;

namespace Theatre.ViewModels
{
    public class ProfileViewModel : BaseViewModel
    {
        private User _user;
        public UserQuaries userQuaries = new();

        public string Login
        {
            get => _user.Login;
            set
            {
                _user.Login = value;
                OnPropertyChanged(nameof(Login));
            }
        }

        public string Email
        {
            get => _user.Email;
            set
            {
                _user.Email = value;
                OnPropertyChanged(nameof(Email));
            }
        }

        public ProfileViewModel(int userId)
        {
            _user = new User
            { 
                Id = userId,
                Login = Core.Models.CurrentUser.Login,
                Email = Core.Models.CurrentUser.Email,
                TypeId = Core.Models.CurrentUser.TypeId
            };
            if (string.IsNullOrEmpty(_user.Login))
            {
                userQuaries.LoadUserData();
            }
        }
    }
}