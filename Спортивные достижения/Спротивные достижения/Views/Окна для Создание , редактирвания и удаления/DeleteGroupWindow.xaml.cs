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

namespace Спротивные_достижения.Views.Окна_для_Создание___редактирвания_и_удаления
{
    /// <summary>
    /// Логика взаимодействия для DeleteGroupWindow.xaml
    /// </summary>
    public partial class DeleteGroupWindow : Window
    {
        private SportAchievementDBEntities _context;
        private UserModel _currentUser;
        public event Action<int, string, List<(int Id, string Name)>, (int? Id, string Name)?> GroupDeleted;
        public DeleteGroupWindow(SportAchievementDBEntities context, UserModel currentUser)
        {
            InitializeComponent();
            _context = context;
            _currentUser = currentUser;
            LoadGroups();
            GroupComboBox.SelectionChanged += GroupComboBox_SelectionChanged;
        }

        private void LoadGroups()
        {
            try
            {
                GroupComboBox.ItemsSource = _context.Группа.ToList();
                if (GroupComboBox.ItemsSource != null && GroupComboBox.Items.Count > 0)
                    GroupComboBox.SelectedIndex = 0;
            }
            catch (Exception ex)
            {
                ShowMessage($"Ошибка загрузки групп: {ex.Message}");
            }
        }

        private void GroupComboBox_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            try
            {
                if (GroupComboBox.SelectedItem == null)
                {
                    GroupInfoBorder.Visibility = Visibility.Collapsed;
                    return;
                }

                var selectedGroup = (Группа)GroupComboBox.SelectedItem;
                int groupId = selectedGroup.ID_Группа;

                var students = _context.Студент.Where(s => s.ID_Группа == groupId).ToList();
                int studentCount = students.Count;

                var teacher = _context.Классный_руководитель.FirstOrDefault(t => t.ID_Группа == groupId);
                string teacherName = teacher != null ? $"{teacher.Фамилия} {teacher.Имя} {teacher.Отчество}".Trim() : "Не назначен";

                txtGroupName.Text = $"Название: {selectedGroup.Название}";
                txtStudentCount.Text = $"Студентов: {studentCount}";
                txtTeacherName.Text = $"Классный руководитель: {teacherName}";

                GroupInfoBorder.Visibility = Visibility.Visible;
            }
            catch (Exception ex)
            {
                ShowMessage($"Ошибка: {ex.Message}");
            }
        }

        private void AddAuditLog(string operationType, string tableName, int recordId, string details)
        {
            try
            {
                var audit = new Журнал_аудита
                {
                    Тип_операции = operationType,
                    Имя_таблицы = tableName,
                    ID_записи = recordId,
                    Пользователь = _currentUser.FullName,
                    Роль = _currentUser.Role,
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

        private bool Validate()
        {
            if (GroupComboBox.SelectedItem == null)
            {
                ShowMessage("Выберите группу для удаления");
                GroupComboBox.Focus();
                return false;
            }
            return true;
        }

        private void DeleteButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (!Validate()) return;

                var selectedGroup = (Группа)GroupComboBox.SelectedItem;
                int groupId = selectedGroup.ID_Группа;
                string groupName = selectedGroup.Название;

                var students = _context.Студент.Where(s => s.ID_Группа == groupId).ToList();
                var teacher = _context.Классный_руководитель.FirstOrDefault(t => t.ID_Группа == groupId);

                var result = MessageBox.Show($"Вы уверены, что хотите удалить группу \"{groupName}\"?\n\n" +
                    "Будут удалены:\n" +
                    $"• Группа \"{groupName}\"\n" +
                    $"• Студенты: {students.Count} человек\n" +
                    $"• Достижения студентов\n" +
                    $"• Классный руководитель: {(teacher != null ? $"{teacher.Фамилия} {teacher.Имя}" : "отсутствует")}\n\n" +
                    "Это действие необратимо!",
                    "Подтверждение удаления", MessageBoxButton.YesNo, MessageBoxImage.Warning);

                if (result != MessageBoxResult.Yes) return;

                // 1. Логируем удаление группы
                AddAuditLog("Удаление", "Группа", groupId,
                    $"Удалена группа: {groupName}, студентов: {students.Count}, классный руководитель: {(teacher != null ? $"{teacher.Фамилия} {teacher.Имя}" : "отсутствует")}");

                // 2. Логируем удаление каждого студента
                foreach (var student in students)
                {
                    AddAuditLog("Удаление", "Студент", student.ID_Студент,
                        $"Удален студент из группы {groupName}: {student.Фамилия} {student.Имя} {student.Отчество}".Trim());
                }

                // 3. Логируем удаление классного руководителя
                if (teacher != null)
                {
                    AddAuditLog("Удаление", "Классный_руководитель", teacher.ID_Классный_руководитель,
                        $"Удален классный руководитель группы {groupName}: {teacher.Фамилия} {teacher.Имя} {teacher.Отчество}".Trim());
                }

                // 4. Удаляем достижения студентов
                foreach (var student in students)
                {
                    var studentAchievements = _context.Студент_Достижение.Where(sa => sa.ID_Студент == student.ID_Студент).ToList();
                    _context.Студент_Достижение.RemoveRange(studentAchievements);
                }

                // 5. Удаляем студентов
                _context.Студент.RemoveRange(students);

                // 6. Удаляем классного руководителя
                if (teacher != null)
                    _context.Классный_руководитель.Remove(teacher);

                // 7. Удаляем группу
                _context.Группа.Remove(selectedGroup);

                _context.SaveChanges();

                MessageBox.Show($"Группа \"{groupName}\" успешно удалена!\n\nУдалено:\n• Группа: {groupName}\n• Студентов: {students.Count}\n• Классный руководитель: {(teacher != null ? "да" : "нет")}",
                    "Успех", MessageBoxButton.OK, MessageBoxImage.Information);

                DialogResult = true;
                Close();
            }
            catch (Exception ex)
            {
                ShowMessage($"Ошибка при удалении: {ex.Message}");
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
