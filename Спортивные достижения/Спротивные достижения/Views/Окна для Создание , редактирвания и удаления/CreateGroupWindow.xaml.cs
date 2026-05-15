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
    /// Логика взаимодействия для CreateGroupWindow.xaml
    /// </summary>
    public partial class CreateGroupWindow : Window
    {
        private SportAchievementDBEntities _context;
        private UserModel _currentUser;
        public int? CreatedGroupId { get; private set; }
        public string CreatedGroupName { get; private set; }

        public event Action<int, string, string, int, int> GroupCreated;

        public CreateGroupWindow(SportAchievementDBEntities context)
        {
            InitializeComponent();
            _context = context;
            LoadData();
        }

        private void LoadData()
        {
            try
            {
                SpecialtyComboBox.ItemsSource = _context.Специальность.ToList();
                CourseComboBox.ItemsSource = _context.Курс.ToList();
            }
            catch (Exception ex)
            {
                ShowMessage($"Ошибка загрузки: {ex.Message}");
            }
        }

        private bool Validate()
        {
            if (string.IsNullOrWhiteSpace(GroupNameTextBox.Text))
            {
                ShowMessage("Введите название группы");
                GroupNameTextBox.Focus();
                return false;
            }
            if (SpecialtyComboBox.SelectedItem == null)
            {
                ShowMessage("Выберите специальность");
                SpecialtyComboBox.Focus();
                return false;
            }
            if (CourseComboBox.SelectedItem == null)
            {
                ShowMessage("Выберите курс");
                CourseComboBox.Focus();
                return false;
            }

            var existingGroup = _context.Группа
                .FirstOrDefault(g => g.Название == GroupNameTextBox.Text.Trim());
            if (existingGroup != null)
            {
                ShowMessage("Группа с таким названием уже существует!");
                GroupNameTextBox.Focus();
                return false;
            }

            return true;
        }

        private void AddAuditLog(string operationType, string tableName, int recordId, string userName, string userRole, string details)
        {
            try
            {
                var audit = new Журнал_аудита
                {
                    Тип_операции = operationType,
                    Имя_таблицы = tableName,
                    ID_записи = recordId,
                    Пользователь = userName,
                    Роль = userRole,
                    Детали = details,
                    Дата_операции = DateTime.Now,
                    ID_Пользователя = _currentUser.Id
                };
                _context.Журнал_аудита.Add(audit);
                _context.SaveChanges();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Ошибка записи в журнал аудита: {ex.Message}");
            }
        }

        // В методе CreateButton_Click после сохранения добавьте:
        private void CreateButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (!Validate()) return;

                var specialty = (Специальность)SpecialtyComboBox.SelectedItem;
                var course = (Курс)CourseComboBox.SelectedItem;
                int currentYear = DateTime.Now.Year;

                var group = new Группа
                {
                    Название = GroupNameTextBox.Text.Trim(),
                    Год_формирования = currentYear,
                    ID_Специальность = specialty.ID_Специальность,
                    ID_Курс = course.ID_Курс
                };

                _context.Группа.Add(group);
                _context.SaveChanges();

                CreatedGroupId = group.ID_Группа;
                CreatedGroupName = group.Название;

                // Вызываем событие
                GroupCreated?.Invoke(
                    group.ID_Группа,
                    group.Название,
                    specialty.Название_специальности,
                    course.Номер_курса,
                    currentYear
                );

                MessageBox.Show($"Группа \"{group.Название}\" успешно создана!", "Успех",
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
        }
    }
}
