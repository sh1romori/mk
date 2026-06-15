using MeatShop.Models;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using MeatShop.Views;
namespace MeatShop
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private void loginButton_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(loginTextBox.Text) || string.IsNullOrWhiteSpace(passwordBox.Password))
            {
                MessageBox.Show("Поля логин и пароль обязательны для заполнения.", "Предупреждение", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            try
            {
                // Используем сгенерированный контекст БД
                using (var db = new SsContext())
                {
                    // Ищем пользователя, игнорируя регистр (защита от багов ввода)
                    var user = db.Users.FirstOrDefault(u => u.Login.ToLower() == loginTextBox.Text.ToLower());

                    if (user == null)
                    {
                        MessageBox.Show("Вы ввели неверный логин или пароль. Пожалуйста проверьте ещё раз введенные данные", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                        return;
                    }

                    if (user.IsLocked)
                    {
                        MessageBox.Show("Вы заблокированы. Обратитесь к администратору", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                        return;
                    }

                    if (user.Password != passwordBox.Password)
                    {
                        user.FailedAttempts++;

                        if (user.FailedAttempts >= 3)
                        {
                            user.IsLocked = true;
                            db.SaveChanges();
                            MessageBox.Show("Вы заблокированы. Обратитесь к администратору", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                        }
                        else
                        {
                            db.SaveChanges();
                            MessageBox.Show("Вы ввели неверный логин или пароль. Пожалуйста проверьте ещё раз введенные данные", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                        }
                        return;
                    }

                    // Успешная авторизация
                    user.FailedAttempts = 0;
                    db.SaveChanges();

                    MessageBox.Show("Вы успешно авторизовались", "Информация", MessageBoxButton.OK, MessageBoxImage.Information);

                    if (user.Role == "Администратор")
                    {
                        AdminWindow adminWindow = new AdminWindow();
                        Application.Current.MainWindow = adminWindow;
                        adminWindow.Show();
                        this.Close();
                    }
                    else
                    {
                        MessageBox.Show("Рабочее место пользователя находится в разработке.", "Информация", MessageBoxButton.OK, MessageBoxImage.Information);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Системная ошибка: " + ex.Message, "Критическая ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}