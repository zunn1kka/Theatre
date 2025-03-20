using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Win32;
using System.Windows.Media.Imaging;
using System.IO;
using System.Windows.Input;
using Theatre.Core.Models;
using GalaSoft.MvvmLight.Command;

namespace Theatre.ViewModels
{
    public class ProfileViewModel : INotifyPropertyChanged
    {
        private User _user;
        private BitmapImage _userPhoto;
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
        public BitmapImage UserPhoto
        {
            get => _userPhoto;
            set
            {
                _userPhoto = value;
                OnPropertyChanged(nameof(UserPhoto));
            }
        }
        public ICommand LoadPhotoCommand { get; }
        public ProfileViewModel()
        {
            _user = new User();
            LoadPhotoCommand = new RelayCommand(LoadPhoto);
        }
        private void LoadPhoto()
        {
            var openFileDialog = new OpenFileDialog
            {
                Filter = "Image files (*.jpg, *.jpeg, *.png)|*.jpg;*.jpeg;*.png|All files(*.*)|*.*",
                Title = "Выберите фото"
            };
            if(openFileDialog.ShowDialog() == true)
            {
                var filePath = openFileDialog.FileName;
                _user.Photo = File.ReadAllBytes(filePath);
                UserPhoto = LoadImage(_user.Photo);
            }
        }
        private BitmapImage LoadImage(byte[] imageData)
        {
            var image = new BitmapImage();
            using(var stream= new MemoryStream(imageData))
            {
                stream.Position = 0;
                image.BeginInit();
                image.CacheOption = BitmapCacheOption.OnLoad;
                image.StreamSource = stream;
                image.EndInit();
            }
            return image;
        }
        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
