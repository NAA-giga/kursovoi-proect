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
    /// Логика взаимодействия для StudentWindow.xaml
    /// </summary>
    public partial class StudentWindow : Window
    {
        private SportAchievementDBEntities _context;
        private UserModel _currentUser;
        private int _studentId;
        private bool _isClosing = false;
        private bool _isLoggingOut = false;
        public StudentWindow(UserModel user)
        {
            InitializeComponent();
            _context = new SportAchievementDBEntities();
            _currentUser = user;
            _studentId = _currentUser.Id;
            txtUserName.Text = _currentUser.FullName;
            LoadAchievements();
        }
        private void AchievementsScrollViewer_PreviewMouseWheel(object sender, System.Windows.Input.MouseWheelEventArgs e)
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

        private void LoadAchievements()
        {
            if (_isClosing) return;
            try
            {
                var studentAchievements = _context.Студент_Достижение
                    .Where(sa => sa.ID_Студент == _studentId)
                    .Select(sa => new
                    {
                        Id = sa.ID_Достижение,
                        EventName = sa.Достижение.Название_мероприятия ?? "",
                        SportType = sa.Достижение.Название_вида_спорта ?? "",
                        Level = sa.Достижение.Уровень_соревнования ?? "",
                        Place = sa.Занятое_место,
                        Venue = sa.Место_проведения ?? "",
                        EventDate = sa.Дата_проведения,
                        IssueDate = sa.Дата_выдачи,
                        IsTeam = sa.Достижение.Командная_игра ? "Да" : "Нет",
                        TeamSize = sa.Достижение.Численность_команды.HasValue ? sa.Достижение.Численность_команды.ToString() : "-"
                    })
                    .ToList();

                lvAchievements.ItemsSource = studentAchievements;
                txtAchievementCount.Text = $"Всего: {studentAchievements.Count}";
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки достижений: {ex.Message}",
                    "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void LvAchievements_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            btnViewImage.IsEnabled = lvAchievements.SelectedItem != null;
        }

        private void BtnViewImage_Click(object sender, RoutedEventArgs e)
        {
            if (_isClosing) return;
            try
            {
                if (lvAchievements.SelectedItem == null)
                {
                    MessageBox.Show("Выберите достижение для просмотра изображения", "Внимание",
                        MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                dynamic selectedAchievement = lvAchievements.SelectedItem;
                int achievementId = selectedAchievement.Id;

                string fileName = $"{_studentId}-{achievementId}.png";
                string baseDirectory = AppDomain.CurrentDomain.BaseDirectory;
                string imagesFolder = System.IO.Path.Combine(baseDirectory, "картинки достижений");
                string imagePath = System.IO.Path.Combine(imagesFolder, fileName);

                if (!System.IO.Directory.Exists(imagesFolder))
                {
                    MessageBox.Show($"Папка с изображениями не найдена:\n{imagesFolder}\n\n" +
                                   "Создайте папку 'картинки достижений' в директории программы и поместите туда изображения.",
                                   "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                string title = $"Достижение: {selectedAchievement.EventName} (ID: {achievementId})";
                var imageViewer = new ImageViewerWindow(imagePath, title);
                imageViewer.ShowDialog();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
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
            {
                // Закрыли крестиком – завершаем приложение
                Application.Current.Shutdown();
            }
            base.OnClosing(e);
        }
    }
}