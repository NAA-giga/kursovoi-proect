USE [SportAchievementDB]
GO

-- =====================================================
-- 1. Заполнение справочных таблиц
-- =====================================================

-- Курс
INSERT INTO [dbo].[Курс] ([Номер_курса]) VALUES (1),(2),(3),(4),(5),(6)
GO

-- Факультет
INSERT INTO [dbo].[Факультет] ([Название_факультета]) VALUES 
(N'Факультет информационных технологий'),
(N'Факультет прикладной математики'),
(N'Факультет программирования')
GO

-- Специальность
INSERT INTO [dbo].[Специальность] ([Код_специальности], [Название_специальности], [ID_Факультет]) VALUES
(N'09.03.01', N'Информационные системы', 1),
(N'09.03.02', N'Программная инженерия', 3),
(N'09.03.03', N'Прикладная математика', 2),
(N'09.03.04', N'Сетевое администрирование', 1),
(N'09.03.05', N'Веб-разработка', 3),
(N'09.03.06', N'Базы данных', 2)
GO

-- Кафедра
INSERT INTO [dbo].[Кафедра] ([Название_кафедры], [ID_Факультет]) VALUES
(N'Кафедра информационных систем', 1),
(N'Кафедра программирования', 3),
(N'Кафедра баз данных', 2),
(N'Кафедра сетевых технологий', 1),
(N'Кафедра прикладной математики', 2),
(N'Кафедра веб-технологий', 3)
GO

-- Группа (2‑й курс, год формирования 2024)
INSERT INTO [dbo].[Группа] ([Название], [Год_формирования], [ID_Специальность], [ID_Курс]) VALUES
(N'ИС-201', 2024, 1, 2),
(N'ПР-202', 2024, 2, 2),
(N'БД-201', 2024, 6, 2),
(N'СЕТ-201', 2024, 4, 2),
(N'МАТ-201', 2024, 3, 2),
(N'ВЕБ-201', 2024, 5, 2)
GO

-- Классные руководители (по одному на группу)
INSERT INTO [dbo].[Классный_руководитель] 
    ([Фамилия], [Имя], [Отчество], [Логин], [Пароль], [Номер_телефона], [Электронная_почта], [ID_Группа], [ID_Кафедра])
VALUES
(N'Смирнов', N'Дмитрий', N'Владимирович', N'smirnov_dv', N'teacher2024_1', N'89161234501', N'smirnov.dv@university.ru', 1, 1),
(N'Кузнецова', N'Елена', N'Андреевна', N'kuznetsova_ea', N'teacher2024_2', N'89161234502', N'kuznetsova.ea@university.ru', 2, 2),
(N'Попов', N'Сергей', N'Николаевич', N'popov_sn', N'teacher2024_3', N'89161234503', N'popov.sn@university.ru', 3, 3),
(N'Васильева', N'Анна', N'Михайловна', N'vasilieva_am', N'teacher2024_4', N'89161234504', N'vasilieva.am@university.ru', 4, 4),
(N'Соколов', N'Александр', N'Игоревич', N'sokolov_ai', N'teacher2024_5', N'89161234505', N'sokolov.ai@university.ru', 5, 5),
(N'Новикова', N'Ольга', N'Дмитриевна', N'novikova_od', N'teacher2024_6', N'89161234506', N'novikova.od@university.ru', 6, 6)
GO

-- Администраторы
INSERT INTO [dbo].[Администратор] 
    ([Фамилия], [Имя], [Отчество], [Логин], [Пароль], [Номер_телефона], [Электронная_почта])
VALUES
(N'Иванов', N'Алексей', N'Петрович', N'admin_ivanov', N'admin_pass_123', N'89001234567', N'admin.ivanov@university.ru'),
(N'Петрова', N'Мария', N'Сергеевна', N'admin_petrova', N'secure_admin_456', N'89007654321', N'admin.petrova@university.ru')
GO

-- =====================================================
-- 2. Заполнение таблицы Студент (117 студентов, распределение по группам)
-- =====================================================

-- Вспомогательные списки русских имён, фамилий, отчеств
DECLARE @Surnames TABLE (ID INT IDENTITY(1,1), Surname NVARCHAR(50))
INSERT INTO @Surnames (Surname) VALUES 
(N'Абрамов'),(N'Борисов'),(N'Воронов'),(N'Григорьев'),(N'Дмитриев'),(N'Егорова'),(N'Жуков'),(N'Зайцева'),
(N'Ильин'),(N'Козлов'),(N'Лебедева'),(N'Макаров'),(N'Николаев'),(N'Орлова'),(N'Павлов'),(N'Романов'),
(N'Семенова'),(N'Тихонов'),(N'Алексеев'),(N'Баранова'),(N'Виноградов'),(N'Голубев'),(N'Дорофеев'),(N'Ершова'),
(N'Жаров'),(N'Зубков'),(N'Исаева'),(N'Кириллов'),(N'Логинова'),(N'Миронов'),(N'Нестерова'),(N'Овчинников'),
(N'Панфилова'),(N'Рогов'),(N'Савельева'),(N'Тихомиров'),(N'Андреев'),(N'Белова'),(N'Волков'),(N'Гусева'),
(N'Давыдов'),(N'Ефимов'),(N'Журавлев'),(N'Захарова'),(N'Иванова'),(N'Калинин'),(N'Лазарев'),(N'Медведева'),
(N'Назаров'),(N'Осипова'),(N'Поляков'),(N'Рыбакова'),(N'Соловьев'),(N'Тарасова'),(N'Ушаков'),(N'Федорова'),
(N'Антонов'),(N'Богданова'),(N'Власов'),(N'Гордеева'),(N'Данилов'),(N'Емельянова'),(N'Злобин'),(N'Кадочникова'),
(N'Лебедь'),(N'Михайлова'),(N'Носов'),(N'Орехова'),(N'Пестов'),(N'Родионова'),(N'Сазонов'),(N'Авдеев'),
(N'Беспалова'),(N'Воронцов'),(N'Галкина'),(N'Дружинин'),(N'Ермакова'),(N'Зуев'),(N'Ильина'),(N'Кондратьев'),
(N'Ларина'),(N'Мартынов'),(N'Некрасова'),(N'Окулов'),(N'Пахомова'),(N'Ржевский'),(N'Селезнева'),(N'Таланов'),
(N'Уварова'),(N'Фокин'),(N'Хохлова'),(N'Царев'),(N'Черкасова'),(N'Шестаков'),(N'Щукина'),(N'Эйлер')
GO

DECLARE @FirstNames TABLE (ID INT IDENTITY(1,1), FirstName NVARCHAR(50))
INSERT INTO @FirstNames (FirstName) VALUES 
(N'Иван'),(N'Петр'),(N'Алексей'),(N'Максим'),(N'Николай'),(N'Светлана'),(N'Андрей'),(N'Марина'),(N'Владимир'),
(N'Денис'),(N'Татьяна'),(N'Степан'),(N'Артем'),(N'Екатерина'),(N'Игорь'),(N'Кирилл'),(N'Алина'),(N'Михаил'),
(N'Виктор'),(N'Юлия'),(N'Павел'),(N'Наталья'),(N'Константин'),(N'Антон'),(N'Богдан'),(N'Вера'),(N'Полина'),
(N'Глеб'),(N'Евгений'),(N'София'),(N'Тимофей'),(N'Яна'),(N'Марк'),(N'Алиса'),(N'Даниил'),(N'Валерия'),
(N'Лев'),(N'Ангелина'),(N'Георгий'),(N'Елизавета'),(N'Илья'),(N'Арсений'),(N'Матвей'),(N'Варвара'),(N'Платон'),
(N'Тихон'),(N'Александра'),(N'Захар'),(N'Ева'),(N'Ярослав'),(N'Милана'),(N'Савелий'),(N'Таисия'),(N'Артемий'),
(N'Злата'),(N'Герман'),(N'Любовь'),(N'Прохор'),(N'Агата'),(N'Демьян'),(N'Лука'),(N'Надежда'),(N'Станислав'),
(N'Регина'),(N'Тарас'),(N'Сабина'),(N'Федор'),(N'Ярослава'),(N'Эдуард'),(N'Юлиана'),(N'Шамиль'),(N'Эмилия'),
(N'Юрий'),(N'Чулпан'),(N'Эльдар'),(N'Бронислав'),(N'Виолетта'),(N'Гавриил'),(N'Дарина'),(N'Ефим'),(N'Жанна'),
(N'Зиновий'),(N'Изольда'),(N'Карл'),(N'Лада'),(N'Мирон'),(N'Нина'),(N'Остап'),(N'Пелагея'),(N'Ратибор'),
(N'Серафима'),(N'Тарас'),(N'Ульяна'),(N'Филипп'),(N'Христина'),(N'Цезарь'),(N'Чеслава'),(N'Шарль'),(N'Эвелина'),
(N'Юлиан')
GO

DECLARE @Patronymics TABLE (ID INT IDENTITY(1,1), Patronymic NVARCHAR(50))
INSERT INTO @Patronymics (Patronymic) VALUES 
(N'Сергеевич'),(N'Алексеевич'),(N'Дмитриевич'),(N'Павлович'),(N'Викторович'),(N'Игоревна'),(N'Семенович'),
(N'Александровна'),(N'Олегович'),(N'Романович'),(N'Максимовна'),(N'Григорьевич'),(N'Борисович'),(N'Владимировна'),
(N'Анатольевич'),(N'Денисович'),(N'Владимировна'),(N'Евгеньевич'),(N'Петрович'),(N'Андреевна'),(N'Сергеевна'),
(N'Игоревич'),(N'Валерьевич'),(N'Михайлович'),(N'Николаевна'),(N'Олеговна'),(N'Александрович'),(N'Романовна'),
(N'Степанович'),(N'Григорьевна'),(N'Алексеевна'),(N'Дмитриевна'),(N'Павловна'),(N'Викторовна'),(N'Игоревна'),
(N'Семеновна'),(N'Александровна'),(N'Олеговна'),(N'Романовна'),(N'Максимовна'),(N'Григорьевна'),(N'Борисовна'),
(N'Владимировна'),(N'Анатольевна'),(N'Денисовна'),(N'Евгеньевна'),(N'Петровна'),(N'Андреевна'),(N'Сергеевна')
GO

-- Определяем размеры групп (сумма = 117)
DECLARE @GroupSizes TABLE (GroupID INT, Size INT)
INSERT INTO @GroupSizes VALUES
(1, 20),  -- ИС-201
(2, 18),  -- ПР-202
(3, 22),  -- БД-201
(4, 15),  -- СЕТ-201
(5, 25),  -- МАТ-201
(6, 17)   -- ВЕБ-201

DECLARE @StudentID INT
DECLARE @GroupID INT
DECLARE @GroupSize INT
DECLARE @TeacherID INT
DECLARE @SurnameID INT, @FirstNameID INT, @PatronymicID INT
DECLARE @Surname NVARCHAR(50), @FirstName NVARCHAR(50), @Patronymic NVARCHAR(50)
DECLARE @Login NVARCHAR(50), @Password NVARCHAR(255), @Phone NVARCHAR(15), @Email NVARCHAR(100)
DECLARE @Counter INT

-- Временная таблица для хранения ID студентов (понадобится позже)
CREATE TABLE #StudentIDs (ID INT)

DECLARE group_cursor CURSOR FOR
SELECT GroupID, Size FROM @GroupSizes

OPEN group_cursor
FETCH NEXT FROM group_cursor INTO @GroupID, @GroupSize

WHILE @@FETCH_STATUS = 0
BEGIN
    SET @Counter = 1
    -- Получаем ID классного руководителя для этой группы
    SELECT @TeacherID = ID_Классный_руководитель FROM [dbo].[Классный_руководитель] WHERE ID_Группа = @GroupID
    
    WHILE @Counter <= @GroupSize
    BEGIN
        -- Случайные ID из списков
        SET @SurnameID = 1 + ABS(CHECKSUM(NEWID())) % (SELECT COUNT(*) FROM @Surnames)
        SET @FirstNameID = 1 + ABS(CHECKSUM(NEWID())) % (SELECT COUNT(*) FROM @FirstNames)
        SET @PatronymicID = 1 + ABS(CHECKSUM(NEWID())) % (SELECT COUNT(*) FROM @Patronymics)
        
        SELECT @Surname = Surname FROM @Surnames WHERE ID = @SurnameID
        SELECT @FirstName = FirstName FROM @FirstNames WHERE ID = @FirstNameID
        SELECT @Patronymic = Patronymic FROM @Patronymics WHERE ID = @PatronymicID
        
        -- Логин: фамилия_инициалы (латиница, упрощённо)
        SET @Login = LOWER(REPLACE(@Surname, N'ё', N'e')) + '_' + LOWER(LEFT(@FirstName,1)) + LOWER(LEFT(@Patronymic,1))
        SET @Password = 'student_pass_' + RIGHT('000' + CAST(@Counter AS VARCHAR(3)), 3)
        SET @Phone = '8915' + RIGHT('0000000' + CAST(ABS(CHECKSUM(NEWID())) % 10000000 AS VARCHAR(7)), 7)
        SET @Email = @Login + '@student.ru'
        
        INSERT INTO [dbo].[Студент] 
            ([Фамилия], [Имя], [Отчество], [Логин], [Пароль], [Номер_телефона], [Электронная_почта], 
             [ID_Группа], [ID_Классный_руководитель], [Дата_зачисления])
        VALUES 
            (@Surname, @FirstName, @Patronymic, @Login, @Password, @Phone, @Email, 
             @GroupID, @TeacherID, '2024-09-01')
        
        SET @Counter = @Counter + 1
    END
    FETCH NEXT FROM group_cursor INTO @GroupID, @GroupSize
END

CLOSE group_cursor
DEALLOCATE group_cursor

-- Сохраняем ID всех студентов
INSERT INTO #StudentIDs (ID) SELECT ID_Студент FROM [dbo].[Студент]
GO

-- =====================================================
-- 3. Создание командных достижений
-- =====================================================

-- Случайные значения для командных достижений
DECLARE @AchievementID INT
DECLARE @TeamSport NVARCHAR(100)
DECLARE @TeamEvent NVARCHAR(200)
DECLARE @TeamLevel NVARCHAR(50)
DECLARE @TeamSize INT
DECLARE @TeamPlace INT
DECLARE @TeamVenue NVARCHAR(200)
DECLARE @TeamDate DATE

DECLARE @TeamCount INT = 0
WHILE @TeamCount < 12  -- создадим 12 командных достижений
BEGIN
    -- Генерируем данные
    SET @TeamEvent = 
        CASE ABS(CHECKSUM(NEWID())) % 10
            WHEN 0 THEN N'Чемпионат университета'
            WHEN 1 THEN N'Спартакиада студентов'
            WHEN 2 THEN N'Кубок ректора'
            WHEN 3 THEN N'Универсиада'
            WHEN 4 THEN N'Студенческая лига'
            WHEN 5 THEN N'Первенство факультета'
            WHEN 6 THEN N'Турнир памяти'
            WHEN 7 THEN N'Открытый чемпионат'
            WHEN 8 THEN N'Кубок города'
            ELSE N'Всероссийские соревнования'
        END
    SET @TeamSport =
        CASE ABS(CHECKSUM(NEWID())) % 8
            WHEN 0 THEN N'Футбол'
            WHEN 1 THEN N'Баскетбол'
            WHEN 2 THEN N'Волейбол'
            WHEN 3 THEN N'Хоккей'
            WHEN 4 THEN N'Гандбол'
            WHEN 5 THEN N'Регби'
            WHEN 6 THEN N'Мини-футбол'
            ELSE N'Пляжный волейбол'
        END
    SET @TeamLevel =
        CASE ABS(CHECKSUM(NEWID())) % 4
            WHEN 0 THEN N'Университетский'
            WHEN 1 THEN N'Городской'
            WHEN 2 THEN N'Региональный'
            ELSE N'Всероссийский'
        END
    SET @TeamSize = 2 + ABS(CHECKSUM(NEWID())) % 9  -- от 2 до 10
    SET @TeamPlace = 1 + ABS(CHECKSUM(NEWID())) % 3
    SET @TeamVenue =
        CASE ABS(CHECKSUM(NEWID())) % 5
            WHEN 0 THEN N'Спортивный комплекс'
            WHEN 1 THEN N'Дворец спорта'
            WHEN 2 THEN N'Стадион'
            WHEN 3 THEN N'Спортивный центр'
            ELSE N'Универсальный зал'
        END
    SET @TeamDate = DATEADD(day, -ABS(CHECKSUM(NEWID())) % 1095, GETDATE())
    
    -- Вставляем командное достижение
    INSERT INTO [dbo].[Достижение] 
        ([Название мероприятия], [Название_вида_спорта], [Уровень_соревнования], 
         [Командная_игра], [Численность команды])
    VALUES 
        (@TeamEvent, @TeamSport, @TeamLevel, 1, @TeamSize)
    
    SET @AchievementID = SCOPE_IDENTITY()
    
    -- Выбираем случайных студентов для участия (из всех, без повторений)
    -- Для простоты выбираем @TeamSize студентов из всех, включая возможные повторы? Лучше без повторов.
    DECLARE @TeamMembers TABLE (StudentID INT)
    INSERT INTO @TeamMembers (StudentID)
    SELECT TOP (@TeamSize) ID FROM #StudentIDs ORDER BY NEWID()
    
    -- Добавляем связи для каждого участника
    INSERT INTO [dbo].[Студент_Достижение] 
        ([ID_Студент], [ID_Достижение], [Занятое_место], [Место_проведения], [Дата_проведения], [Дата_выдачи])
    SELECT 
        StudentID, @AchievementID, @TeamPlace, @TeamVenue, @TeamDate, GETDATE()
    FROM @TeamMembers
    
    SET @TeamCount = @TeamCount + 1
END
GO

-- =====================================================
-- 4. Создание индивидуальных достижений для каждого студента
-- =====================================================

-- Определяем, сколько достижений нужно каждому студенту (1-5)
CREATE TABLE #StudentNeeds (StudentID INT, Need INT, AchievementsAdded INT DEFAULT 0)
INSERT INTO #StudentNeeds (StudentID, Need)
SELECT ID, 1 + ABS(CHECKSUM(NEWID())) % 5 FROM #StudentIDs

-- Для каждого студента создаём недостающие индивидуальные достижения
DECLARE @CurrentStudentID INT
DECLARE @CurrentNeed INT
DECLARE @AchievementsSoFar INT
DECLARE @Remaining INT

-- Сначала узнаем, сколько командных достижений уже есть у каждого студента
SELECT StudentID, COUNT(*) AS TeamCount
INTO #StudentTeamCounts
FROM [dbo].[Студент_Достижение] sd
INNER JOIN [dbo].[Достижение] d ON sd.ID_Достижение = d.ID_достижение
WHERE d.Командная_игра = 1
GROUP BY StudentID

-- Обновляем #StudentNeeds: вычитаем количество уже имеющихся командных достижений
UPDATE sn
SET sn.Need = sn.Need - ISNULL(stc.TeamCount, 0)
FROM #StudentNeeds sn
LEFT JOIN #StudentTeamCounts stc ON sn.StudentID = stc.StudentID

-- Убедимся, что Need не стал меньше 1
UPDATE #StudentNeeds SET Need = 1 WHERE Need < 1

-- Теперь для каждого студента создаём Need индивидуальных достижений
DECLARE student_cursor CURSOR FOR
SELECT StudentID, Need FROM #StudentNeeds

OPEN student_cursor
FETCH NEXT FROM student_cursor INTO @CurrentStudentID, @CurrentNeed

WHILE @@FETCH_STATUS = 0
BEGIN
    SET @AchievementsSoFar = 0
    WHILE @AchievementsSoFar < @CurrentNeed
    BEGIN
        -- Генерируем данные для индивидуального достижения
        DECLARE @IndEvent NVARCHAR(200) =
            CASE ABS(CHECKSUM(NEWID())) % 10
                WHEN 0 THEN N'Чемпионат университета'
                WHEN 1 THEN N'Спартакиада студентов'
                WHEN 2 THEN N'Кубок ректора'
                WHEN 3 THEN N'Универсиада'
                WHEN 4 THEN N'Студенческая лига'
                WHEN 5 THEN N'Первенство факультета'
                WHEN 6 THEN N'Турнир памяти'
                WHEN 7 THEN N'Открытый чемпионат'
                WHEN 8 THEN N'Кубок города'
                ELSE N'Всероссийские соревнования'
            END
        DECLARE @IndSport NVARCHAR(100) =
            CASE ABS(CHECKSUM(NEWID())) % 15
                WHEN 0 THEN N'Легкая атлетика'
                WHEN 1 THEN N'Плавание'
                WHEN 2 THEN N'Теннис'
                WHEN 3 THEN N'Шахматы'
                WHEN 4 THEN N'Настольный теннис'
                WHEN 5 THEN N'Бег'
                WHEN 6 THEN N'Дзюдо'
                WHEN 7 THEN N'Самбо'
                WHEN 8 THEN N'Лыжные гонки'
                WHEN 9 THEN N'Бокс'
                WHEN 10 THEN N'Армрестлинг'
                WHEN 11 THEN N'Гиревой спорт'
                WHEN 12 THEN N'Пауэрлифтинг'
                WHEN 13 THEN N'Скалолазание'
                ELSE N'Спортивная гимнастика'
            END
        DECLARE @IndLevel NVARCHAR(50) =
            CASE ABS(CHECKSUM(NEWID())) % 4
                WHEN 0 THEN N'Университетский'
                WHEN 1 THEN N'Городской'
                WHEN 2 THEN N'Региональный'
                ELSE N'Всероссийский'
            END
        DECLARE @IndPlace INT = 1 + ABS(CHECKSUM(NEWID())) % 3
        DECLARE @IndVenue NVARCHAR(200) =
            CASE ABS(CHECKSUM(NEWID())) % 5
                WHEN 0 THEN N'Спортивный комплекс'
                WHEN 1 THEN N'Дворец спорта'
                WHEN 2 THEN N'Стадион'
                WHEN 3 THEN N'Спортивный центр'
                ELSE N'Универсальный зал'
            END
        DECLARE @IndDate DATE = DATEADD(day, -ABS(CHECKSUM(NEWID())) % 1095, GETDATE())
        
        -- Вставляем индивидуальное достижение
        INSERT INTO [dbo].[Достижение] 
            ([Название мероприятия], [Название_вида_спорта], [Уровень_соревнования], 
             [Командная_игра], [Численность команды])
        VALUES 
            (@IndEvent, @IndSport, @IndLevel, 0, NULL)
        
        SET @AchievementID = SCOPE_IDENTITY()
        
        -- Добавляем связь для этого студента
        INSERT INTO [dbo].[Студент_Достижение] 
            ([ID_Студент], [ID_Достижение], [Занятое_место], [Место_проведения], [Дата_проведения], [Дата_выдачи])
        VALUES 
            (@CurrentStudentID, @AchievementID, @IndPlace, @IndVenue, @IndDate, GETDATE())
        
        SET @AchievementsSoFar = @AchievementsSoFar + 1
    END
    FETCH NEXT FROM student_cursor INTO @CurrentStudentID, @CurrentNeed
END

CLOSE student_cursor
DEALLOCATE student_cursor

-- =====================================================
-- 5. Очистка временных объектов
-- =====================================================
DROP TABLE #StudentIDs
DROP TABLE #StudentNeeds
DROP TABLE #StudentTeamCounts
GO

-- =====================================================
-- 6. Проверочные запросы (для информации)
-- =====================================================
SELECT 'Количество студентов: ' + CAST(COUNT(*) AS VARCHAR) FROM [dbo].[Студент]
SELECT 'Количество достижений: ' + CAST(COUNT(*) AS VARCHAR) FROM [dbo].[Достижение]
SELECT 'Количество связей студент-достижение: ' + CAST(COUNT(*) AS VARCHAR) FROM [dbo].[Студент_Достижение]
SELECT 'Среднее количество достижений на студента: ' + 
    CAST(CAST(COUNT(*) AS FLOAT) / (SELECT COUNT(*) FROM [dbo].[Студент]) AS VARCHAR) 
FROM [dbo].[Студент_Достижение]

PRINT '====================================================='
PRINT 'Заполнение базы данных успешно завершено!'
PRINT '====================================================='
PRINT 'Данные для входа:'
PRINT 'Администраторы:'
PRINT '  Логин: admin_ivanov, Пароль: admin_pass_123'
PRINT '  Логин: admin_petrova, Пароль: secure_admin_456'
PRINT ''
PRINT 'Классные руководители:'
PRINT '  Логин: smirnov_dv, Пароль: teacher2024_1 (ИС-201)'
PRINT '  Логин: kuznetsova_ea, Пароль: teacher2024_2 (ПР-202)'
PRINT '  Логин: popov_sn, Пароль: teacher2024_3 (БД-201)'
PRINT '  Логин: vasilieva_am, Пароль: teacher2024_4 (СЕТ-201)'
PRINT '  Логин: sokolov_ai, Пароль: teacher2024_5 (МАТ-201)'
PRINT '  Логин: novikova_od, Пароль: teacher2024_6 (ВЕБ-201)'
PRINT ''
PRINT 'Студенты (пример):'
SELECT TOP 5 Логин, Пароль FROM [dbo].[Студент] ORDER BY NEWID()
PRINT '====================================================='
GO