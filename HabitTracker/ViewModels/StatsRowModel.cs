using HabitTracker.Core.Models;
using HabitTracker.Core.Rules;
using HabitTracker.Core.Texts;
using HabitTracker.Texts;

namespace HabitTracker.ViewModels;

public sealed class StatsRowModel
{
    public StatsRowModel(HabitProgressRow row)
    {
        Id = row.Id;
        Name = row.Name;
        Glyph = row.Glyph;
        ColorHex = row.ColorHex;
        Progress = row.Progress;
        ProgressText = HabitTexts.ProgressText(row.EffectiveStreak, row.GoalDays);
        PercentText = Messages.PercentText(HabitRules.GetProgressPercent(row.Progress));
        IsGoalReached = row.IsGoalReached;
    }

    public int Id { get; }
    public string Name { get; }
    public string Glyph { get; }
    public string ColorHex { get; }
    public double Progress { get; }
    public string ProgressText { get; }
    public string PercentText { get; }
    public bool IsGoalReached { get; }
}