using MeatShop.Models;
using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using MeatShop.Models;

namespace MeatShop.Views
{
    /// <summary>
    /// Логика взаимодействия для AdminWindow.xaml
    /// </summary>
    public partial class AdminWindow : Window
    {
        public AdminWindow()
        {
            InitializeComponent();
            LoadUsers();
        }

        private void LoadUsers()
        {
            try
            {
                using (var db = new SsContext())
                {
                    usersDataGrid.ItemsSource = db.Users.ToList();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка загрузки данных: " + ex.Message, "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        // КЛИК ПО ТАБЛИЦЕ: Заполняем поля данными выбранного юзера для изменения
        private void usersDataGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (usersDataGrid.SelectedItem is User selectedUser)
            {
                userLoginTextBox.Text = selectedUser.Login;
                userPasswordTextBox.Text = selectedUser.Password;

                if (selectedUser.Role == "Администратор")
                    roleComboBox.SelectedIndex = 0;
                else
                    roleComboBox.SelectedIndex = 1;
            }
        }

        // ДОБАВЛЕНИЕ ПОЛЬЗОВАТЕЛЯ
        private void addUserButton_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(userLoginTextBox.Text) || string.IsNullOrWhiteSpace(userPasswordTextBox.Text))
            {
                MessageBox.Show("Заполните логин и пароль.", "Предупреждение", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            try
            {
                using (var db = new SsContext())
                {
                    if (db.Users.Any(u => u.Login == userLoginTextBox.Text))
                    {
                        MessageBox.Show("Пользователь с указанным логином уже существует.", "Предупреждение", MessageBoxButton.OK, MessageBoxImage.Warning);
                        return;
                    }

                    var newUser = new User
                    {
                        Login = userLoginTextBox.Text,
                        Password = userPasswordTextBox.Text,
                        Role = ((ComboBoxItem)roleComboBox.SelectedItem)?.Content?.ToString() ?? "Пользователь",
                        IsLocked = false,
                        FailedAttempts = 0
                    };

                    db.Users.Add(newUser);
                    db.SaveChanges();

                    MessageBox.Show("Пользователь успешно добавлен.", "Информация", MessageBoxButton.OK, MessageBoxImage.Information);
                    LoadUsers();
                    ClearFields();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка добавления: " + ex.Message, "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        // ИЗМЕНЕНИЕ ДАННЫХ ТЕКУЩЕГО ПОЛЬЗОВАТЕЛЯ
        private void editUserButton_Click(object sender, RoutedEventArgs e)
        {
            var selectedUser = usersDataGrid.SelectedItem as User;

            if (selectedUser == null)
            {
                MessageBox.Show("Выберите пользователя в таблице для изменения данных.", "Информация", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            if (string.IsNullOrWhiteSpace(userLoginTextBox.Text) || string.IsNullOrWhiteSpace(userPasswordTextBox.Text))
            {
                MessageBox.Show("Поля логин и пароль не могут быть пустыми.", "Предупреждение", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            try
            {
                using (var db = new SsContext())
                {
                    // Проверяем, если логин меняется на чужой существующий
                    if (db.Users.Any(u => u.Login == userLoginTextBox.Text && u.UserId != selectedUser.UserId))
                    {
                        MessageBox.Show("Этот логин занят другим пользователем.", "Предупреждение", MessageBoxButton.OK, MessageBoxImage.Warning);
                        return;
                    }

                    var user = db.Users.Find(selectedUser.UserId);
                    if (user != null)
                    {
                        user.Login = userLoginTextBox.Text;
                        user.Password = userPasswordTextBox.Text;
                        user.Role = ((ComboBoxItem)roleComboBox.SelectedItem)?.Content?.ToString() ?? "Пользователь";

                        db.SaveChanges();
                        MessageBox.Show("Данные пользователя успешно изменены.", "Информация", MessageBoxButton.OK, MessageBoxImage.Information);
                        LoadUsers();
                        ClearFields();
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка изменения данных: " + ex.Message, "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        // СНЯТИЕ БЛОКИРОВКИ
        private void unlockUserButton_Click(object sender, RoutedEventArgs e)
        {
            var selectedUser = usersDataGrid.SelectedItem as User;

            if (selectedUser == null)
            {
                MessageBox.Show("Выберите пользователя в таблице для разблокировки.", "Информация", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            try
            {
                using (var db = new SsContext())
                {
                    var user = db.Users.Find(selectedUser.UserId);
                    if (user != null)
                    {
                        user.IsLocked = false;
                        user.FailedAttempts = 0;
                        db.SaveChanges();
                    }
                }

                MessageBox.Show("Пользователь разблокирован.", "Информация", MessageBoxButton.OK, MessageBoxImage.Information);
                LoadUsers();
                ClearFields();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка разблокировки: " + ex.Message, "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void ClearFields()
        {
            userLoginTextBox.Clear();
            userPasswordTextBox.Clear();
            usersDataGrid.SelectedItem = null;
        }

        protected override void OnClosed(EventArgs e)
        {
            base.OnClosed(e);
            Application.Current.Shutdown();
        }
    }
}

