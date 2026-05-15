using Microsoft.Win32;
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
    /// Логика взаимодействия для ReportWindow.xaml
    /// </summary>
    public partial class ReportWindow : Window
    {
        private SportAchievementDBEntities _context;
        private List<ReportItem> _reportData;

        public ReportWindow(SportAchievementDBEntities context)
        {
            InitializeComponent();
            _context = context;

            StartDatePicker.SelectedDate = DateTime.Now.AddMonths(-1);
            EndDatePicker.SelectedDate = DateTime.Now;
        }

        private void GenerateReportButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (StartDatePicker.SelectedDate == null || EndDatePicker.SelectedDate == null)
                {
                    DateStatusTextBlock.Text = "Выберите начальную и конечную дату";
                    return;
                }

                DateTime startDate = StartDatePicker.SelectedDate.Value;
                DateTime endDate = EndDatePicker.SelectedDate.Value.AddDays(1).AddSeconds(-1);

                if (startDate > endDate)
                {
                    DateStatusTextBlock.Text = "Дата начала не может быть позже даты окончания";
                    return;
                }

                DateStatusTextBlock.Text = $"Формирование отчета за период: {startDate:dd.MM.yyyy} - {endDate:dd.MM.yyyy}";

                _reportData = new List<ReportItem>();

                // Сбор информации из журнала аудита
                CollectAuditData(startDate, endDate);

                // Сортировка по дате
                _reportData = _reportData.OrderByDescending(r => r.Date).ToList();

                lvReport.ItemsSource = _reportData;

                int createCount = _reportData.Count(r => r.OperationType == "Создание");
                int editCount = _reportData.Count(r => r.OperationType == "Редактирование");
                int deleteCount = _reportData.Count(r => r.OperationType == "Удаление");

                SummaryTextBlock.Text = $"Всего операций: {_reportData.Count} | " +
                                       $"Создано: {createCount} | " +
                                       $"Изменено: {editCount} | " +
                                       $"Удалено: {deleteCount}";

                ExportButton.IsEnabled = _reportData.Any();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка формирования отчета: {ex.Message}",
                    "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void CollectAuditData(DateTime startDate, DateTime endDate)
        {
            try
            {
                if (_context.Журнал_аудита != null)
                {
                    var items = _context.Журнал_аудита
                        .Where(a => a.Дата_операции >= startDate && a.Дата_операции <= endDate)
                        .OrderByDescending(a => a.Дата_операции)
                        .ToList();

                    foreach (var item in items)
                    {
                        _reportData.Add(new ReportItem
                        {
                            Id = item.ID_записи,
                            OperationType = item.Тип_операции,
                            Date = item.Дата_операции,
                            UserName = item.Пользователь,
                            UserRole = item.Роль,
                            ObjectName = item.Имя_таблицы,
                            Details = item.Детали ?? ""
                        });
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Ошибка сбора данных: {ex.Message}");
            }
        }

        private void ReportScrollViewer_PreviewMouseWheel(object sender, System.Windows.Input.MouseWheelEventArgs e)
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

        private void ExportButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (_reportData == null || !_reportData.Any())
                {
                    MessageBox.Show("Нет данных для экспорта", "Внимание",
                        MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                SaveFileDialog saveFileDialog = new SaveFileDialog();
                saveFileDialog.Filter = "CSV файл (*.csv)|*.csv|Excel файл (*.xls)|*.xls";
                saveFileDialog.DefaultExt = ".csv";
                saveFileDialog.FileName = $"Отчет_{DateTime.Now:yyyyMMdd_HHmmss}";

                if (saveFileDialog.ShowDialog() == true)
                {
                    string delimiter = ";";

                    using (System.IO.StreamWriter writer = new System.IO.StreamWriter(saveFileDialog.FileName, false, System.Text.Encoding.UTF8))
                    {
                        writer.Write('\uFEFF');
                        writer.WriteLine($"ID{delimiter}Тип операции{delimiter}Дата{delimiter}Пользователь{delimiter}Роль{delimiter}Объект{delimiter}Детали");

                        foreach (var item in _reportData)
                        {
                            string formattedDate = item.Date.ToString("yyyy-MM-dd");
                            string userName = EscapeCsvField(item.UserName);
                            string details = EscapeCsvField(item.Details);

                            writer.WriteLine($"{item.Id}{delimiter}{item.OperationType}{delimiter}{formattedDate}{delimiter}{userName}{delimiter}{item.UserRole}{delimiter}{item.ObjectName}{delimiter}{details}");
                        }

                        writer.WriteLine();
                        writer.WriteLine($"Итоги:{delimiter}");
                        writer.WriteLine($"Всего операций:{delimiter}{_reportData.Count}");
                        writer.WriteLine($"Создано:{delimiter}{_reportData.Count(r => r.OperationType == "Создание")}");
                        writer.WriteLine($"Изменено:{delimiter}{_reportData.Count(r => r.OperationType == "Редактирование")}");
                        writer.WriteLine($"Удалено:{delimiter}{_reportData.Count(r => r.OperationType == "Удаление")}");
                        writer.WriteLine($"Период:{delimiter}{StartDatePicker.SelectedDate:dd.MM.yyyy}{delimiter}-{delimiter}{EndDatePicker.SelectedDate:dd.MM.yyyy}");
                    }

                    MessageBox.Show($"Отчет успешно сохранен!\n\n{saveFileDialog.FileName}",
                        "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка экспорта: {ex.Message}", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private string EscapeCsvField(string field)
        {
            if (string.IsNullOrEmpty(field))
                return "";

            if (field.Contains(";") || field.Contains("\"") || field.Contains("\n") || field.Contains(","))
            {
                field = field.Replace("\"", "\"\"");
                return $"\"{field}\"";
            }
            return field;
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}

