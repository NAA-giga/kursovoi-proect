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
    /// Логика взаимодействия для TeacherWindow.xaml
    /// </summary>
    public partial class TeacherWindow : Window
    {
        private SportAchievementDBEntities _context;
        private UserModel _currentUser;
        private int _teacherId;
        private int? _teacherGroupId;
        private string _teacherGroupName;
        private bool _isLoggingOut = false;

        public TeacherWindow(UserModel user)
        {
            InitializeComponent();
            _context = new SportAchievementDBEntities();
            _currentUser = user;
            _teacherId = _currentUser.Id;

            txtUserName.Text = _currentUser.FullName;

            LoadTeacherData();
            LoadStudents();
        }

        private void LoadTeacherData()
        {
            try
            {
                var teacher = _context.Классный_руководитель.Find(_teacherId);
                if (teacher != null)
                {
                    _teacherGroupId = teacher.ID_Группа;
                    if (_teacherGroupId.HasValue)
                    {
                        var group = _context.Группа.Find(_teacherGroupId.Value);
                        if (group != null)
                        {
                            _teacherGroupName = group.Название;
                            txtGroupName.Text = _teacherGroupName;
                        }
                        else
                        {
                            txtGroupName.Text = "Группа не найдена";
                        }
                    }
                    else
                    {
                        txtGroupName.Text = "Группа не назначена";
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки данных преподавателя: {ex.Message}",
                    "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void LoadStudents()
        {
            try
            {
                if (!_teacherGroupId.HasValue)
                {
                    lvStudents.ItemsSource = null;
                    txtStudentCount.Text = "Всего: 0";
                    btnViewAchievements.IsEnabled = false;
                    return;
                }

                var students = _context.Студент
                    .Where(s => s.ID_Группа == _teacherGroupId.Value)
                    .Select(s => new
                    {
                        Id = s.ID_Студент,
                        LastName = s.Фамилия ?? "",
                        FirstName = s.Имя ?? "",
                        MiddleName = s.Отчество ?? "",
                        Login = s.Логин ?? "",
                        Email = s.Электронная_почта ?? "",
                        Phone = s.Номер_телефона ?? "",
                        EnrollmentDate = s.Дата_зачисления
                    })
                    .ToList();

                lvStudents.ItemsSource = students;
                txtStudentCount.Text = $"Всего: {students.Count}";
                btnViewAchievements.IsEnabled = students.Any();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки студентов: {ex.Message}",
                    "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void LvStudents_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            btnViewAchievements.IsEnabled = lvStudents.SelectedItem != null;
        }

        private void BtnViewAchievements_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (lvStudents.SelectedItem == null)
                {
                    MessageBox.Show("Выберите студента для просмотра достижений", "Внимание",
                        MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                dynamic selectedStudent = lvStudents.SelectedItem;
                int studentId = selectedStudent.Id;
                string studentName = $"{selectedStudent.LastName} {selectedStudent.FirstName} {selectedStudent.MiddleName}".Trim();

                var achievementsWindow = new StudentAchievementsWindow(_context, studentId, studentName, _currentUser);
                achievementsWindow.ShowDialog();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void StudentsScrollViewer_PreviewMouseWheel(object sender, System.Windows.Input.MouseWheelEventArgs e)
        {
            var scrollViewer = sender as ScrollViewer;
            if (scrollViewer != null)
            {
                if (e.Delta > 0)
                    scrollViewer.LineUp();
                else
                    scrollViewer.LineDown();
                e.Handled = true;
            }
        }

        private void BtnChangePassword_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var changePasswordWindow = new ChangePasswordWindow(_context, _currentUser);
                if (changePasswordWindow.ShowDialog() == true)
                {
                    MessageBox.Show("Пароль успешно изменен! При следующем входе используйте новый пароль.",
                        "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void BtnLogout_Click(object sender, RoutedEventArgs e)
        {
            _isLoggingOut = true;
            this.Close();
        }

        protected override void OnClosing(System.ComponentModel.CancelEventArgs e)
        {
            if (!_isLoggingOut)
                Application.Current.Shutdown();
            base.OnClosing(e);
        }

        protected override void OnClosed(EventArgs e)
        {
            _context?.Dispose();
            base.OnClosed(e);
        }
    }
}
