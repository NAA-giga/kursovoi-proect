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
using Спротивные_достижения.Models;

namespace Спротивные_достижения.Views
{
    /// <summary>
    /// Логика взаимодействия для ChangePasswordWindow.xaml
    /// </summary>
    public partial class ChangePasswordWindow : Window
    {
        private SportAchievementDBEntities _context;
        private UserModel _currentUser;

        public ChangePasswordWindow(SportAchievementDBEntities context, UserModel currentUser)
        {
            InitializeComponent();
            _context = context;
            _currentUser = currentUser;
        }

        private void ChangePasswordButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                string oldPassword = OldPasswordBox.Password;
                string newPassword = NewPasswordBox.Password;
                string confirmPassword = ConfirmPasswordBox.Password;

                // Валидация
                if (string.IsNullOrWhiteSpace(oldPassword))
                {
                    ShowStatus("Введите старый пароль");
                    OldPasswordBox.Focus();
                    return;
                }

                if (string.IsNullOrWhiteSpace(newPassword))
                {
                    ShowStatus("Введите новый пароль");
                    NewPasswordBox.Focus();
                    return;
                }

                if (string.IsNullOrWhiteSpace(confirmPassword))
                {
                    ShowStatus("Подтвердите новый пароль");
                    ConfirmPasswordBox.Focus();
                    return;
                }

                if (newPassword != confirmPassword)
                {
                    ShowStatus("Новый пароль и подтверждение не совпадают");
                    NewPasswordBox.Password = "";
                    ConfirmPasswordBox.Password = "";
                    NewPasswordBox.Focus();
                    return;
                }

                if (newPassword.Length < 4)
                {
                    ShowStatus("Новый пароль должен содержать не менее 4 символов");
                    NewPasswordBox.Focus();
                    return;
                }

                // Проверка старого пароля
                bool oldPasswordValid = false;

                switch (_currentUser.Role)
                {
                    case "Студент":
                        var student = _context.Студент.Find(_currentUser.Id);
                        if (student != null && student.Пароль == oldPassword)
                        {
                            student.Пароль = newPassword;
                            _context.SaveChanges();
                            oldPasswordValid = true;
                        }
                        break;

                    case "Классный руководитель":
                        var teacher = _context.Классный_руководитель.Find(_currentUser.Id);
                        if (teacher != null && teacher.Пароль == oldPassword)
                        {
                            teacher.Пароль = newPassword;
                            _context.SaveChanges();
                            oldPasswordValid = true;
                        }
                        break;

                    case "Администратор":
                        var admin = _context.Администратор.Find(_currentUser.Id);
                        if (admin != null && admin.Пароль == oldPassword)
                        {
                            admin.Пароль = newPassword;
                            _context.SaveChanges();
                            oldPasswordValid = true;
                        }
                        break;
                }

                if (oldPasswordValid)
                {
                    MessageBox.Show("Пароль успешно изменен!", "Успех",
                        MessageBoxButton.OK, MessageBoxImage.Information);

                    // Обновляем пароль в текущем пользователе
                    _currentUser.Password = newPassword;

                    DialogResult = true;
                    Close();
                }
                else
                {
                    ShowStatus("Неверный старый пароль");
                    OldPasswordBox.Password = "";
                    OldPasswordBox.Focus();
                }
            }
            catch (Exception ex)
            {
                ShowStatus($"Ошибка: {ex.Message}");
            }
        }

        private void ShowStatus(string message)
        {
            StatusTextBlock.Text = message;
            StatusTextBlock.Foreground = System.Windows.Media.Brushes.Red;
        }
    }
}
