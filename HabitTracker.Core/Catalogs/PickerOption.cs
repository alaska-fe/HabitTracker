using System.Diagnostics.CodeAnalysis;

namespace HabitTracker.Core.Catalogs;

/// <summary>Элемент выбора для Picker: ключ хранится в БД, Title показывается пользователю.</summary>
/// <remarks>DTO без логики: сгенерированные record-члены исключены из замера покрытия.</remarks>
[ExcludeFromCodeCoverage]
public sealed record PickerOption(string Key, string Title);