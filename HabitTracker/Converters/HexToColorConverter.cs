using System.Globalization;
using HabitTracker.Core.Catalogs;

namespace HabitTracker.Converters;

/// <summary>
/// hex → Color (ТЗ 5.2): преобразование живёт в приложении, не в Core.
/// Для некорректного значения возвращает цвет по умолчанию из справочника (ТЗ 10.3).
/// </summary>
public sealed class HexToColorConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is string hex && !string.IsNullOrWhiteSpace(hex))
        {
            try
            {
                return Color.FromArgb(hex);
            }
            catch (ArgumentException)
            {
                // Повреждённое значение — используем цвет по умолчанию.
            }
        }

        return Color.FromArgb(HabitColors.GetHex(HabitColors.Default));
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture) =>
        throw new NotSupportedException("Преобразование цвета обратно в hex не используется.");
}