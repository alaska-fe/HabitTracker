namespace HabitTracker.Core.Catalogs;


public static class HabitIcons
{
    public const string Default = "book";
    
    public static readonly IReadOnlyList<(string Key, string Glyph, string Title)> All = new[]
    {
        ("water",  "💧", "Вода"),
        ("run",    "🏃", "Бег"),
        ("book",   "📚", "Чтение"),
        ("yoga",   "🧘", "Медитация"),
        ("sleep",  "💤", "Сон"),
        ("food",   "🥗", "Питание"),
        ("write",  "✍️", "Письмо"),
        ("music",  "🎸", "Музыка"),
    };

    private static readonly string DefaultGlyph =
        All.First(option => option.Key == Default).Glyph;

 
    public static string GetGlyph(string key)
    {
        foreach (var option in All)
        {
            if (option.Key == key)
            {
                return option.Glyph;
            }
        }

        return DefaultGlyph;
    }
}