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

namespace Спротивные_достижения.Views.Окна_для_Создание_и_редактирвания
{
    /// <summary>
    /// Логика взаимодействия для CreateTeacherWindow.xaml
    /// </summary>
    public partial class CreateTeacherWindow : Window
    {
        private SportAchievementDBEntities _context;

        private UserModel _currentUser;

        // Событие при создании преподавателя (5 аргументов)
        public event Action<int, string, string, string, string> TeacherCreated;

        public CreateTeacherWindow(SportAchievementDBEntities context, UserModel currentUser)
        {
            InitializeComponent();
            _context = context;
            _currentUser = currentUser;
            LoadData();
        }

        private void LoadData()
        {
            try
            {
                DepartmentComboBox.ItemsSource = _context.Кафедра.ToList();
                GroupComboBox.ItemsSource = _context.Группа.ToList();
            }
            catch (Exception ex)
            {
                ShowMessage($"Ошибка загрузки: {ex.Message}");
            }
        }

        private bool IsLoginPasswordEmailUnique(string login, string password, string email)
        {
            var studentExists = _context.Студент.Any(s =>
                s.Логин == login || s.Пароль == password || s.Электронная_почта == email);
            if (studentExists) return false;

            var teacherExists = _context.Классный_руководитель.Any(t =>
                t.Логин == login || t.Пароль == password || t.Электронная_почта == email);
            if (teacherExists) return false;

            var adminExists = _context.Администратор.Any(a =>
                a.Логин == login || a.Пароль == password || a.Электронная_почта == email);
            if (adminExists) return false;

            return true;
        }

        private bool Validate()
        {
            if (string.IsNullOrWhiteSpace(LastNameTextBox.Text))
            {
                ShowMessage("Введите фамилию");
                LastNameTextBox.Focus();
                return false;
            }
            if (string.IsNullOrWhiteSpace(FirstNameTextBox.Text))
            {
                ShowMessage("Введите имя");
                FirstNameTextBox.Focus();
                return false;
            }
            if (string.IsNullOrWhiteSpace(LoginTextBox.Text))
            {
                ShowMessage("Введите логин");
                LoginTextBox.Focus();
                return false;
            }
            if (string.IsNullOrWhiteSpace(PasswordBox.Password))
            {
                ShowMessage("Введите пароль");
                PasswordBox.Focus();
                return false;
            }
            if (string.IsNullOrWhiteSpace(EmailTextBox.Text))
            {
                ShowMessage("Введите email");
                EmailTextBox.Focus();
                return false;
            }
            if (string.IsNullOrWhiteSpace(PhoneTextBox.Text))
            {
                ShowMessage("Введите телефон");
                PhoneTextBox.Focus();
                return false;
            }
            if (DepartmentComboBox.SelectedItem == null)
            {
                ShowMessage("Выберите кафедру");
                DepartmentComboBox.Focus();
                return false;
            }
            if (GroupComboBox.SelectedItem == null)
            {
                ShowMessage("Выберите закрепленную группу");
                GroupComboBox.Focus();
                return false;
            }

            if (!IsLoginPasswordEmailUnique(LoginTextBox.Text.Trim(), PasswordBox.Password, EmailTextBox.Text.Trim()))
            {
                ShowMessage("Пользователь с таким логином, паролем или email уже существует в системе!");
                return false;
            }

            return true;
        }

        private void CreateGroupButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var createGroupWindow = new CreateGroupWindow(_context);
                createGroupWindow.GroupCreated += (groupId, groupName, specialtyName, courseNumber, year) =>
                {
                    // Запись в журнал аудита
                    var audit = new Журнал_аудита
                    {
                        Тип_операции = "Создание",
                        Имя_таблицы = "Группа",
                        ID_записи = groupId,
                        Пользователь = _currentUser.FullName,
                        Роль = _currentUser.Role,
                        Детали = $"Создана группа: {groupName}, специальность: {specialtyName}, курс: {courseNumber}, год формирования: {year}",
                        Дата_операции = DateTime.Now,
                        ID_Пользователя = _currentUser.Id
                    };
                    _context.Журнал_аудита.Add(audit);
                    _context.SaveChanges();
                };

                if (createGroupWindow.ShowDialog() == true)
                {
                    GroupComboBox.ItemsSource = _context.Группа.ToList();
                    if (createGroupWindow.CreatedGroupId.HasValue)
                    {
                        var newGroup = _context.Группа.FirstOrDefault(g => g.ID_Группа == createGroupWindow.CreatedGroupId.Value);
                        if (newGroup != null)
                        {
                            GroupComboBox.SelectedValue = newGroup.ID_Группа;
                            ShowMessage($"Группа \"{newGroup.Название}\" выбрана");
                            StatusTextBlock.Foreground = System.Windows.Media.Brushes.Green;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                ShowMessage($"Ошибка при создании группы: {ex.Message}");
            }
        }

        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (!Validate()) return;

                var department = (Кафедра)DepartmentComboBox.SelectedItem;
                var group = (Группа)GroupComboBox.SelectedItem;

                var teacher = new Классный_руководитель
                {
                    Фамилия = LastNameTextBox.Text.Trim(),
                    Имя = FirstNameTextBox.Text.Trim(),
                    Отчество = string.IsNullOrWhiteSpace(MiddleNameTextBox.Text) ? null : MiddleNameTextBox.Text.Trim(),
                    Логин = LoginTextBox.Text.Trim(),
                    Пароль = PasswordBox.Password,
                    Электронная_почта = EmailTextBox.Text.Trim(),
                    Номер_телефона = PhoneTextBox.Text.Trim(),
                    ID_Кафедра = department.ID_Кафедра,
                    ID_Группа = group.ID_Группа
                };

                _context.Классный_руководитель.Add(teacher);
                _context.SaveChanges();

                // Вызываем событие с 5 аргументами
                TeacherCreated?.Invoke(
                    teacher.ID_Классный_руководитель,
                    $"{teacher.Фамилия} {teacher.Имя} {teacher.Отчество}".Trim(),
                    teacher.Логин,
                    department.Название_кафедры,
                    group.Название
                );

                MessageBox.Show("Преподаватель успешно создан!", "Успех",
                    MessageBoxButton.OK, MessageBoxImage.Information);
                DialogResult = true;
                Close();
            }
            catch (Exception ex)
            {
                ShowMessage($"Ошибка: {ex.Message}");
            }
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }

        private void ShowMessage(string message)
        {
            StatusTextBlock.Text = message;
            StatusTextBlock.Foreground = System.Windows.Media.Brushes.Red;
        }
    }
}
