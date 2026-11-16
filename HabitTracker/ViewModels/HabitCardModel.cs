using CommunityToolkit.Mvvm.ComponentModel;
using HabitTracker.Core.Catalogs;
using HabitTracker.Core.Models;
using HabitTracker.Core.Rules;
using HabitTracker.Core.Texts;
using HabitTracker.Texts;

namespace HabitTracker.ViewModels;

/// <summary>
/// Карточка привычки — вычисленное представление Habit на сегодняшнюю дату (ТЗ 5.5).
/// Обновляется через Update: PropertyChanged приходит только на изменившиеся свойства.
/// IsBusy — собственный флаг карточки (не общий), чтобы не блокировать соседние кнопки.
/// </summary>
public sealed class HabitCardModel : ObservableObject
{
    private int _id;
    private string _name = string.Empty;
    private string _glyph = string.Empty;
    private string _colorHex = string.Empty;
    private int _effectiveStreak;
    private int _goalDays;
    private double _progress;
    private string _progressText = string.Empty;
    private string _streakText = string.Empty;
    private bool _isDoneToday;
    private bool _isStreakBroken;
    private bool _isGoalReached;
    private string _buttonText = string.Empty;
    private bool _isBusy;

    public HabitCardModel(Habit habit, DateTime today)
    {
        Source = habit;
        Update(habit, today);
    }

    /// <summary>Исходная сущность — нужна для повторной сортировки через Core.</summary>
    public Habit Source { get; private set; }

    public int Id { get => _id; private set => SetProperty(ref _id, value); }
    public string Name { get => _name; private set => SetProperty(ref _name, value); }
    public string Glyph { get => _glyph; private set => SetProperty(ref _glyph, value); }
    public string ColorHex { get => _colorHex; private set => SetProperty(ref _colorHex, value); }
    public int EffectiveStreak { get => _effectiveStreak; private set => SetProperty(ref _effectiveStreak, value); }
    public int GoalDays { get => _goalDays; private set => SetProperty(ref _goalDays, value); }
    public double Progress { get => _progress; private set => SetProperty(ref _progress, value); }
    public string ProgressText { get => _progressText; private set => SetProperty(ref _progressText, value); }
    public string StreakText { get => _streakText; private set => SetProperty(ref _streakText, value); }
    public bool IsDoneToday { get => _isDoneToday; private set => SetProperty(ref _isDoneToday, value); }
    public bool IsStreakBroken { get => _isStreakBroken; private set => SetProperty(ref _isStreakBroken, value); }
    public bool IsGoalReached { get => _isGoalReached; private set => SetProperty(ref _isGoalReached, value); }
    public string ButtonText { get => _buttonText; private set => SetProperty(ref _buttonText, value); }

    public bool IsBusy { get => _isBusy; set => SetProperty(ref _isBusy, value); }

    /// <summary>Пересчитывает все вычисляемые поля по актуальной сущности и дате.</summary>
    public void Update(Habit habit, DateTime today)
    {
        Source = habit;
        Id = habit.Id;
        Name = habit.Name;
        Glyph = HabitIcons.GetGlyph(habit.IconKey);
        ColorHex = HabitColors.GetHex(habit.ColorKey);
        GoalDays = habit.GoalDays;

        EffectiveStreak = HabitRules.GetEffectiveStreak(habit, today);
        Progress = HabitRules.GetProgress(habit, today);
        IsDoneToday = HabitRules.IsDoneToday(habit, today);
        IsStreakBroken = HabitRules.IsStreakBroken(habit, today);
        IsGoalReached = HabitRules.IsGoalReached(habit, today);

        ProgressText = HabitTexts.ProgressText(EffectiveStreak, habit.GoalDays);
        StreakText = IsStreakBroken ? Messages.StreakBroken : HabitTexts.StreakText(EffectiveStreak);
        ButtonText = IsDoneToday ? Messages.DoneToday : Messages.MarkToday;
    }
}