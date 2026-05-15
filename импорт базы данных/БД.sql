-- =====================================================
-- Скрипт создания базы данных SportAchievementDB
-- для SQL Server Express (все версии)
-- Без лишних настроек, совместим с Express
-- =====================================================



USE [SportAchievementDB]
GO

-- =====================================================
-- 1. Таблицы
-- =====================================================

-- Курс
CREATE TABLE [dbo].[Курс](
    [ID_Курс] [int] IDENTITY(1,1) NOT NULL,
    [Номер_курса] [int] NOT NULL,
    CONSTRAINT [PK_Курс] PRIMARY KEY CLUSTERED ([ID_Курс] ASC)
)
GO

CREATE UNIQUE NONCLUSTERED INDEX [UQ_Номер_курса] ON [dbo].[Курс] ([Номер_курса])
GO

-- Факультет
CREATE TABLE [dbo].[Факультет](
    [ID_Факультет] [int] IDENTITY(1,1) NOT NULL,
    [Название_факультета] [nvarchar](100) NOT NULL,
    CONSTRAINT [PK_Факультет] PRIMARY KEY CLUSTERED ([ID_Факультет] ASC)
)
GO

CREATE UNIQUE NONCLUSTERED INDEX [UQ_Факультет_Название] ON [dbo].[Факультет] ([Название_факультета])
GO

-- Специальность
CREATE TABLE [dbo].[Специальность](
    [ID_Специальность] [int] IDENTITY(1,1) NOT NULL,
    [Код_специальности] [nvarchar](20) NOT NULL,
    [Название_специальности] [nvarchar](100) NOT NULL,
    [ID_Факультет] [int] NOT NULL,
    CONSTRAINT [PK_Специальность] PRIMARY KEY CLUSTERED ([ID_Специальность] ASC)
)
GO

CREATE UNIQUE NONCLUSTERED INDEX [UQ_Специальность_Код] ON [dbo].[Специальность] ([Код_специальности])
GO
CREATE NONCLUSTERED INDEX [IX_Специальность_Факультет] ON [dbo].[Специальность] ([ID_Факультет])
GO

-- Кафедра
CREATE TABLE [dbo].[Кафедра](
    [ID_Кафедра] [int] IDENTITY(1,1) NOT NULL,
    [Название_кафедры] [nvarchar](100) NOT NULL,
    [ID_Факультет] [int] NOT NULL,
    CONSTRAINT [PK_Кафедра] PRIMARY KEY CLUSTERED ([ID_Кафедра] ASC)
)
GO

CREATE UNIQUE NONCLUSTERED INDEX [UQ_Кафедра_Название] ON [dbo].[Кафедра] ([Название_кафедры])
GO
CREATE NONCLUSTERED INDEX [IX_Кафедра_Факультет] ON [dbo].[Кафедра] ([ID_Факультет])
GO

-- Группа
CREATE TABLE [dbo].[Группа](
    [ID_Группа] [int] IDENTITY(1,1) NOT NULL,
    [Название] [nvarchar](20) NOT NULL,
    [Год_формирования] [int] NOT NULL,
    [ID_Специальность] [int] NOT NULL,
    [ID_Курс] [int] NOT NULL,
    CONSTRAINT [PK_Группа] PRIMARY KEY CLUSTERED ([ID_Группа] ASC)
)
GO

CREATE UNIQUE NONCLUSTERED INDEX [UQ_Группа_Название] ON [dbo].[Группа] ([Название])
GO
CREATE NONCLUSTERED INDEX [IX_Группа_Специальность] ON [dbo].[Группа] ([ID_Специальность])
GO
CREATE NONCLUSTERED INDEX [IX_Группа_Курс] ON [dbo].[Группа] ([ID_Курс])
GO

-- Классный руководитель
CREATE TABLE [dbo].[Классный_руководитель](
    [ID_Классный_руководитель] [int] IDENTITY(1,1) NOT NULL,
    [Фамилия] [nvarchar](50) NOT NULL,
    [Имя] [nvarchar](50) NOT NULL,
    [Отчество] [nvarchar](50) NULL,
    [Логин] [nvarchar](50) NOT NULL,
    [Пароль] [nvarchar](255) NOT NULL,
    [Номер_телефона] [nvarchar](15) NOT NULL,
    [Электронная_почта] [nvarchar](100) NOT NULL,
    [ID_Группа] [int] NOT NULL,
    [ID_Кафедра] [int] NOT NULL,
    CONSTRAINT [PK_Классный_руководитель] PRIMARY KEY CLUSTERED ([ID_Классный_руководитель] ASC)
)
GO

CREATE UNIQUE NONCLUSTERED INDEX [UQ_Классный_руководитель_Логин] ON [dbo].[Классный_руководитель] ([Логин])
GO
CREATE UNIQUE NONCLUSTERED INDEX [UQ_Классный_руководитель_Email] ON [dbo].[Классный_руководитель] ([Электронная_почта])
GO
CREATE NONCLUSTERED INDEX [IX_Классный_руководитель_Группа] ON [dbo].[Классный_руководитель] ([ID_Группа])
GO
CREATE NONCLUSTERED INDEX [IX_Классный_руководитель_Кафедра] ON [dbo].[Классный_руководитель] ([ID_Кафедра])
GO

-- Администратор
CREATE TABLE [dbo].[Администратор](
    [ID_Администратор] [int] IDENTITY(1,1) NOT NULL,
    [Фамилия] [nvarchar](50) NOT NULL,
    [Имя] [nvarchar](50) NOT NULL,
    [Отчество] [nvarchar](50) NULL,
    [Логин] [nvarchar](50) NOT NULL,
    [Пароль] [nvarchar](255) NOT NULL,
    [Номер_телефона] [nvarchar](15) NOT NULL,
    [Электронная_почта] [nvarchar](100) NOT NULL,
    CONSTRAINT [PK_Администратор] PRIMARY KEY CLUSTERED ([ID_Администратор] ASC)
)
GO

CREATE UNIQUE NONCLUSTERED INDEX [UQ_Администратор_Логин] ON [dbo].[Администратор] ([Логин])
GO
CREATE UNIQUE NONCLUSTERED INDEX [UQ_Администратор_Email] ON [dbo].[Администратор] ([Электронная_почта])
GO

-- Студент
CREATE TABLE [dbo].[Студент](
    [ID_Студент] [int] IDENTITY(1,1) NOT NULL,
    [Фамилия] [nvarchar](50) NOT NULL,
    [Имя] [nvarchar](50) NOT NULL,
    [Отчество] [nvarchar](50) NULL,
    [Логин] [nvarchar](50) NOT NULL,
    [Пароль] [nvarchar](255) NOT NULL,
    [Номер_телефона] [nvarchar](15) NOT NULL,
    [Электронная_почта] [nvarchar](100) NOT NULL,
    [ID_Группа] [int] NOT NULL,
    [ID_Классный_руководитель] [int] NOT NULL,
    [Дата_зачисления] [date] NOT NULL,
    CONSTRAINT [PK_Студент] PRIMARY KEY CLUSTERED ([ID_Студент] ASC)
)
GO

CREATE UNIQUE NONCLUSTERED INDEX [UQ_Студент_Логин] ON [dbo].[Студент] ([Логин])
GO
CREATE UNIQUE NONCLUSTERED INDEX [UQ_Студент_Email] ON [dbo].[Студент] ([Электронная_почта])
GO
CREATE NONCLUSTERED INDEX [IX_Студент_Группа] ON [dbo].[Студент] ([ID_Группа])
GO
CREATE NONCLUSTERED INDEX [IX_Студент_Классный_руководитель] ON [dbo].[Студент] ([ID_Классный_руководитель])
GO
CREATE NONCLUSTERED INDEX [IX_Студент_ФИО] ON [dbo].[Студент] ([Фамилия], [Имя], [Отчество])
GO

-- Достижение
CREATE TABLE [dbo].[Достижение](
    [ID_достижение] [int] IDENTITY(1,1) NOT NULL,
    [Название мероприятия] [nvarchar](200) NOT NULL,
    [Название_вида_спорта] [nvarchar](100) NOT NULL,
    [Уровень_соревнования] [nvarchar](50) NOT NULL,
    [Командная_игра] [bit] NOT NULL DEFAULT 0,
    [Численность команды] [int] NULL,
    CONSTRAINT [PK_Достижение] PRIMARY KEY CLUSTERED ([ID_достижение] ASC)
)
GO

-- Связующая таблица Студент_Достижение
CREATE TABLE [dbo].[Студент_Достижение](
    [ID_Студент_Достижение] [int] IDENTITY(1,1) NOT NULL,
    [ID_Студент] [int] NOT NULL,
    [ID_Достижение] [int] NOT NULL,
    [Занятое_место] [int] NOT NULL,
    [Место_проведения] [nvarchar](200) NOT NULL,
    [Дата_проведения] [date] NOT NULL,
    [Дата_выдачи] [date] NOT NULL,
    CONSTRAINT [PK_Студент_Достижение] PRIMARY KEY CLUSTERED ([ID_Студент_Достижение] ASC)
)
GO

CREATE NONCLUSTERED INDEX [IX_Студент_Достижение_Студент] ON [dbo].[Студент_Достижение] ([ID_Студент])
GO
CREATE NONCLUSTERED INDEX [IX_Студент_Достижение_Достижение] ON [dbo].[Студент_Достижение] ([ID_Достижение])
GO

-- Журнал аудита
CREATE TABLE [dbo].[Журнал_аудита](
    [ID_Журнал] [int] IDENTITY(1,1) NOT NULL,
    [Тип_операции] [nvarchar](50) NOT NULL,
    [Имя_таблицы] [nvarchar](100) NOT NULL,
    [ID_записи] [int] NOT NULL,
    [Пользователь] [nvarchar](200) NOT NULL,
    [Роль] [nvarchar](50) NOT NULL,
    [Детали] [nvarchar](max) NULL,
    [Дата_операции] [datetime] NOT NULL,
    [ID_Пользователя] [int] NULL,
    CONSTRAINT [PK_Журнал_аудита] PRIMARY KEY CLUSTERED ([ID_Журнал] ASC)
)
GO

-- =====================================================
-- 2. Внешние ключи
-- =====================================================
ALTER TABLE [dbo].[Специальность] ADD CONSTRAINT [FK_Специальность_Факультет] FOREIGN KEY([ID_Факультет]) REFERENCES [dbo].[Факультет] ([ID_Факультет])
GO
ALTER TABLE [dbo].[Кафедра] ADD CONSTRAINT [FK_Кафедра_Факультет] FOREIGN KEY([ID_Факультет]) REFERENCES [dbo].[Факультет] ([ID_Факультет])
GO
ALTER TABLE [dbo].[Группа] ADD CONSTRAINT [FK_Группа_Специальность] FOREIGN KEY([ID_Специальность]) REFERENCES [dbo].[Специальность] ([ID_Специальность])
GO
ALTER TABLE [dbo].[Группа] ADD CONSTRAINT [FK_Группа_Курс] FOREIGN KEY([ID_Курс]) REFERENCES [dbo].[Курс] ([ID_Курс])
GO
ALTER TABLE [dbo].[Классный_руководитель] ADD CONSTRAINT [FK_Классный_руководитель_Группа] FOREIGN KEY([ID_Группа]) REFERENCES [dbo].[Группа] ([ID_Группа])
GO
ALTER TABLE [dbo].[Классный_руководитель] ADD CONSTRAINT [FK_Классный_руководитель_Кафедра] FOREIGN KEY([ID_Кафедра]) REFERENCES [dbo].[Кафедра] ([ID_Кафедра])
GO
ALTER TABLE [dbo].[Студент] ADD CONSTRAINT [FK_Студент_Группа] FOREIGN KEY([ID_Группа]) REFERENCES [dbo].[Группа] ([ID_Группа])
GO
ALTER TABLE [dbo].[Студент] ADD CONSTRAINT [FK_Студент_Классный_руководитель] FOREIGN KEY([ID_Классный_руководитель]) REFERENCES [dbo].[Классный_руководитель] ([ID_Классный_руководитель])
GO
ALTER TABLE [dbo].[Студент_Достижение] ADD CONSTRAINT [FK_Студент_Достижение_Студент] FOREIGN KEY([ID_Студент]) REFERENCES [dbo].[Студент] ([ID_Студент])
GO
ALTER TABLE [dbo].[Студент_Достижение] ADD CONSTRAINT [FK_Студент_Достижение_Достижение] FOREIGN KEY([ID_Достижение]) REFERENCES [dbo].[Достижение] ([ID_достижение])
GO
ALTER TABLE [dbo].[Журнал_аудита] ADD CONSTRAINT [FK_Журнал_аудита_Администратор] FOREIGN KEY([ID_Пользователя]) REFERENCES [dbo].[Администратор] ([ID_Администратор])
GO
ALTER TABLE [dbo].[Журнал_аудита] ADD CONSTRAINT [FK_Журнал_аудита_Классный_руководитель] FOREIGN KEY([ID_Пользователя]) REFERENCES [dbo].[Классный_руководитель] ([ID_Классный_руководитель])
GO

-- =====================================================
-- 3. Функции
-- =====================================================
IF OBJECT_ID(N'[dbo].[fn_GetCurrentCourse]') IS NOT NULL
    DROP FUNCTION [dbo].[fn_GetCurrentCourse]
GO
CREATE FUNCTION [dbo].[fn_GetCurrentCourse](@Год_формирования INT)
RETURNS INT
AS
BEGIN
    DECLARE @Course INT = YEAR(GETDATE()) - @Год_формирования
    IF @Course < 1 SET @Course = 1
    IF @Course > 6 SET @Course = 6
    RETURN @Course
END
GO

IF OBJECT_ID(N'[dbo].[fn_GetStudentAchievements]') IS NOT NULL
    DROP FUNCTION [dbo].[fn_GetStudentAchievements]
GO
CREATE FUNCTION [dbo].[fn_GetStudentAchievements](@StudentID INT)
RETURNS TABLE
AS
RETURN
(
    SELECT 
        d.ID_достижение,
        d.[Название мероприятия],
        d.[Название_вида_спорта],
        d.[Уровень_соревнования],
        sd.Занятое_место,
        sd.Место_проведения,
        sd.Дата_проведения,
        d.Командная_игра,
        d.[Численность команды],
        CASE WHEN d.Командная_игра = 1 
            THEN (SELECT COUNT(*) FROM [dbo].[Студент_Достижение] WHERE ID_Достижение = d.ID_достижение)
            ELSE 1 
        END AS Количество_участников,
        (SELECT STUFF((
            SELECT ',' + CAST(s.ID_Студент AS NVARCHAR(10))
            FROM [dbo].[Студент_Достижение] s 
            WHERE s.ID_Достижение = d.ID_достижение
            ORDER BY s.ID_Студент
            FOR XML PATH('')), 1, 1, '') AS Участники_команды
    FROM [dbo].[Студент_Достижение] sd
    INNER JOIN [dbo].[Достижение] d ON sd.ID_Достижение = d.ID_достижение
    WHERE sd.ID_Студент = @StudentID
)
GO

-- =====================================================
-- 4. Представления
-- =====================================================
IF OBJECT_ID(N'[dbo].[vw_Информация_о_группах]') IS NOT NULL
    DROP VIEW [dbo].[vw_Информация_о_группах]
GO
CREATE VIEW [dbo].[vw_Информация_о_группах] AS
SELECT 
    g.ID_Группа,
    g.Название AS Группа,
    s.Код_специальности,
    s.Название_специальности AS Специальность,
    k.Номер_курса AS Курс,
    g.Год_формирования,
    f.Название_факультета AS Факультет,
    YEAR(GETDATE()) - g.Год_формирования AS Текущий_курс
FROM [dbo].[Группа] g
INNER JOIN [dbo].[Специальность] s ON g.ID_Специальность = s.ID_Специальность
INNER JOIN [dbo].[Курс] k ON g.ID_Курс = k.ID_Курс
INNER JOIN [dbo].[Факультет] f ON s.ID_Факультет = f.ID_Факультет
GO

IF OBJECT_ID(N'[dbo].[vw_Информация_о_студентах]') IS NOT NULL
    DROP VIEW [dbo].[vw_Информация_о_студентах]
GO
CREATE VIEW [dbo].[vw_Информация_о_студентах] AS
SELECT 
    st.ID_Студент,
    st.Фамилия,
    st.Имя,
    st.Отчество,
    st.Логин,
    st.Номер_телефона,
    st.Электронная_почта,
    st.Дата_зачисления,
    g.Название AS Группа,
    sp.Название_специальности AS Специальность,
    k.Номер_курса AS Курс_обучения,
    YEAR(GETDATE()) - g.Год_формирования AS Текущий_курс,
    kr.Фамилия + ' ' + kr.Имя + ' ' + ISNULL(kr.Отчество, '') AS Классный_руководитель,
    kaf.Название_кафедры AS Кафедра,
    f.Название_факультета AS Факультет
FROM [dbo].[Студент] st
INNER JOIN [dbo].[Группа] g ON st.ID_Группа = g.ID_Группа
INNER JOIN [dbo].[Специальность] sp ON g.ID_Специальность = sp.ID_Специальность
INNER JOIN [dbo].[Курс] k ON g.ID_Курс = k.ID_Курс
INNER JOIN [dbo].[Классный_руководитель] kr ON st.ID_Классный_руководитель = kr.ID_Классный_руководитель
INNER JOIN [dbo].[Кафедра] kaf ON kr.ID_Кафедра = kaf.ID_Кафедра
INNER JOIN [dbo].[Факультет] f ON sp.ID_Факультет = f.ID_Факультет
GO

IF OBJECT_ID(N'[dbo].[vw_Информация_о_преподавателях]') IS NOT NULL
    DROP VIEW [dbo].[vw_Информация_о_преподавателях]
GO
CREATE VIEW [dbo].[vw_Информация_о_преподавателях] AS
SELECT 
    kr.ID_Классный_руководитель,
    kr.Фамилия,
    kr.Имя,
    kr.Отчество,
    kr.Логин,
    kr.Номер_телефона,
    kr.Электронная_почта,
    g.Название AS Закрепленная_группа,
    kaf.Название_кафедры AS Кафедра,
    f.Название_факультета AS Факультет,
    (SELECT COUNT(*) FROM [dbo].[Студент] WHERE ID_Классный_руководитель = kr.ID_Классный_руководитель) AS Количество_студентов
FROM [dbo].[Классный_руководитель] kr
INNER JOIN [dbo].[Группа] g ON kr.ID_Группа = g.ID_Группа
INNER JOIN [dbo].[Кафедра] kaf ON kr.ID_Кафедра = kaf.ID_Кафедра
INNER JOIN [dbo].[Факультет] f ON kaf.ID_Факультет = f.ID_Факультет
GO

IF OBJECT_ID(N'[dbo].[vw_Достижения_студентов]') IS NOT NULL
    DROP VIEW [dbo].[vw_Достижения_студентов]
GO
CREATE VIEW [dbo].[vw_Достижения_студентов] AS
SELECT 
    s.ID_Студент,
    s.Фамилия + ' ' + s.Имя + ' ' + ISNULL(s.Отчество, '') AS ФИО_Студента,
    g.Название AS Группа,
    sd.ID_Студент_Достижение,
    d.ID_достижение,
    d.[Название мероприятия],
    d.[Название_вида_спорта],
    d.[Уровень_соревнования],
    sd.Занятое_место,
    CASE sd.Занятое_место
        WHEN 1 THEN N'Золото'
        WHEN 2 THEN N'Серебро'
        WHEN 3 THEN N'Бронза'
        ELSE CAST(sd.Занятое_место AS NVARCHAR(10))
    END AS Медаль,
    sd.Место_проведения,
    sd.Дата_проведения,
    sd.Дата_выдачи,
    d.Командная_игра,
    CASE WHEN d.Командная_игра = 1 THEN N'Да' ELSE N'Нет' END AS Командная,
    d.[Численность команды]
FROM [dbo].[Студент] s
INNER JOIN [dbo].[Группа] g ON s.ID_Группа = g.ID_Группа
INNER JOIN [dbo].[Студент_Достижение] sd ON s.ID_Студент = sd.ID_Студент
INNER JOIN [dbo].[Достижение] d ON sd.ID_Достижение = d.ID_достижение
GO

PRINT 'Скрипт успешно выполнен. База данных SportAchievementDB создана для SQL Server Express.'
GO