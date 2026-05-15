using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Mail;
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

namespace Спротивные_достижения.Views
{
    /// <summary>
    /// Логика взаимодействия для ForgotPasswordWindow.xaml
    /// </summary>
    public partial class ForgotPasswordWindow : Window
    {
        private SportAchievementDBEntities dbContext;
        private bool _isLoggingOut = false;

        public ForgotPasswordWindow()
        {
            InitializeComponent();
            dbContext = new SportAchievementDBEntities();
        }

        private void BtnResetPassword_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                string login = txtLogin.Text.Trim();
                string email = txtEmail.Text.Trim();

                if (string.IsNullOrWhiteSpace(login))
                {
                    ShowStatus("Введите логин", true);
                    txtLogin.Focus();
                    return;
                }

                if (string.IsNullOrWhiteSpace(email))
                {
                    ShowStatus("Введите адрес электронной почты", true);
                    txtEmail.Focus();
                    return;
                }

                var user = FindUserByLogin(login);

                if (user != null)
                {
                    string newPassword = GenerateRandomPassword(8);
                    bool emailSent = SendEmail(email, newPassword, user.FullName);

                    if (emailSent)
                    {
                        bool updated = UpdateUserPassword(user, newPassword);
                        if (updated)
                        {
                            ShowStatus($"Новый пароль отправлен на почту {email}", false);
                            ClearFields();

                            var result = MessageBox.Show("Новый пароль отправлен на указанную почту.\n\n" +
                                                         "Вернуться к окну авторизации?",
                                                         "Успешно",
                                                         MessageBoxButton.YesNo,
                                                         MessageBoxImage.Information);
                            if (result == MessageBoxResult.Yes)
                            {
                                var mainWindow = new MainWindow();
                                mainWindow.Show();
                                this.Close();
                            }
                        }
                        else
                        {
                            ShowStatus("Ошибка при сохранении пароля в базе данных", true);
                        }
                    }
                    else
                    {
                        ShowStatus("Не удалось отправить email. Пароль не изменен.", true);
                    }
                }
                else
                {
                    ShowStatus("Пользователь с указанным логином не найден", true);
                }
            }
            catch (Exception ex)
            {
                ShowStatus($"Ошибка: {ex.Message}", true);
            }
        }

        private dynamic FindUserByLogin(string login)
        {
            var student = dbContext.Студент.FirstOrDefault(s => s.Логин == login);
            if (student != null)
            {
                return new
                {
                    Type = "Student",
                    Id = student.ID_Студент,
                    Login = student.Логин,
                    Email = student.Электронная_почта,
                    FullName = $"{student.Фамилия} {student.Имя} {student.Отчество}".Trim()
                };
            }

            var teacher = dbContext.Классный_руководитель.FirstOrDefault(t => t.Логин == login);
            if (teacher != null)
            {
                return new
                {
                    Type = "Teacher",
                    Id = teacher.ID_Классный_руководитель,
                    Login = teacher.Логин,
                    Email = teacher.Электронная_почта,
                    FullName = $"{teacher.Фамилия} {teacher.Имя} {teacher.Отчество}".Trim()
                };
            }

            var admin = dbContext.Администратор.FirstOrDefault(a => a.Логин == login);
            if (admin != null)
            {
                return new
                {
                    Type = "Admin",
                    Id = admin.ID_Администратор,
                    Login = admin.Логин,
                    Email = admin.Электронная_почта,
                    FullName = $"{admin.Фамилия} {admin.Имя} {admin.Отчество}".Trim()
                };
            }

            return null;
        }

        private bool UpdateUserPassword(dynamic user, string newPassword)
        {
            try
            {
                switch (user.Type)
                {
                    case "Student":
                        var student = dbContext.Студент.Find(user.Id);
                        if (student != null)
                        {
                            student.Пароль = newPassword;
                            dbContext.SaveChanges();
                            return true;
                        }
                        break;

                    case "Teacher":
                        var teacher = dbContext.Классный_руководитель.Find(user.Id);
                        if (teacher != null)
                        {
                            teacher.Пароль = newPassword;
                            dbContext.SaveChanges();
                            return true;
                        }
                        break;

                    case "Admin":
                        var admin = dbContext.Администратор.Find(user.Id);
                        if (admin != null)
                        {
                            admin.Пароль = newPassword;
                            dbContext.SaveChanges();
                            return true;
                        }
                        break;
                }
                return false;
            }
            catch
            {
                return false;
            }
        }

        private string GenerateRandomPassword(int length)
        {
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789";
            var random = new Random();
            var password = new StringBuilder();

            for (int i = 0; i < length; i++)
            {
                password.Append(chars[random.Next(chars.Length)]);
            }

            return password.ToString();
        }

        private bool SendEmail(string toEmail, string newPassword, string userName)
        {
            try
            {
                // Настройки SMTP (замените на свои данные)
                string fromEmail = "nikitaabaturov862@gmail.com";
                string fromPassword = "rmbh cidt pyev bjqp";
                string smtpServer = "smtp.gmail.com";
                int smtpPort = 587;

                using (SmtpClient smtp = new SmtpClient(smtpServer, smtpPort))
                {
                    smtp.EnableSsl = true;
                    smtp.UseDefaultCredentials = false;
                    smtp.Credentials = new NetworkCredential(fromEmail, fromPassword);
                    smtp.DeliveryMethod = SmtpDeliveryMethod.Network;
                    smtp.Timeout = 30000;

                    MailMessage message = new MailMessage();
                    message.From = new MailAddress(fromEmail, "Система учета спортивных достижений");
                    message.To.Add(toEmail);
                    message.Subject = "Восстановление пароля";
                    message.Body = $@"
Здравствуйте, {userName}!

Вы запросили восстановление пароля в системе учета спортивных достижений.

Ваш новый пароль: {newPassword}

Рекомендуем изменить пароль после входа в систему.

Если вы не запрашивали восстановление пароля, проигнорируйте это сообщение.

С уважением,
Администрация системы.
";
                    message.IsBodyHtml = false;
                    message.BodyEncoding = Encoding.UTF8;
                    message.SubjectEncoding = Encoding.UTF8;

                    smtp.Send(message);
                    return true;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка отправки email: {ex.Message}\n\n" +
                               "Проверьте настройки SMTP в файле конфигурации.",
                               "Ошибка",
                               MessageBoxButton.OK,
                               MessageBoxImage.Error);
                return false;
            }
        }

        private void BtnBack_Click(object sender, RoutedEventArgs e)
        {
            _isLoggingOut = true;
            this.Close();
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

        private void ClearFields()
        {
            txtLogin.Text = string.Empty;
            txtEmail.Text = string.Empty;
        }

        protected override void OnClosing(System.ComponentModel.CancelEventArgs e)
        {
            if (!_isLoggingOut)
            {
                Application.Current.Shutdown();
            }
            base.OnClosing(e);
        }
    }
}
