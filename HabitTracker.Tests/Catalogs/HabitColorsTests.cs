using HabitTracker.Core.Catalogs;
using Xunit;

namespace HabitTracker.Tests.Catalogs;

public class HabitColorsTests
{
    [Fact]
    public void Default_IsBlueKey()
    {
        Assert.Equal("blue", HabitColors.Default);
    }

    [Fact]
    public void DefaultKey_ExistsInCatalog()
    {
        Assert.Contains(HabitColors.All, option => option.Key == HabitColors.Default);
    }

    [Theory]
    [InlineData("blue", "#2563EB")]
    [InlineData("green", "#16A34A")]
    [InlineData("orange", "#EA580C")]
    [InlineData("red", "#DC2626")]
    [InlineData("purple", "#7C3AED")]
    [InlineData("teal", "#0D9488")]
    [InlineData("pink", "#DB2777")]
    [InlineData("gray", "#475569")]
    public void GetHex_KnownKey_ReturnsCatalogHex(string key, string expectedHex)
    {
        Assert.Equal(expectedHex, HabitColors.GetHex(key));
    }

    [Theory]
    [InlineData("unknown")]
    [InlineData("")]
    [InlineData("BLUE")]
    public void GetHex_UnknownKey_ReturnsDefaultHex(string key)
    {
        var defaultHex = HabitColors.GetHex(HabitColors.Default);

        Assert.Equal(defaultHex, HabitColors.GetHex(key));
    }

    [Fact]
    public void GetHex_NullKey_ReturnsDefaultHex()
    {
        var defaultHex = HabitColors.GetHex(HabitColors.Default);

        Assert.Equal(defaultHex, HabitColors.GetHex(null!));
    }

    [Fact]
    public void All_KeysAreUnique()
    {
        var keys = HabitColors.All.Select(option => option.Key).ToList();

        Assert.Equal(keys.Count, keys.Distinct().Count());
    }

    [Fact]
    public void All_HexValues_HaveRgbFormat()
    {
        Assert.All(HabitColors.All, option =>
        {
            Assert.Matches("^#[0-9A-F]{6}$", option.Hex);
            Assert.False(string.IsNullOrWhiteSpace(option.Title));
        });
    }
}