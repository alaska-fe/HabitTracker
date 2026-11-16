using System.Globalization;
using HabitTracker.Core.Texts;

namespace HabitTracker.Texts;

public static class Messages
{
    private static readonly CultureInfo Russian = CultureInfo.GetCultureInfo("ru-RU");

    public static string HabitAdded(string name) => $"Привычка «{name}» добавлена";
    public static string HabitMarked(int streak) => $"Отлично! Серия: {streak} {HabitTexts.DaysText(streak)}";
    public const string AlreadyDoneToday = "Сегодня эта привычка уже отмечена";
    public static string GoalReached(string name, int goalDays) =>
        $"Поздравляем! Цель «{name}» — {goalDays} {HabitTexts.DaysText(goalDays)} подряд — достигнута!";
    public const string GoalReachedTitle = "Цель достигнута";
    public const string StreakBroken = "Серия прервана — начните снова сегодня";
    public static string ConfirmDelete(string name) =>
        $"Удалить привычку «{name}»? Серия и прогресс будут потеряны.";
    public const string ConfirmDeleteTitle = "Удаление привычки";
    public const string DeleteAccept = "Удалить";
    public const string DeleteCancel = "Отмена";
    public static string HabitDeleted(string name) => $"Привычка «{name}» удалена";
    public const string LoadFailed = "Не удалось загрузить привычки. Повторите попытку.";
    public const string SaveFailed = "Не удалось сохранить изменения. Повторите попытку.";
    public const string ClockMovedBack =
        "Дата на устройстве раньше последней отметки. Проверьте настройки даты и времени.";
    public const string DuplicateName = "Привычка с таким названием уже есть";
    public const string ConfirmDiscard = "Отменить ввод? Введённые данные будут потеряны.";
    public const string ConfirmDiscardTitle = "Несохранённые изменения";
    public const string DiscardAccept = "Отменить ввод";
    public const string DiscardCancel = "Продолжить";
    public const string EmptyHabits = "Привычек пока нет. Добавьте первую!";
    public static string LimitReached(int limit) =>
        $"Достигнут предел: {limit} привычек. Удалите ненужные, чтобы добавить новые.";
    public const string NotFound = "Привычка не найдена. Список обновлён.";

    public const string ErrorTitle = "Ошибка";
    public const string Retry = "Повторить";
    public const string Save = "Сохранить";
    public const string Cancel = "Отмена";
    public const string Loading = "Загрузка…";

    public const string HabitsTitle = "Привычки";
    public const string AddFirstHabit = "Добавить первую привычку";
    public const string MarkToday = "Отметить сегодня";
    public const string DoneToday = "Выполнено сегодня ✓";
    public static string TodaySummary(int done, int total) => $"Сегодня выполнено: {done} из {total}";

    public const string StatsTitle = "Статистика";
    public const string EmptyStats = "Нет данных для статистики. Добавьте первую привычку";
    public const string GoToHabits = "Добавить привычку";
    public const string TileTotal = "Привычек";
    public const string TileToday = "Сегодня";
    public const string TileGoals = "Цель достигнута";
    public const string AverageProgress = "Средний прогресс";
    public const string LongestCurrent = "Лучшая текущая серия";
    public const string LongestEver = "Рекорд за всё время";
    public const string AllHabits = "Все привычки";

    public static string TodayTile(int done, int total) => $"{done} из {total}";
    public static string PercentText(int percent) => $"{percent} %";
    public static string AveragePercentText(double percent) => $"{percent.ToString("0.0", Russian)} %";
    public static string ShortDays(int days) => $"{days} дн.";
    public static string AverageProgressLine(double percent) => $"{AverageProgress}: {AveragePercentText(percent)}";
    public static string LongestCurrentLine(int days) => $"{LongestCurrent}: {ShortDays(days)}";
    public static string LongestEverLine(int days) => $"{LongestEver}: {ShortDays(days)}";

    public const string AddHabitTitle = "Новая привычка";
    public const string NameLabel = "Название *";
    public const string DescriptionLabel = "Описание";
    public const string IconLabel = "Иконка";
    public const string ColorLabel = "Цвет";
    public const string GoalLabel = "Цель";
    public const string PreviewLabel = "Предпросмотр";
    public static string CharCounter(int current, int max) => $"Введено {current} из {max}";

    public const string DetailsTitle = "Детали привычки";
    public const string CurrentStreakLabel = "Текущая серия";
    public const string BestStreakLabel = "Лучшая серия";
    public const string CreatedAtLabel = "Дата создания";
    public const string ProgressLabel = "Прогресс";
}