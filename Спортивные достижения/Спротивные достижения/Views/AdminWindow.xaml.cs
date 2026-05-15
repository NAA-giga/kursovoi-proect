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
using Спротивные_достижения.Views.Окна_для_Создание___редактирвания_и_удаления;
using Спротивные_достижения.Views.Окна_для_Создание_и_редактирвания;

namespace Спротивные_достижения.Views
{
    /// <summary>
    /// Логика взаимодействия для AdminWindow.xaml
    /// </summary>
    public partial class AdminWindow : Window
    {
        private SportAchievementDBEntities _context;
        private UserModel _currentUser;
        private bool _isLoggingOut = false;

        public AdminWindow(UserModel user)
        {
            InitializeComponent();
            _currentUser = user;
            txtUserName.Text = _currentUser.FullName;

            try
            {
                _context = new SportAchievementDBEntities();
                LoadStudents();
                lvUsers.SelectionChanged += LvUsers_SelectionChanged;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка инициализации: {ex.Message}",
                    "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        // ==================== ЗАГРУЗКА ДАННЫХ ====================

        private void LoadStudents()
        {
            try
            {
                if (_context == null) return;

                var studentsList = _context.Студент.ToList();
                var groups = _context.Группа.ToDictionary(g => g.ID_Группа, g => g);
                var specialties = _context.Специальность.ToDictionary(s => s.ID_Специальность, s => s);
                var courses = _context.Курс.ToDictionary(c => c.ID_Курс, c => c);
                var teachers = _context.Классный_руководитель.ToDictionary(t => t.ID_Классный_руководитель, t => t);

                var students = studentsList.Select(s => new
                {
                    Id = s.ID_Студент,
                    LastName = s.Фамилия ?? "",
                    FirstName = s.Имя ?? "",
                    MiddleName = s.Отчество ?? "",
                    Login = s.Логин ?? "",
                    Email = s.Электронная_почта ?? "",
                    Phone = s.Номер_телефона ?? "",
                    Group = groups.ContainsKey(s.ID_Группа) ? (groups[s.ID_Группа].Название ?? "") : "",
                    Specialty = (groups.ContainsKey(s.ID_Группа) && specialties.ContainsKey(groups[s.ID_Группа].ID_Специальность)) ?
                               (specialties[groups[s.ID_Группа].ID_Специальность].Название_специальности ?? "") : "",
                    Course = (groups.ContainsKey(s.ID_Группа) && courses.ContainsKey(groups[s.ID_Группа].ID_Курс)) ?
                            courses[groups[s.ID_Группа].ID_Курс].Номер_курса : 0,
                    EnrollmentDate = s.Дата_зачисления,
                    TeacherName = teachers.ContainsKey(s.ID_Классный_руководитель) ?
                                 $"{teachers[s.ID_Классный_руководитель].Фамилия} {teachers[s.ID_Классный_руководитель].Имя}".Trim() : "",
                    Department = "",
                    Faculty = "",
                    StudentCount = 0
                }).ToList();

                lvUsers.ItemsSource = students;
                txtRecordCount.Text = $"Всего: {students.Count}";
                SetColumnsVisibilityForStudents();
                UpdateViewAchievementsButtonState();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки студентов: {ex.Message}",
                    "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void LoadTeachers()
        {
            try
            {
                if (_context == null) return;

                var teachersList = _context.Классный_руководитель.ToList();
                var groups = _context.Группа.ToDictionary(g => g.ID_Группа, g => g);
                var departments = _context.Кафедра.ToDictionary(d => d.ID_Кафедра, d => d);
                var faculties = _context.Факультет.ToDictionary(f => f.ID_Факультет, f => f);

                var teachers = teachersList.Select(t => new
                {
                    Id = t.ID_Классный_руководитель,
                    LastName = t.Фамилия ?? "",
                    FirstName = t.Имя ?? "",
                    MiddleName = t.Отчество ?? "",
                    Login = t.Логин ?? "",
                    Email = t.Электронная_почта ?? "",
                    Phone = t.Номер_телефона ?? "",
                    Group = groups.ContainsKey(t.ID_Группа) ? (groups[t.ID_Группа].Название ?? "") : "",
                    Specialty = "",
                    Course = 0,
                    EnrollmentDate = (DateTime?)null,
                    TeacherName = "",
                    Department = departments.ContainsKey(t.ID_Кафедра) ? (departments[t.ID_Кафедра].Название_кафедры ?? "") : "",
                    Faculty = (departments.ContainsKey(t.ID_Кафедра) && faculties.ContainsKey(departments[t.ID_Кафедра].ID_Факультет)) ?
                             faculties[departments[t.ID_Кафедра].ID_Факультет].Название_факультета ?? "" : "",
                    StudentCount = _context.Студент.Count(s => s.ID_Классный_руководитель == t.ID_Классный_руководитель)
                }).ToList();

                lvUsers.ItemsSource = teachers;
                txtRecordCount.Text = $"Всего: {teachers.Count}";
                SetColumnsVisibilityForTeachers();
                UpdateViewAchievementsButtonState();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки преподавателей: {ex.Message}",
                    "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void SetColumnsVisibilityForStudents()
        {
            var gridView = new GridView();
            gridView.Columns.Add(new GridViewColumn { Header = "ID", DisplayMemberBinding = new System.Windows.Data.Binding("Id"), Width = 50 });
            gridView.Columns.Add(new GridViewColumn { Header = "Фамилия", DisplayMemberBinding = new System.Windows.Data.Binding("LastName"), Width = 100 });
            gridView.Columns.Add(new GridViewColumn { Header = "Имя", DisplayMemberBinding = new System.Windows.Data.Binding("FirstName"), Width = 100 });
            gridView.Columns.Add(new GridViewColumn { Header = "Отчество", DisplayMemberBinding = new System.Windows.Data.Binding("MiddleName"), Width = 100 });
            gridView.Columns.Add(new GridViewColumn { Header = "Логин", DisplayMemberBinding = new System.Windows.Data.Binding("Login"), Width = 120 });
            gridView.Columns.Add(new GridViewColumn { Header = "Email", DisplayMemberBinding = new System.Windows.Data.Binding("Email"), Width = 180 });
            gridView.Columns.Add(new GridViewColumn { Header = "Телефон", DisplayMemberBinding = new System.Windows.Data.Binding("Phone"), Width = 110 });
            gridView.Columns.Add(new GridViewColumn { Header = "Группа", DisplayMemberBinding = new System.Windows.Data.Binding("Group"), Width = 100 });
            gridView.Columns.Add(new GridViewColumn { Header = "Специальность", DisplayMemberBinding = new System.Windows.Data.Binding("Specialty"), Width = 150 });
            gridView.Columns.Add(new GridViewColumn { Header = "Курс", Width = 60, CellTemplate = CreateCenteredTextBlockTemplate("Course") });
            gridView.Columns.Add(new GridViewColumn { Header = "Дата зачисления", DisplayMemberBinding = new System.Windows.Data.Binding("EnrollmentDate") { StringFormat = "dd.MM.yyyy" }, Width = 100 });

            lvUsers.View = gridView;
        }

        private void SetColumnsVisibilityForTeachers()
        {
            var gridView = new GridView();
            gridView.Columns.Add(new GridViewColumn { Header = "ID", DisplayMemberBinding = new System.Windows.Data.Binding("Id"), Width = 50 });
            gridView.Columns.Add(new GridViewColumn { Header = "Фамилия", DisplayMemberBinding = new System.Windows.Data.Binding("LastName"), Width = 100 });
            gridView.Columns.Add(new GridViewColumn { Header = "Имя", DisplayMemberBinding = new System.Windows.Data.Binding("FirstName"), Width = 100 });
            gridView.Columns.Add(new GridViewColumn { Header = "Отчество", DisplayMemberBinding = new System.Windows.Data.Binding("MiddleName"), Width = 100 });
            gridView.Columns.Add(new GridViewColumn { Header = "Логин", DisplayMemberBinding = new System.Windows.Data.Binding("Login"), Width = 120 });
            gridView.Columns.Add(new GridViewColumn { Header = "Email", DisplayMemberBinding = new System.Windows.Data.Binding("Email"), Width = 180 });
            gridView.Columns.Add(new GridViewColumn { Header = "Телефон", DisplayMemberBinding = new System.Windows.Data.Binding("Phone"), Width = 110 });
            gridView.Columns.Add(new GridViewColumn { Header = "Группа", DisplayMemberBinding = new System.Windows.Data.Binding("Group"), Width = 100 });
            gridView.Columns.Add(new GridViewColumn { Header = "Кафедра", DisplayMemberBinding = new System.Windows.Data.Binding("Department"), Width = 150 });
            gridView.Columns.Add(new GridViewColumn { Header = "Факультет", DisplayMemberBinding = new System.Windows.Data.Binding("Faculty"), Width = 150 });
            gridView.Columns.Add(new GridViewColumn { Header = "Кол-во студентов", Width = 100, CellTemplate = CreateCenteredTextBlockTemplate("StudentCount") });

            lvUsers.View = gridView;
        }

        private DataTemplate CreateCenteredTextBlockTemplate(string propertyName)
        {
            var template = new DataTemplate();
            var factory = new FrameworkElementFactory(typeof(TextBlock));
            factory.SetBinding(TextBlock.TextProperty, new System.Windows.Data.Binding(propertyName));
            factory.SetValue(TextBlock.HorizontalAlignmentProperty, HorizontalAlignment.Center);
            factory.SetValue(TextBlock.VerticalAlignmentProperty, VerticalAlignment.Center);
            template.VisualTree = factory;
            return template;
        }

        // ==================== ВСПОМОГАТЕЛЬНЫЕ МЕТОДЫ ====================

        private void CmbUserType_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (cmbUserType.SelectedItem == null) return;

            ComboBoxItem selectedItem = (ComboBoxItem)cmbUserType.SelectedItem;
            string selectedType = selectedItem.Content.ToString();

            if (selectedType == "Студенты")
                LoadStudents();
            else if (selectedType == "Классные руководители")
                LoadTeachers();
        }

        private string GetSelectedUserType()
        {
            if (cmbUserType.SelectedItem == null) return "Student";
            ComboBoxItem selectedItem = (ComboBoxItem)cmbUserType.SelectedItem;
            return selectedItem.Content.ToString() == "Студенты" ? "Student" : "Teacher";
        }

        private void RefreshUserList()
        {
            string selectedType = GetSelectedUserType();
            if (selectedType == "Student")
                LoadStudents();
            else
                LoadTeachers();
        }

        private void LvUsers_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            UpdateViewAchievementsButtonState();
        }

        private void UpdateViewAchievementsButtonState()
        {
            string selectedType = GetSelectedUserType();
            if (selectedType == "Student" && lvUsers.SelectedItem != null)
            {
                btnViewAchievements.IsEnabled = true;
                btnViewAchievements.ToolTip = "Просмотреть достижения студента";
            }
            else
            {
                btnViewAchievements.IsEnabled = false;
                btnViewAchievements.ToolTip = selectedType != "Student" ?
                    "Просмотр достижений доступен только для студентов" :
                    "Выберите студента для просмотра достижений";
            }
        }

        // ==================== ЖУРНАЛ АУДИТА ====================

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

        // ==================== КНОПКИ УПРАВЛЕНИЯ ====================

        private void BtnCreate_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                string selectedType = GetSelectedUserType();
                if (selectedType == "Student")
                {
                    var createWindow = new CreateStudentWindow(_context);
                    createWindow.StudentCreated += (studentId, studentName, studentLogin, groupName, teacherName) =>
                    {
                        AddAuditLog("Создание", "Студент", studentId,
                            $"Создан студент: {studentName}, логин: {studentLogin}, группа: {groupName}, классный руководитель: {teacherName}");
                    };
                    if (createWindow.ShowDialog() == true)
                    {
                        RefreshUserList();
                        MessageBox.Show("Студент успешно создан!", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
                    }
                }
                else if (selectedType == "Teacher")
                {
                    var createWindow = new CreateTeacherWindow(_context, _currentUser);
                    createWindow.TeacherCreated += (teacherId, teacherName, teacherLogin, departmentName, groupName) =>
                    {
                        AddAuditLog("Создание", "Классный_руководитель", teacherId,
                            $"Создан преподаватель: {teacherName}, логин: {teacherLogin}, кафедра: {departmentName}, закрепленная группа: {groupName}");
                    };
                    if (createWindow.ShowDialog() == true)
                    {
                        RefreshUserList();
                        MessageBox.Show("Преподаватель успешно создан!", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при создании пользователя: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void BtnEdit_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (lvUsers.SelectedItem == null)
                {
                    MessageBox.Show("Выберите пользователя для редактирования", "Внимание", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                string selectedType = GetSelectedUserType();
                dynamic selectedUser = lvUsers.SelectedItem;

                if (selectedType == "Student")
                {
                    var editWindow = new EditStudentWindow(_context, selectedUser);
                    editWindow.StudentUpdated += (studentId, studentName, groupName, teacherName) =>
                    {
                        AddAuditLog("Редактирование", "Студент", studentId,
                            $"Обновлены данные студента: {studentName}, группа: {groupName}, классный руководитель: {teacherName}");
                    };
                    if (editWindow.ShowDialog() == true)
                    {
                        RefreshUserList();
                        MessageBox.Show("Данные студента успешно обновлены!", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
                    }
                }
                else if (selectedType == "Teacher")
                {
                    var editWindow = new EditTeacherWindow(_context, selectedUser);
                    editWindow.TeacherUpdated += (teacherId, teacherName, departmentName, groupName) =>
                    {
                        AddAuditLog("Редактирование", "Классный_руководитель", teacherId,
                            $"Обновлены данные преподавателя: {teacherName}, кафедра: {departmentName}, закрепленная группа: {groupName}");
                    };
                    if (editWindow.ShowDialog() == true)
                    {
                        RefreshUserList();
                        MessageBox.Show("Данные преподавателя успешно обновлены!", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при редактировании пользователя: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void BtnDelete_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (lvUsers.SelectedItem == null)
                {
                    MessageBox.Show("Выберите пользователя для удаления", "Внимание", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                var result = MessageBox.Show("Вы уверены, что хотите удалить выбранного пользователя?\n\nВНИМАНИЕ: Это действие необратимо!",
                    "Подтверждение удаления", MessageBoxButton.YesNo, MessageBoxImage.Question);

                if (result == MessageBoxResult.Yes)
                {
                    dynamic selectedUser = lvUsers.SelectedItem;
                    string selectedType = GetSelectedUserType();

                    if (selectedType == "Student")
                    {
                        var student = _context.Студент.Find((int)selectedUser.Id);
                        if (student != null)
                        {
                            AddAuditLog("Удаление", "Студент", student.ID_Студент,
                                $"Удален студент: {student.Фамилия} {student.Имя} {student.Отчество}".Trim() +
                                $", группа: {student.Группа?.Название ?? "Не указана"}");
                            _context.Студент.Remove(student);
                            _context.SaveChanges();
                            MessageBox.Show("Студент успешно удален!", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
                        }
                    }
                    else if (selectedType == "Teacher")
                    {
                        var teacher = _context.Классный_руководитель.Find((int)selectedUser.Id);
                        if (teacher != null)
                        {
                            AddAuditLog("Удаление", "Классный_руководитель", teacher.ID_Классный_руководитель,
                                $"Удален преподаватель: {teacher.Фамилия} {teacher.Имя} {teacher.Отчество}".Trim() +
                                $", кафедра: {teacher.Кафедра?.Название_кафедры ?? "Не указана"}");
                            _context.Классный_руководитель.Remove(teacher);
                            _context.SaveChanges();
                            MessageBox.Show("Преподаватель успешно удален!", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
                        }
                    }
                    RefreshUserList();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при удалении пользователя: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void BtnViewAchievements_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (lvUsers.SelectedItem == null)
                {
                    MessageBox.Show("Выберите студента для просмотра достижений", "Внимание", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                string selectedType = GetSelectedUserType();
                if (selectedType != "Student")
                {
                    MessageBox.Show("Просмотр достижений доступен только для студентов", "Внимание", MessageBoxButton.OK, MessageBoxImage.Information);
                    return;
                }

                dynamic selectedUser = lvUsers.SelectedItem;
                int studentId = selectedUser.Id;
                string studentName = $"{selectedUser.LastName} {selectedUser.FirstName} {selectedUser.MiddleName}".Trim();

                var achievementsWindow = new StudentAchievementsWindow(_context, studentId, studentName, _currentUser);
                achievementsWindow.ShowDialog();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void BtnGenerateReport_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var reportWindow = new ReportWindow(_context);
                reportWindow.ShowDialog();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void BtnImportStudents_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var openFileDialog = new OpenFileDialog();
                openFileDialog.Title = "Выберите CSV файл с данными студентов";
                openFileDialog.Filter = "CSV файлы (*.csv)|*.csv|Текстовые файлы (*.txt)|*.txt|Все файлы (*.*)|*.*";
                openFileDialog.FilterIndex = 1;

                if (openFileDialog.ShowDialog() == true)
                {
                    ImportStudentsFromCSV(openFileDialog.FileName);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при импорте: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void BtnDeleteGroup_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var deleteGroupWindow = new DeleteGroupWindow(_context, _currentUser);
                if (deleteGroupWindow.ShowDialog() == true)
                {
                    RefreshUserList();
                    MessageBox.Show("Группа и все связанные данные успешно удалены!",
                        "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
                }
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

        // ==================== ИМПОРТ СТУДЕНТОВ ====================

        private void ImportStudentsFromCSV(string filePath)
        {
            try
            {
                var lines = File.ReadAllLines(filePath, Encoding.UTF8);
                if (lines.Length == 0)
                {
                    MessageBox.Show("Файл пуст", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                int startLine = 0;
                if (lines[0].Contains("Фамилия") || lines[0].Contains("LastName"))
                    startLine = 1;

                int successCount = 0, errorCount = 0;
                var errors = new List<string>();

                // Загружаем справочники
                var allGroups = _context.Группа.ToList();
                var allTeachers = _context.Классный_руководитель.ToList();
                var allDepartments = _context.Кафедра.ToList();
                var allFaculties = _context.Факультет.ToList();
                var allSpecialties = _context.Специальность.ToList();
                var allCourses = _context.Курс.ToList();

                for (int i = startLine; i < lines.Length; i++)
                {
                    if (string.IsNullOrWhiteSpace(lines[i]))
                        continue;

                    try
                    {
                        var values = ParseCSVLine(lines[i]);

                        // Минимальное количество полей: Фамилия, Имя, Логин, Пароль, Email, Телефон
                        if (values.Length < 6)
                        {
                            errors.Add($"Строка {i + 1}: недостаточно данных (требуется минимум 6 полей)");
                            errorCount++;
                            continue;
                        }

                        // Формат CSV: Фамилия,Имя,Отчество,Логин,Пароль,Email,Телефон,Группа,Специальность,Курс,Классный_руководитель,Кафедра_классного_руководителя,Факультет
                        string lastName = values[0].Trim();
                        string firstName = values[1].Trim();
                        string middleName = values.Length > 2 ? values[2].Trim() : null;
                        string login = values[3].Trim();
                        string password = values[4].Trim();
                        string email = values[5].Trim();
                        string phone = values.Length > 6 ? values[6].Trim() : "";
                        string groupName = values.Length > 7 ? values[7].Trim() : null;
                        string specialtyName = values.Length > 8 ? values[8].Trim() : null;
                        string courseNumber = values.Length > 9 ? values[9].Trim() : null;
                        string teacherName = values.Length > 10 ? values[10].Trim() : null;
                        string departmentName = values.Length > 11 ? values[11].Trim() : null;
                        string facultyName = values.Length > 12 ? values[12].Trim() : null;

                        // Проверка обязательных полей
                        if (string.IsNullOrEmpty(lastName) || string.IsNullOrEmpty(firstName) ||
                            string.IsNullOrEmpty(login) || string.IsNullOrEmpty(password) ||
                            string.IsNullOrEmpty(email) || string.IsNullOrEmpty(phone))
                        {
                            errors.Add($"Строка {i + 1}: обязательные поля не заполнены");
                            errorCount++;
                            continue;
                        }

                        // Проверка на существование студента
                        var existingStudent = _context.Студент
                            .FirstOrDefault(s => s.Логин == login || s.Электронная_почта == email);
                        if (existingStudent != null)
                        {
                            errors.Add($"Строка {i + 1}: студент с логином '{login}' или email '{email}' уже существует");
                            errorCount++;
                            continue;
                        }

                        // ==================== ПОИСК ИЛИ СОЗДАНИЕ ФАКУЛЬТЕТА ====================
                        int? facultyId = null;
                        if (!string.IsNullOrEmpty(facultyName))
                        {
                            var faculty = allFaculties.FirstOrDefault(f => f.Название_факультета == facultyName);
                            if (faculty == null)
                            {
                                faculty = new Факультет { Название_факультета = facultyName };
                                _context.Факультет.Add(faculty);
                                _context.SaveChanges();
                                allFaculties = _context.Факультет.ToList();
                                facultyId = faculty.ID_Факультет;

                                AddAuditLog("Создание", "Факультет", faculty.ID_Факультет,
                                    $"Создан факультет: {facultyName}");
                            }
                            else
                            {
                                facultyId = faculty.ID_Факультет;
                            }
                        }

                        // ==================== ПОИСК ИЛИ СОЗДАНИЕ СПЕЦИАЛЬНОСТИ ====================
                        int? specialtyId = null;
                        if (!string.IsNullOrEmpty(specialtyName) && facultyId.HasValue)
                        {
                            var specialty = allSpecialties.FirstOrDefault(s => s.Название_специальности == specialtyName);
                            if (specialty == null)
                            {
                                specialty = new Специальность
                                {
                                    Код_специальности = $"09.03.01",
                                    Название_специальности = specialtyName,
                                    ID_Факультет = facultyId.Value
                                };
                                _context.Специальность.Add(specialty);
                                _context.SaveChanges();
                                allSpecialties = _context.Специальность.ToList();
                                specialtyId = specialty.ID_Специальность;

                                AddAuditLog("Создание", "Специальность", specialty.ID_Специальность,
                                    $"Создана специальность: {specialtyName} на факультете {facultyName}");
                            }
                            else
                            {
                                specialtyId = specialty.ID_Специальность;
                            }
                        }

                        // ==================== ПОИСК ИЛИ СОЗДАНИЕ КУРСА ====================
                        int? courseId = null;
                        if (!string.IsNullOrEmpty(courseNumber))
                        {
                            int courseNum = int.Parse(courseNumber);
                            var course = allCourses.FirstOrDefault(c => c.Номер_курса == courseNum);
                            if (course == null)
                            {
                                course = new Курс { Номер_курса = courseNum };
                                _context.Курс.Add(course);
                                _context.SaveChanges();
                                allCourses = _context.Курс.ToList();
                                courseId = course.ID_Курс;

                                AddAuditLog("Создание", "Курс", course.ID_Курс,
                                    $"Создан курс: {courseNum}");
                            }
                            else
                            {
                                courseId = course.ID_Курс;
                            }
                        }

                        // ==================== ПОИСК ИЛИ СОЗДАНИЕ ГРУППЫ ====================
                        int? groupId = null;
                        if (!string.IsNullOrEmpty(groupName))
                        {
                            var group = allGroups.FirstOrDefault(g => g.Название == groupName);
                            if (group == null && specialtyId.HasValue && courseId.HasValue)
                            {
                                group = new Группа
                                {
                                    Название = groupName,
                                    Год_формирования = DateTime.Now.Year,
                                    ID_Специальность = specialtyId.Value,
                                    ID_Курс = courseId.Value
                                };
                                _context.Группа.Add(group);
                                _context.SaveChanges();
                                allGroups = _context.Группа.ToList();
                                groupId = group.ID_Группа;

                                AddAuditLog("Создание", "Группа", group.ID_Группа,
                                    $"Создана группа: {groupName}, специальность: {specialtyName}, курс: {courseNumber}");
                            }
                            else if (group != null)
                            {
                                groupId = group.ID_Группа;
                            }
                            else
                            {
                                errors.Add($"Строка {i + 1}: группа '{groupName}' не найдена и нет данных для ее создания");
                                errorCount++;
                                continue;
                            }
                        }

                        // ==================== ПОИСК ИЛИ СОЗДАНИЕ КАФЕДРЫ ====================
                        int? departmentId = null;
                        if (!string.IsNullOrEmpty(departmentName) && facultyId.HasValue)
                        {
                            var department = allDepartments.FirstOrDefault(d => d.Название_кафедры == departmentName);
                            if (department == null)
                            {
                                department = new Кафедра
                                {
                                    Название_кафедры = departmentName,
                                    ID_Факультет = facultyId.Value
                                };
                                _context.Кафедра.Add(department);
                                _context.SaveChanges();
                                allDepartments = _context.Кафедра.ToList();
                                departmentId = department.ID_Кафедра;

                                AddAuditLog("Создание", "Кафедра", department.ID_Кафедра,
                                    $"Создана кафедра: {departmentName} на факультете {facultyName}");
                            }
                            else
                            {
                                departmentId = department.ID_Кафедра;
                            }
                        }

                        // ==================== ПОИСК ИЛИ СОЗДАНИЕ КЛАССНОГО РУКОВОДИТЕЛЯ ====================
                        int? teacherId = null;
                        if (!string.IsNullOrEmpty(teacherName))
                        {
                            var nameParts = teacherName.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                            if (nameParts.Length >= 2)
                            {
                                var teacher = allTeachers.FirstOrDefault(t =>
                                    t.Фамилия == nameParts[0] && t.Имя == nameParts[1]);

                                if (teacher == null && departmentId.HasValue && groupId.HasValue)
                                {
                                    teacher = new Классный_руководитель
                                    {
                                        Фамилия = nameParts[0],
                                        Имя = nameParts[1],
                                        Отчество = nameParts.Length > 2 ? nameParts[2] : null,
                                        Логин = $"{nameParts[0].ToLower()}_{nameParts[1].ToLower()}",
                                        Пароль = "pass123",
                                        Электронная_почта = $"{nameParts[0].ToLower()}.{nameParts[1].ToLower()}@teacher.ru",
                                        Номер_телефона = "89161234567",
                                        ID_Группа = groupId.Value,
                                        ID_Кафедра = departmentId.Value
                                    };
                                    _context.Классный_руководитель.Add(teacher);
                                    _context.SaveChanges();
                                    allTeachers = _context.Классный_руководитель.ToList();
                                    teacherId = teacher.ID_Классный_руководитель;

                                    AddAuditLog("Создание", "Классный_руководитель", teacher.ID_Классный_руководитель,
                                        $"Создан преподаватель: {teacherName}, кафедра: {departmentName}");
                                }
                                else if (teacher != null)
                                {
                                    teacherId = teacher.ID_Классный_руководитель;
                                }
                            }
                            else
                            {
                                errors.Add($"Строка {i + 1}: некорректное ФИО классного руководителя '{teacherName}'");
                                errorCount++;
                                continue;
                            }
                        }

                        // Если группа не найдена, используем группу по умолчанию
                        if (groupId == null)
                        {
                            var defaultGroup = allGroups.FirstOrDefault();
                            if (defaultGroup != null)
                            {
                                groupId = defaultGroup.ID_Группа;
                            }
                            else
                            {
                                errors.Add($"Строка {i + 1}: нет доступных групп");
                                errorCount++;
                                continue;
                            }
                        }

                        // Если классный руководитель не найден, используем первого
                        if (teacherId == null)
                        {
                            var defaultTeacher = allTeachers.FirstOrDefault();
                            if (defaultTeacher != null)
                            {
                                teacherId = defaultTeacher.ID_Классный_руководитель;
                            }
                            else
                            {
                                errors.Add($"Строка {i + 1}: нет доступных классных руководителей");
                                errorCount++;
                                continue;
                            }
                        }

                        // Создаем студента
                        var student = new Студент
                        {
                            Фамилия = lastName,
                            Имя = firstName,
                            Отчество = string.IsNullOrEmpty(middleName) ? null : middleName,
                            Логин = login,
                            Пароль = password,
                            Электронная_почта = email,
                            Номер_телефона = phone,
                            ID_Группа = groupId.Value,
                            ID_Классный_руководитель = teacherId.Value,
                            Дата_зачисления = DateTime.Now
                        };

                        _context.Студент.Add(student);
                        _context.SaveChanges();

                        // Запись в журнал аудита о создании студента
                        AddAuditLog("Создание", "Студент", student.ID_Студент,
                            $"Создан студент: {lastName} {firstName} {middleName}".Trim() +
                            $", логин: {login}, группа: {groupName}");

                        successCount++;
                    }
                    catch (Exception ex)
                    {
                        errors.Add($"Строка {i + 1}: {ex.Message}");
                        errorCount++;
                    }
                }

                // Обновляем таблицу
                RefreshUserList();

                string message = $"Импорт завершен!\n\nУспешно добавлено: {successCount}\nОшибок: {errorCount}";
                if (errors.Count > 0 && errors.Count <= 10)
                {
                    message += $"\n\nОшибки:\n{string.Join("\n", errors)}";
                }
                else if (errors.Count > 10)
                {
                    message += $"\n\nПервые 10 ошибок:\n{string.Join("\n", errors.Take(10))}";
                }

                MessageBox.Show(message, "Результат импорта",
                    MessageBoxButton.OK,
                    errorCount > 0 ? MessageBoxImage.Warning : MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при чтении файла: {ex.Message}", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private string[] ParseCSVLine(string line)
        {
            var result = new List<string>();
            bool inQuotes = false;
            string currentField = "";

            for (int i = 0; i < line.Length; i++)
            {
                char c = line[i];
                if (c == '"')
                {
                    inQuotes = !inQuotes;
                }
                else if (c == ',' && !inQuotes)
                {
                    result.Add(currentField);
                    currentField = "";
                }
                else
                {
                    currentField += c;
                }
            }
            result.Add(currentField);

            for (int i = 0; i < result.Count; i++)
                result[i] = result[i].Trim().Trim('"');

            return result.ToArray();
        }

        // ==================== ПРОКРУТКА ТАБЛИЦЫ ====================

        private void UsersScrollViewer_PreviewMouseWheel(object sender, System.Windows.Input.MouseWheelEventArgs e)
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

        // ==================== ЗАКРЫТИЕ ОКНА ====================

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
