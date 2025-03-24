using Npgsql;
using System.Windows;
using System;
using Theatre.Core.Models;

namespace Theatre.ViewModels
{
    public class ProfileViewModel : BaseViewModel
    {
        private User _user;

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
            _user = new User { Id = userId };
            LoadUserData();
        }

        private void LoadUserData()
        {
            try
            {
                const string connectionString = "Host=localhost;Port=5432;Database=Users;Username=postgres;Password=Mailgame5250580;";
                using var connection = new NpgsqlConnection(connectionString);
                connection.Open();

                const string sql = @"
                    SELECT login, email
                    FROM users 
                    WHERE id_user = @id_user";
                using var command = new NpgsqlCommand(sql, connection);
                command.Parameters.AddWithValue("@id_user", _user.Id);

                using var reader = command.ExecuteReader();
                if (reader.Read())
                {
                    _user.Login = reader.GetString(0);
                    _user.Email = reader.GetString(1);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка при загрузке данных пользователя: " + ex.Message);
            }
        }
    }
}