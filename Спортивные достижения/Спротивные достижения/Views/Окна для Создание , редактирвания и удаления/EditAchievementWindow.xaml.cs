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
    /// Логика взаимодействия для EditAchievementWindow.xaml
    /// </summary>
    public partial class EditAchievementWindow : Window
    {
        private SportAchievementDBEntities _context;
        private int _achievementId;
        private int _studentId;
        public event Action<int, string, string, string, bool, int?> AchievementUpdated;
        public EditAchievementWindow(SportAchievementDBEntities context, dynamic achievement, int studentId)
        {
            InitializeComponent();
            _context = context;
            _achievementId = achievement.Id;
            _studentId = studentId;
            LoadAchievementData(achievement);
        }

        private void LoadAchievementData(dynamic achievement)
        {
            IdTextBox.Text = achievement.Id.ToString();
            EventNameTextBox.Text = achievement.EventName;
            SportTypeTextBox.Text = achievement.SportType;

            // Установка уровня
            foreach (ComboBoxItem item in LevelComboBox.Items)
            {
                if (item.Content.ToString() == achievement.Level)
                {
                    LevelComboBox.SelectedItem = item;
                    break;
                }
            }

            // Загружаем данные из связующей таблицы
            var studentAchievement = _context.Студент_Достижение
                .FirstOrDefault(sa => sa.ID_Студент == _studentId && sa.ID_Достижение == _achievementId);

            if (studentAchievement != null)
            {
                PlaceTextBox.Text = studentAchievement.Занятое_место.ToString();
                VenueTextBox.Text = studentAchievement.Место_проведения;
                EventDatePicker.SelectedDate = studentAchievement.Дата_проведения;
                IssueDatePicker.SelectedDate = studentAchievement.Дата_выдачи;
            }

            bool isTeam = achievement.IsTeam == "Да";
            IsTeamCheckBox.IsChecked = isTeam;

            if (isTeam && achievement.TeamSize != "-")
            {
                TeamSizeTextBox.Text = achievement.TeamSize;
                TeamSizePanel.Visibility = Visibility.Visible;
            }
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

                var achievement = _context.Достижение.Find(_achievementId);
                if (achievement == null)
                {
                    ShowMessage("Достижение не найдено");
                    return;
                }

                bool isTeam = IsTeamCheckBox.IsChecked == true;
                int? teamSize = isTeam ? int.Parse(TeamSizeTextBox.Text) : (int?)null;
                int place = int.Parse(PlaceTextBox.Text);

                achievement.Название_мероприятия = EventNameTextBox.Text.Trim();
                achievement.Название_вида_спорта = SportTypeTextBox.Text.Trim();
                achievement.Уровень_соревнования = ((ComboBoxItem)LevelComboBox.SelectedItem).Content.ToString();
                achievement.Командная_игра = isTeam;
                achievement.Численность_команды = teamSize;

                var studentAchievement = _context.Студент_Достижение
                    .FirstOrDefault(sa => sa.ID_Студент == _studentId && sa.ID_Достижение == _achievementId);

                if (studentAchievement != null)
                {
                    studentAchievement.Занятое_место = place;
                    studentAchievement.Место_проведения = VenueTextBox.Text.Trim();
                    studentAchievement.Дата_проведения = EventDatePicker.SelectedDate.Value;
                    studentAchievement.Дата_выдачи = IssueDatePicker.SelectedDate.Value;
                }
                else
                {
                    // Если связи нет, создаём её
                    _context.Студент_Достижение.Add(new Студент_Достижение
                    {
                        ID_Студент = _studentId,
                        ID_Достижение = _achievementId,
                        Занятое_место = place,
                        Место_проведения = VenueTextBox.Text.Trim(),
                        Дата_проведения = EventDatePicker.SelectedDate.Value,
                        Дата_выдачи = IssueDatePicker.SelectedDate.Value
                    });
                }

                _context.SaveChanges();
                AchievementUpdated?.Invoke(
                    achievement.ID_достижение,
                    achievement.Название_мероприятия,
                    achievement.Название_вида_спорта,
                    achievement.Уровень_соревнования,
                    achievement.Командная_игра,
                    achievement.Численность_команды
                );
                MessageBox.Show("Достижение успешно обновлено!", "Успех",
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
