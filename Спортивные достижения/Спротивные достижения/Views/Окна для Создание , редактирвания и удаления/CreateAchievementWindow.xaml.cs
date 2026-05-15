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
    /// Логика взаимодействия для CreateAchievementWindow.xaml
    /// </summary>
    public partial class CreateAchievementWindow : Window
    {
        private SportAchievementDBEntities _context;
        private int _studentId;
        public event Action<int, string, string, string, bool, int?> AchievementCreated;
        public CreateAchievementWindow(SportAchievementDBEntities context, int studentId)
        {
            InitializeComponent();
            _context = context;
            _studentId = studentId;
            EventDatePicker.SelectedDate = DateTime.Now;
            IssueDatePicker.SelectedDate = DateTime.Now;
        }

        private void IsTeamCheckBox_Checked(object sender, RoutedEventArgs e)
        {
            TeamSizePanel.Visibility = Visibility.Visible;
        }

        private void IsTeamCheckBox_Unchecked(object sender, RoutedEventArgs e)
        {
            TeamSizePanel.Visibility = Visibility.Collapsed;
            TeamSizeTextBox.Text = "";
        }

        private bool Validate()
        {
            if (string.IsNullOrWhiteSpace(EventNameTextBox.Text))
            {
                ShowMessage("Введите название мероприятия");
                EventNameTextBox.Focus();
                return false;
            }
            if (string.IsNullOrWhiteSpace(SportTypeTextBox.Text))
            {
                ShowMessage("Введите вид спорта");
                SportTypeTextBox.Focus();
                return false;
            }
            if (LevelComboBox.SelectedItem == null)
            {
                ShowMessage("Выберите уровень соревнования");
                LevelComboBox.Focus();
                return false;
            }
            if (string.IsNullOrWhiteSpace(PlaceTextBox.Text))
            {
                ShowMessage("Введите занятое место");
                PlaceTextBox.Focus();
                return false;
            }
            if (!int.TryParse(PlaceTextBox.Text, out int place) || place < 1 || place > 50)
            {
                ShowMessage("Занятое место должно быть числом от 1 до 50");
                PlaceTextBox.Focus();
                return false;
            }
            if (string.IsNullOrWhiteSpace(VenueTextBox.Text))
            {
                ShowMessage("Введите место проведения");
                VenueTextBox.Focus();
                return false;
            }
            if (EventDatePicker.SelectedDate == null)
            {
                ShowMessage("Выберите дату проведения");
                EventDatePicker.Focus();
                return false;
            }
            if (IssueDatePicker.SelectedDate == null)
            {
                ShowMessage("Выберите дату выдачи");
                IssueDatePicker.Focus();
                return false;
            }
            if (IsTeamCheckBox.IsChecked == true && string.IsNullOrWhiteSpace(TeamSizeTextBox.Text))
            {
                ShowMessage("Введите численность команды");
                TeamSizeTextBox.Focus();
                return false;
            }
            if (IsTeamCheckBox.IsChecked == true && (!int.TryParse(TeamSizeTextBox.Text, out int teamSize) || teamSize < 1))
            {
                ShowMessage("Численность команды должна быть положительным числом");
                TeamSizeTextBox.Focus();
                return false;
            }
            return true;
        }

        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (!Validate()) return;

                bool isTeam = IsTeamCheckBox.IsChecked == true;
                int? teamSize = isTeam ? int.Parse(TeamSizeTextBox.Text) : (int?)null;
                int place = int.Parse(PlaceTextBox.Text);

                // Создаём запись в таблице Достижение (общая информация)
                var achievement = new Достижение
                {
                    Название_мероприятия = EventNameTextBox.Text.Trim(),
                    Название_вида_спорта = SportTypeTextBox.Text.Trim(),
                    Уровень_соревнования = ((ComboBoxItem)LevelComboBox.SelectedItem).Content.ToString(),
                    Командная_игра = isTeam,
                    Численность_команды = teamSize
                };

                _context.Достижение.Add(achievement);
                _context.SaveChanges();

                // Создаём связь студента с достижением (индивидуальные данные)
                var studentAchievement = new Студент_Достижение
                {
                    ID_Студент = _studentId,
                    ID_Достижение = achievement.ID_достижение,
                    Занятое_место = place,
                    Место_проведения = VenueTextBox.Text.Trim(),
                    Дата_проведения = EventDatePicker.SelectedDate.Value,
                    Дата_выдачи = IssueDatePicker.SelectedDate.Value
                };

                _context.Студент_Достижение.Add(studentAchievement);
                _context.SaveChanges();
                AchievementCreated?.Invoke(
                    achievement.ID_достижение,
                    achievement.Название_мероприятия,
                    achievement.Название_вида_спорта,
                    achievement.Уровень_соревнования,
                    achievement.Командная_игра,
                    achievement.Численность_команды
                );
                MessageBox.Show("Достижение успешно создано!", "Успех",
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
