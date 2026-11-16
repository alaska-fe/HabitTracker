using HabitTracker.Core.Catalogs;
using Xunit;

namespace HabitTracker.Tests.Catalogs;

public class HabitIconsTests
{
    [Fact]
    public void Default_IsBookKey()
    {
        Assert.Equal("book", HabitIcons.Default);
    }

    [Fact]
    public void DefaultKey_ExistsInCatalog()
    {
        Assert.Contains(HabitIcons.All, option => option.Key == HabitIcons.Default);
    }

    [Theory]
    [InlineData("water", "💧")]
    [InlineData("run", "🏃")]
    [InlineData("book", "📚")]
    [InlineData("yoga", "🧘")]
    [InlineData("sleep", "💤")]
    [InlineData("food", "🥗")]
    [InlineData("write", "✍️")]
    [InlineData("music", "🎸")]
    public void GetGlyph_KnownKey_ReturnsCatalogGlyph(string key, string expectedGlyph)
    {
        Assert.Equal(expectedGlyph, HabitIcons.GetGlyph(key));
    }

    [Theory]
    [InlineData("unknown")]
    [InlineData("")]
    [InlineData("BOOK")] // сравнение ключей точное: регистр важен
    public void GetGlyph_UnknownKey_ReturnsDefaultGlyph(string key)
    {
        var defaultGlyph = HabitIcons.GetGlyph(HabitIcons.Default);

        Assert.Equal(defaultGlyph, HabitIcons.GetGlyph(key));
    }

    [Fact]
    public void GetGlyph_NullKey_ReturnsDefaultGlyph()
    {
       
        var defaultGlyph = HabitIcons.GetGlyph(HabitIcons.Default);

        Assert.Equal(defaultGlyph, HabitIcons.GetGlyph(null!));
    }

    [Fact]
    public void All_KeysAreUnique()
    {
        var keys = HabitIcons.All.Select(option => option.Key).ToList();

        Assert.Equal(keys.Count, keys.Distinct().Count());
    }

    [Fact]
    public void All_GlyphsAndTitles_AreNotEmpty()
    {
        Assert.All(HabitIcons.All, option =>
        {
            Assert.False(string.IsNullOrWhiteSpace(option.Glyph));
            Assert.False(string.IsNullOrWhiteSpace(option.Title));
        });
    }
}