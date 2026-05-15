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

namespace Спротивные_достижения.Views.Окна_для_Создание_и_редактирвания
{
    /// <summary>
    /// Логика взаимодействия для CreateStudentWindow.xaml
    /// </summary>
    public partial class CreateStudentWindow : Window
    {
        private SportAchievementDBEntities _context;
        private int? _selectedTeacherId;

        // Событие при создании студента (5 аргументов)
        public event Action<int, string, string, string, string> StudentCreated;

        public CreateStudentWindow(SportAchievementDBEntities context)
        {
            InitializeComponent();
            _context = context;
            LoadData();
            EnrollmentDatePicker.SelectedDate = DateTime.Now;
        }

        private void LoadData()
        {
            try
            {
                GroupComboBox.ItemsSource = _context.Группа.ToList();
            }
            catch (Exception ex)
            {
                ShowMessage($"Ошибка загрузки: {ex.Message}");
            }
        }

        private void GroupComboBox_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            try
            {
                if (GroupComboBox.SelectedItem == null)
                {
                    _selectedTeacherId = null;
                    return;
                }

                var selectedGroup = (Группа)GroupComboBox.SelectedItem;

                var teacher = _context.Классный_руководитель
                    .FirstOrDefault(t => t.ID_Группа == selectedGroup.ID_Группа);

                if (teacher != null)
                {
                    _selectedTeacherId = teacher.ID_Классный_руководитель;
                    ShowMessage($"Классный руководитель: {teacher.Фамилия} {teacher.Имя}");
                    StatusTextBlock.Foreground = System.Windows.Media.Brushes.Green;
                }
                else
                {
                    _selectedTeacherId = null;
                    ShowMessage("Для выбранной группы не назначен классный руководитель!");
                    StatusTextBlock.Foreground = System.Windows.Media.Brushes.Orange;
                }
            }
            catch (Exception ex)
            {
                ShowMessage($"Ошибка: {ex.Message}");
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
            if (GroupComboBox.SelectedItem == null)
            {
                ShowMessage("Выберите группу");
                GroupComboBox.Focus();
                return false;
            }
            if (EnrollmentDatePicker.SelectedDate == null)
            {
                ShowMessage("Выберите дату зачисления");
                EnrollmentDatePicker.Focus();
                return false;
            }
            if (_selectedTeacherId == null)
            {
                ShowMessage("Для выбранной группы не назначен классный руководитель!");
                return false;
            }

            if (!IsLoginPasswordEmailUnique(LoginTextBox.Text.Trim(), PasswordBox.Password, EmailTextBox.Text.Trim()))
            {
                ShowMessage("Пользователь с таким логином, паролем или email уже существует в системе!");
                return false;
            }

            return true;
        }

        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (!Validate()) return;

                var group = (Группа)GroupComboBox.SelectedItem;
                var teacher = _context.Классный_руководитель.Find(_selectedTeacherId.Value);

                var student = new Студент
                {
                    Фамилия = LastNameTextBox.Text.Trim(),
                    Имя = FirstNameTextBox.Text.Trim(),
                    Отчество = string.IsNullOrWhiteSpace(MiddleNameTextBox.Text) ? null : MiddleNameTextBox.Text.Trim(),
                    Логин = LoginTextBox.Text.Trim(),
                    Пароль = PasswordBox.Password,
                    Электронная_почта = EmailTextBox.Text.Trim(),
                    Номер_телефона = PhoneTextBox.Text.Trim(),
                    ID_Группа = group.ID_Группа,
                    ID_Классный_руководитель = _selectedTeacherId.Value,
                    Дата_зачисления = EnrollmentDatePicker.SelectedDate.Value
                };

                _context.Студент.Add(student);
                _context.SaveChanges();

                // Вызываем событие с 5 аргументами
                StudentCreated?.Invoke(
                    student.ID_Студент,
                    $"{student.Фамилия} {student.Имя} {student.Отчество}".Trim(),
                    student.Логин,
                    group.Название,
                    teacher != null ? $"{teacher.Фамилия} {teacher.Имя}" : "Не назначен"
                );

                MessageBox.Show("Студент успешно создан!", "Успех",
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
