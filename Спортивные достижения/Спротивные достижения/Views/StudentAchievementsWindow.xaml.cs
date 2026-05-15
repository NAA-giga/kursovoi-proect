using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.IO;
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
using Спротивные_достижения.Views.Окна_для_Создание_и_редактирвания;

namespace Спротивные_достижения.Views
{
    /// <summary>
    /// Логика взаимодействия для StudentAchievementsWindow.xaml
    /// </summary>
    public partial class StudentAchievementsWindow : Window
    {
        private SportAchievementDBEntities _context;
        private int _studentId;
        private string _studentName;
        private UserModel _currentUser;
        private bool _isClosing = false;

        public StudentAchievementsWindow(SportAchievementDBEntities context, int studentId, string studentName, UserModel currentUser)
        {
            InitializeComponent();
            _context = context;
            _studentId = studentId;
            _studentName = studentName;
            _currentUser = currentUser;
            txtStudentInfo.Text = studentName;
            LoadAchievements();
        }

        protected override void OnClosing(System.ComponentModel.CancelEventArgs e)
        {
            _isClosing = true;
            base.OnClosing(e);
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

                AchievementsDataGrid.ItemsSource = studentAchievements;
                txtAchievementCount.Text = $"Всего: {studentAchievements.Count}";
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки достижений: {ex.Message}",
                    "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
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

        private void BtnCreate_Click(object sender, RoutedEventArgs e)
        {
            if (_isClosing) return;
            try
            {
                var createWindow = new CreateAchievementWindow(_context, _studentId);
                createWindow.AchievementCreated += (achId, eventName, sportType, level, isTeam, teamSize) =>
                {
                    string teamInfo = isTeam ? $", командная игра, численность: {teamSize}" : ", индивидуальное";
                    AddAuditLog(
                        "Создание",
                        "Достижение",
                        achId,
                        $"Создано достижение: {eventName}, вид спорта: {sportType}, уровень: {level}{teamInfo}"
                    );
                };
                if (createWindow.ShowDialog() == true)
                {
                    LoadAchievements();
                    MessageBox.Show("Достижение успешно добавлено!", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void BtnEdit_Click(object sender, RoutedEventArgs e)
        {
            if (_isClosing) return;
            try
            {
                if (AchievementsDataGrid.SelectedItem == null)
                {
                    MessageBox.Show("Выберите достижение для редактирования", "Внимание", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                dynamic selectedAchievement = AchievementsDataGrid.SelectedItem;
                var editWindow = new EditAchievementWindow(_context, selectedAchievement, _studentId);
                editWindow.AchievementUpdated += (achId, eventName, sportType, level, isTeam, teamSize) =>
                {
                    string teamInfo = isTeam ? $", командная игра, численность: {teamSize}" : ", индивидуальное";
                    AddAuditLog(
                        "Редактирование",
                        "Достижение",
                        achId,
                        $"Обновлено достижение: {eventName}, вид спорта: {sportType}, уровень: {level}{teamInfo}"
                    );
                };
                if (editWindow.ShowDialog() == true)
                {
                    LoadAchievements();
                    MessageBox.Show("Достижение успешно обновлено!", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void BtnDelete_Click(object sender, RoutedEventArgs e)
        {
            if (_isClosing) return;
            try
            {
                if (AchievementsDataGrid.SelectedItem == null)
                {
                    MessageBox.Show("Выберите достижение для удаления", "Внимание", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                var result = MessageBox.Show("Вы уверены, что хотите удалить выбранное достижение?\n\n" +
                    "ВНИМАНИЕ: Это действие необратимо!",
                    "Подтверждение удаления", MessageBoxButton.YesNo, MessageBoxImage.Question);

                if (result == MessageBoxResult.Yes)
                {
                    dynamic selectedAchievement = AchievementsDataGrid.SelectedItem;
                    int achievementId = selectedAchievement.Id;

                    var studentAchievement = _context.Студент_Достижение
                        .FirstOrDefault(sa => sa.ID_Студент == _studentId && sa.ID_Достижение == achievementId);

                    if (studentAchievement != null)
                    {
                        _context.Студент_Достижение.Remove(studentAchievement);
                        _context.SaveChanges();

                        bool otherStudents = _context.Студент_Достижение.Any(sa => sa.ID_Достижение == achievementId);
                        if (!otherStudents)
                        {
                            var achievement = _context.Достижение.Find(achievementId);
                            if (achievement != null)
                            {
                                AddAuditLog(
                                    "Удаление",
                                    "Достижение",
                                    achievementId,
                                    $"Удалено достижение: {achievement.Название_мероприятия}, вид спорта: {achievement.Название_вида_спорта}, уровень: {achievement.Уровень_соревнования}"
                                );
                                _context.Достижение.Remove(achievement);
                            }
                        }
                        else
                        {
                            AddAuditLog(
                                "Удаление",
                                "Студент_Достижение",
                                studentAchievement.ID_Студент_Достижение,
                                $"Удалена связь студента с достижением (ID студента {_studentId}, ID достижения {achievementId})"
                            );
                        }

                        _context.SaveChanges();
                        LoadAchievements();
                        MessageBox.Show("Достижение успешно удалено!", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void BtnViewImage_Click(object sender, RoutedEventArgs e)
        {
            if (_isClosing) return;
            try
            {
                if (AchievementsDataGrid.SelectedItem == null)
                {
                    MessageBox.Show("Выберите достижение для просмотра изображения", "Внимание", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                dynamic selectedAchievement = AchievementsDataGrid.SelectedItem;
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

        private void BtnAttachImage_Click(object sender, RoutedEventArgs e)
        {
            if (_isClosing) return;
            try
            {
                if (AchievementsDataGrid.SelectedItem == null)
                {
                    MessageBox.Show("Выберите достижение для закрепления изображения", "Внимание", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                dynamic selectedAchievement = AchievementsDataGrid.SelectedItem;
                int achievementId = selectedAchievement.Id;
                string achievementName = selectedAchievement.EventName;

                OpenFileDialog openFileDialog = new OpenFileDialog();
                openFileDialog.Title = "Выберите изображение для достижения";
                openFileDialog.Filter = "Изображения (*.png;*.jpg;*.jpeg;*.bmp;*.gif)|*.png;*.jpg;*.jpeg;*.bmp;*.gif|Все файлы (*.*)|*.*";
                openFileDialog.FilterIndex = 1;

                if (openFileDialog.ShowDialog() == true)
                {
                    string sourceFilePath = openFileDialog.FileName;
                    string fileName = $"{_studentId}-{achievementId}.png";
                    string baseDirectory = AppDomain.CurrentDomain.BaseDirectory;
                    string imagesFolder = System.IO.Path.Combine(baseDirectory, "картинки достижений");
                    string destinationFilePath = System.IO.Path.Combine(imagesFolder, fileName);

                    if (!System.IO.Directory.Exists(imagesFolder))
                        System.IO.Directory.CreateDirectory(imagesFolder);

                    if (System.IO.File.Exists(destinationFilePath))
                    {
                        var overwrite = MessageBox.Show($"Файл {fileName} уже существует.\n\nЗаменить его?",
                            "Подтверждение замены", MessageBoxButton.YesNo, MessageBoxImage.Question);
                        if (overwrite != MessageBoxResult.Yes)
                            return;
                        System.IO.File.Delete(destinationFilePath);
                    }

                    System.IO.File.Copy(sourceFilePath, destinationFilePath);

                    MessageBox.Show($"Изображение успешно закреплено!\n\n" +
                                   $"Файл: {fileName}\n" +
                                   $"Достижение: {achievementName}\n" +
                                   $"Папка: {imagesFolder}",
                                   "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при закреплении изображения: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void AchievementsDataGrid_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            btnViewImage.IsEnabled = AchievementsDataGrid.SelectedItem != null;
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
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
    }
}
