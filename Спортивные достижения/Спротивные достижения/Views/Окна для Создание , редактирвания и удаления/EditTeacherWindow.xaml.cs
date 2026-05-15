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
    /// Логика взаимодействия для EditTeacherWindow.xaml
    /// </summary>
    public partial class EditTeacherWindow : Window
    {
        private SportAchievementDBEntities _context;
        private int _teacherId;
        private string _teacherDepartment;
        private string _teacherGroup;
        private string _teacherPassword;

        public event Action<int, string, string, string> TeacherUpdated;

        public EditTeacherWindow(SportAchievementDBEntities context, dynamic teacher)
        {
            InitializeComponent();
            _context = context;

            _teacherId = teacher.Id;
            _teacherDepartment = teacher.Department ?? "";
            _teacherGroup = teacher.Group ?? "";

            LoadData();
            LoadTeacherData();
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

        private void LoadTeacherData()
        {
            try
            {
                var teacher = _context.Классный_руководитель.Find(_teacherId);
                if (teacher != null)
                {
                    IdTextBox.Text = _teacherId.ToString();
                    LastNameTextBox.Text = teacher.Фамилия ?? "";
                    FirstNameTextBox.Text = teacher.Имя ?? "";
                    MiddleNameTextBox.Text = teacher.Отчество ?? "";
                    LoginTextBox.Text = teacher.Логин ?? "";
                    _teacherPassword = teacher.Пароль ?? "";
                    PasswordBox.Password = _teacherPassword;
                    EmailTextBox.Text = teacher.Электронная_почта ?? "";
                    PhoneTextBox.Text = teacher.Номер_телефона ?? "";

                    if (!string.IsNullOrEmpty(_teacherDepartment))
                    {
                        var department = _context.Кафедра
                            .FirstOrDefault(d => d.Название_кафедры == _teacherDepartment);
                        if (department != null)
                        {
                            DepartmentComboBox.SelectedValue = department.ID_Кафедра;
                        }
                    }

                    if (!string.IsNullOrEmpty(_teacherGroup))
                    {
                        var group = _context.Группа
                            .FirstOrDefault(g => g.Название == _teacherGroup);
                        if (group != null)
                        {
                            GroupComboBox.SelectedValue = group.ID_Группа;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                ShowMessage($"Ошибка загрузки данных преподавателя: {ex.Message}");
            }
        }

        // Добавленный метод для кнопки создания группы
        private void CreateGroupButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var createGroupWindow = new CreateGroupWindow(_context);
                if (createGroupWindow.ShowDialog() == true)
                {
                    GroupComboBox.ItemsSource = _context.Группа.ToList();

                    if (createGroupWindow.CreatedGroupId.HasValue)
                    {
                        var newGroup = _context.Группа
                            .FirstOrDefault(g => g.ID_Группа == createGroupWindow.CreatedGroupId.Value);
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
            return true;
        }

        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (!Validate()) return;

                var teacher = _context.Классный_руководитель.Find(_teacherId);
                if (teacher == null)
                {
                    ShowMessage("Преподаватель не найден");
                    return;
                }

                var department = (Кафедра)DepartmentComboBox.SelectedItem;
                var group = (Группа)GroupComboBox.SelectedItem;

                teacher.Фамилия = LastNameTextBox.Text.Trim();
                teacher.Имя = FirstNameTextBox.Text.Trim();
                teacher.Отчество = string.IsNullOrWhiteSpace(MiddleNameTextBox.Text) ? null : MiddleNameTextBox.Text.Trim();
                teacher.Логин = LoginTextBox.Text.Trim();
                teacher.Пароль = PasswordBox.Password;
                teacher.Электронная_почта = EmailTextBox.Text.Trim();
                teacher.Номер_телефона = PhoneTextBox.Text.Trim();
                teacher.ID_Кафедра = department.ID_Кафедра;
                teacher.ID_Группа = group.ID_Группа;

                _context.SaveChanges();

                TeacherUpdated?.Invoke(
                    teacher.ID_Классный_руководитель,
                    $"{teacher.Фамилия} {teacher.Имя} {teacher.Отчество}".Trim(),
                    department.Название_кафедры,
                    group.Название
                );

                MessageBox.Show("Данные преподавателя обновлены!", "Успех",
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
