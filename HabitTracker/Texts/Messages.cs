using HabitTracker.Core.Texts;

namespace HabitTracker.Texts;

/// <summary>
/// Единый каталог текстов приложения (ТЗ 9.2, 9.4): уведомления N01–N16 и общие надписи.
/// Тексты валидации полей — в Core (ValidationTexts), склонение — HabitTexts.DaysText.
/// Пользователю никогда не показывается ex.Message.
/// </summary>
public static class Messages
{
    // ── Каталог уведомлений (ТЗ 9.2) ─────────────────────────────

    // N01 — строка состояния
    public static string HabitAdded(string name) => $"Привычка «{name}» добавлена";

    // N02 — строка состояния
    public static string HabitMarked(int streak) => $"Отлично! Серия: {streak} {HabitTexts.DaysText(streak)}";

    // N03 — строка состояния
    public const string AlreadyDoneToday = "Сегодня эта привычка уже отмечена";

    // N04 — диалог-сообщение
    public static string GoalReached(string name, int goalDays) =>
        $"Поздравляем! Цель «{name}» — {goalDays} {HabitTexts.DaysText(goalDays)} подряд — достигнута!";
    public const string GoalReachedTitle = "Цель достигнута";

    // N05 — состояние карточки
    public const string StreakBroken = "Серия прервана — начните снова сегодня";

    // N06 — диалог подтверждения
    public static string ConfirmDelete(string name) =>
        $"Удалить привычку «{name}»? Серия и прогресс будут потеряны.";
    public const string ConfirmDeleteTitle = "Удаление привычки";
    public const string DeleteAccept = "Удалить";
    public const string DeleteCancel = "Отмена";

    // N07 — строка состояния
    public static string HabitDeleted(string name) => $"Привычка «{name}» удалена";

    // N08 — inline: тексты в Core.ValidationTexts (единый источник)

    // N09 — баннер
    public const string LoadFailed = "Не удалось загрузить привычки. Повторите попытку.";

    // N10 — баннер / диалог
    public const string SaveFailed = "Не удалось сохранить изменения. Повторите попытку.";

    // N11 — диалог
    public const string ClockMovedBack =
        "Дата на устройстве раньше последней отметки. Проверьте настройки даты и времени.";

    // N12 — inline под полем «Название»
    public const string DuplicateName = "Привычка с таким названием уже есть";

    // N13 — диалог подтверждения
    public const string ConfirmDiscard = "Отменить ввод? Введённые данные будут потеряны.";
    public const string ConfirmDiscardTitle = "Несохранённые изменения";
    public const string DiscardAccept = "Отменить ввод";
    public const string DiscardCancel = "Продолжить";

    // N14 — пустое состояние
    public const string EmptyHabits = "Привычек пока нет. Добавьте первую!";

    // N15 — диалог
    public static string LimitReached(int limit) =>
        $"Достигнут предел: {limit} привычек. Удалите ненужные, чтобы добавить новые.";

    // N16 — строка состояния
    public const string NotFound = "Привычка не найдена. Список обновлён.";

    // ── Общие ────────────────────────────────────────────────────
    public const string ErrorTitle = "Ошибка";
    public const string Ok = "OK";
    public const string Retry = "Повторить";
    public const string Save = "Сохранить";
    public const string Cancel = "Отмена";
    public const string Loading = "Загрузка…";

    // ── Экран «Привычки» ─────────────────────────────────────────
    public const string HabitsTitle = "Привычки";
    public const string AddFirstHabit = "Добавить первую привычку";
    public const string MarkToday = "Отметить сегодня";
    public const string DoneToday = "Выполнено сегодня ✓";
    public static string TodaySummary(int done, int total) => $"Сегодня выполнено: {done} из {total}";

    // ── Экран «Статистика» ───────────────────────────────────────
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

    // ── Экран «Новая привычка» ───────────────────────────────────
    public const string AddHabitTitle = "Новая привычка";
    public const string NameLabel = "Название *";
    public const string DescriptionLabel = "Описание";
    public const string IconLabel = "Иконка";
    public const string ColorLabel = "Цвет";
    public const string GoalLabel = "Цель";
    public const string PreviewLabel = "Предпросмотр";
    public static string CharCounter(int current, int max) => $"Введено {current} из {max}";

    // ── Экран «Детали» (вариант) ─────────────────────────────────
    public const string DetailsTitle = "Детали привычки";
    public const string CurrentStreakLabel = "Текущая серия";
    public const string BestStreakLabel = "Лучшая серия";
    public const string CreatedAtLabel = "Дата создания";
    public const string ProgressLabel = "Прогресс";
}