using HabitTracker.Core.Catalogs;
using HabitTracker.Core.Models;
using Xunit;

namespace HabitTracker.Tests.Models;

public class HabitTests
{
    [Fact]
    public void NewEntity_HasSpecificationDefaults()
    {
        var habit = new Habit();

        Assert.Equal(0, habit.Id);
        Assert.Equal(string.Empty, habit.Name);
        Assert.Equal(string.Empty, habit.Description);
        Assert.Equal(HabitIcons.Default, habit.IconKey);
        Assert.Equal(HabitColors.Default, habit.ColorKey);
        Assert.Equal(21, habit.GoalDays);
        Assert.Equal(0, habit.CurrentStreak);
        Assert.Equal(0, habit.BestStreak);
        Assert.Null(habit.LastCompletedDate);
        
    }
}