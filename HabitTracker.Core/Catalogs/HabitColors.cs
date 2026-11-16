namespace HabitTracker.Core.Catalogs;

public static class HabitColors
{
    public const string Default = "blue";
    
    public static readonly IReadOnlyList<(string Key, string Hex, string Title)> All = new[]
    {
        ("blue",   "#2563EB", "Синий"),
        ("green",  "#16A34A", "Зелёный"),
        ("orange", "#EA580C", "Оранжевый"),
        ("red",    "#DC2626", "Красный"),
        ("purple", "#7C3AED", "Фиолетовый"),
        ("teal",   "#0D9488", "Бирюзовый"),
        ("pink",   "#DB2777", "Розовый"),
        ("gray",   "#475569", "Серый"),
    };

    private static readonly string DefaultHex =
        All.First(option => option.Key == Default).Hex;
    
    public static string GetHex(string key)
    {
        foreach (var option in All)
        {
            if (option.Key == key)
            {
                return option.Hex;
            }
        }

        return DefaultHex;
    }
}