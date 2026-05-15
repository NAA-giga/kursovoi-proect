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
    /// Логика взаимодействия для EditStudentWindow.xaml
    /// </summary>
    public partial class EditStudentWindow : Window
    {
        private SportAchievementDBEntities _context;
        private int _studentId;
        private int? _selectedTeacherId;
        private string _studentGroup;
        private string _studentPassword;

        // Событие при обновлении студента
        public event Action<int, string, string, string> StudentUpdated;

        public EditStudentWindow(SportAchievementDBEntities context, dynamic student)
        {
            InitializeComponent();
            _context = context;

            _studentId = student.Id;
            _studentGroup = student.Group ?? "";

            LoadData();
            LoadStudentData();
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

        private void LoadStudentData()
        {
            try
            {
                var student = _context.Студент.Find(_studentId);
                if (student != null)
                {
                    IdTextBox.Text = _studentId.ToString();
                    LastNameTextBox.Text = student.Фамилия ?? "";
                    FirstNameTextBox.Text = student.Имя ?? "";
                    MiddleNameTextBox.Text = student.Отчество ?? "";
                    LoginTextBox.Text = student.Логин ?? "";
                    _studentPassword = student.Пароль ?? "";
                    PasswordBox.Password = _studentPassword;
                    EmailTextBox.Text = student.Электронная_почта ?? "";
                    PhoneTextBox.Text = student.Номер_телефона ?? "";

                    if (!string.IsNullOrEmpty(_studentGroup))
                    {
                        var group = _context.Группа
                            .FirstOrDefault(g => g.Название == _studentGroup);
                        if (group != null)
                        {
                            GroupComboBox.SelectedValue = group.ID_Группа;
                            UpdateTeacherByGroup(group.ID_Группа);
                        }
                    }

                    EnrollmentDatePicker.SelectedDate = student.Дата_зачисления;
                }
            }
            catch (Exception ex)
            {
                ShowMessage($"Ошибка загрузки данных студента: {ex.Message}");
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
                UpdateTeacherByGroup(selectedGroup.ID_Группа);
            }
            catch (Exception ex)
            {
                ShowMessage($"Ошибка: {ex.Message}");
            }
        }

        private void UpdateTeacherByGroup(int groupId)
        {
            var teacher = _context.Классный_руководитель
                .FirstOrDefault(t => t.ID_Группа == groupId);

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
            return true;
        }

        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (!Validate()) return;

                var student = _context.Студент.Find(_studentId);
                if (student == null)
                {
                    ShowMessage("Студент не найден");
                    return;
                }

                var group = (Группа)GroupComboBox.SelectedItem;
                var teacher = _context.Классный_руководитель.Find(_selectedTeacherId.Value);

                student.Фамилия = LastNameTextBox.Text.Trim();
                student.Имя = FirstNameTextBox.Text.Trim();
                student.Отчество = string.IsNullOrWhiteSpace(MiddleNameTextBox.Text) ? null : MiddleNameTextBox.Text.Trim();
                student.Логин = LoginTextBox.Text.Trim();
                student.Пароль = PasswordBox.Password;
                student.Электронная_почта = EmailTextBox.Text.Trim();
                student.Номер_телефона = PhoneTextBox.Text.Trim();
                student.ID_Группа = group.ID_Группа;
                student.ID_Классный_руководитель = _selectedTeacherId.Value;
                student.Дата_зачисления = EnrollmentDatePicker.SelectedDate.Value;

                _context.SaveChanges();

                // Вызываем событие об обновлении студента
                StudentUpdated?.Invoke(
                    student.ID_Студент,
                    $"{student.Фамилия} {student.Имя} {student.Отчество}".Trim(),
                    group.Название,
                    teacher != null ? $"{teacher.Фамилия} {teacher.Имя}" : "Не назначен"
                );

                MessageBox.Show("Данные студента обновлены!", "Успех",
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
