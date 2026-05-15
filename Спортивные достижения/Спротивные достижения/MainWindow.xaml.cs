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
using System.Windows.Navigation;
using System.Windows.Shapes;
using Спротивные_достижения.Models;
using Спротивные_достижения.Views;

namespace Спротивные_достижения
{
    /// <summary>
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private SportAchievementDBEntities dbContext;
        private bool isPasswordVisible = false;

        public MainWindow()
        {
            InitializeComponent();
            try
            {
                dbContext = new SportAchievementDBEntities();

                if (txtVisiblePassword != null)
                {
                    txtVisiblePassword.TextChanged += (s, e) =>
                    {
                        if (isPasswordVisible)
                            txtPassword.Password = txtVisiblePassword.Text;
                    };
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка подключения к базе данных: {ex.Message}",
                    "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                Application.Current.Shutdown();
            }
        }

        private void BtnLogin_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (dbContext == null)
                {
                    ShowStatus("Ошибка подключения к базе данных", true);
                    return;
                }

                string login = txtLogin.Text.Trim();
                string password = isPasswordVisible ? txtVisiblePassword.Text : txtPassword.Password;

                if (string.IsNullOrWhiteSpace(login))
                {
                    ShowStatus("Введите логин", true);
                    txtLogin.Focus();
                    return;
                }

                if (string.IsNullOrWhiteSpace(password))
                {
                    ShowStatus("Введите пароль", true);
                    if (!isPasswordVisible)
                        txtPassword.Focus();
                    else
                        txtVisiblePassword.Focus();
                    return;
                }

                UserModel user = FindUser(login, password);

                if (user != null)
                {
                    OpenRoleWindow(user);
                    ClearFields();
                }
                else
                {
                    ShowStatus("Неверный логин или пароль", true);
                }
            }
            catch (Exception ex)
            {
                ShowStatus($"Ошибка: {ex.Message}", true);
            }
        }

        private UserModel FindUser(string login, string password)
        {
            try
            {
                var student = dbContext.Студент.FirstOrDefault(s =>
                    s.Логин == login && s.Пароль == password);

                if (student != null)
                {
                    return new UserModel
                    {
                        Id = student.ID_Студент,
                        LastName = student.Фамилия,
                        FirstName = student.Имя,
                        MiddleName = student.Отчество,
                        Login = student.Логин,
                        Password = student.Пароль,
                        Role = "Студент",
                        Email = student.Электронная_почта,
                        Phone = student.Номер_телефона
                    };
                }

                var teacher = dbContext.Классный_руководитель.FirstOrDefault(t =>
                    t.Логин == login && t.Пароль == password);

                if (teacher != null)
                {
                    return new UserModel
                    {
                        Id = teacher.ID_Классный_руководитель,
                        LastName = teacher.Фамилия,
                        FirstName = teacher.Имя,
                        MiddleName = teacher.Отчество,
                        Login = teacher.Логин,
                        Password = teacher.Пароль,
                        Role = "Классный руководитель",
                        Email = teacher.Электронная_почта,
                        Phone = teacher.Номер_телефона
                    };
                }

                var admin = dbContext.Администратор.FirstOrDefault(a =>
                    a.Логин == login && a.Пароль == password);

                if (admin != null)
                {
                    return new UserModel
                    {
                        Id = admin.ID_Администратор,
                        LastName = admin.Фамилия,
                        FirstName = admin.Имя,
                        MiddleName = admin.Отчество,
                        Login = admin.Логин,
                        Password = admin.Пароль,
                        Role = "Администратор",
                        Email = admin.Электронная_почта,
                        Phone = admin.Номер_телефона
                    };
                }

                return null;
            }
            catch (Exception ex)
            {
                ShowStatus($"Ошибка поиска: {ex.Message}", true);
                return null;
            }
        }

        private void OpenRoleWindow(UserModel user)
        {
            Window roleWindow = null;

            switch (user.Role)
            {
                case "Студент":
                    roleWindow = new StudentWindow(user);
                    break;
                case "Классный руководитель":
                    roleWindow = new TeacherWindow(user);
                    break;
                case "Администратор":
                    roleWindow = new AdminWindow(user);
                    break;
            }

            if (roleWindow != null)
            {
                // Подписываемся на событие закрытия окна роли
                roleWindow.Closed += (s, e) =>
                {
                    // Показываем текущее окно авторизации
                    this.Show();
                    // Очищаем поля
                    ClearFields();
                };

                // Скрываем окно авторизации
                this.Hide();

                // Показываем окно роли
                roleWindow.Show();
            }
        }

        private void BtnForgotPassword_Click(object sender, RoutedEventArgs e)
        {
            var forgotPasswordWindow = new ForgotPasswordWindow();
            // Подписываемся на событие закрытия окна восстановления пароля
            forgotPasswordWindow.Closed += (s, ev) =>
            {
                // Показываем текущее окно авторизации
                this.Show();
            };
            forgotPasswordWindow.Show();
            this.Hide();
        }

        private void BtnTogglePassword_Click(object sender, RoutedEventArgs e)
        {
            if (isPasswordVisible)
            {
                txtPassword.Visibility = Visibility.Visible;
                txtVisiblePassword.Visibility = Visibility.Collapsed;
                btnTogglePassword.Content = "👁";
                isPasswordVisible = false;
            }
            else
            {
                txtVisiblePassword.Text = txtPassword.Password;
                txtPassword.Visibility = Visibility.Collapsed;
                txtVisiblePassword.Visibility = Visibility.Visible;
                btnTogglePassword.Content = "👁‍🗨";
                isPasswordVisible = true;
            }
        }

        private void ClearFields()
        {
            txtLogin.Text = string.Empty;
            if (!isPasswordVisible)
                txtPassword.Password = string.Empty;
            else
                txtVisiblePassword.Text = string.Empty;
            ShowStatus(string.Empty, false);
        }

        private void ShowStatus(string message, bool isError)
        {
            txtStatus.Text = message;
            if (isError)
            {
                txtStatus.Foreground = System.Windows.Media.Brushes.Red;
            }
            else
            {
                txtStatus.Foreground = System.Windows.Media.Brushes.Green;
            }
        }

        protected override void OnClosed(EventArgs e)
        {
            if (dbContext != null)
            {
                dbContext.Dispose();
            }
            base.OnClosed(e);
            // Закрываем приложение только когда закрывается окно авторизации
            Application.Current.Shutdown();
        }

        private void btnAftor_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("Создатель: Абатуров Никита Андреевич\n" +
                "Студент: Слободского педагогического колледжа педагогики и социальных отношений\n" +
                "Группа:23П-2\n" +
                "Курс:3\n" +
                "Специальность:09.02.07'Информационные системы и программирование'\n");
        }
    }
}
