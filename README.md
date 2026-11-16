# HabitTracker — трекер привычек

Итоговый учебный проект по курсу «Разработка мобильных приложений на .NET MAUI».
Автономное приложение для отслеживания ежедневных привычек: серии дней подряд,
прогресс до цели и сводная статистика. Данные хранятся на устройстве (SQLite).

## Структура решения

| Проект | Назначение |
|---|---|
| `HabitTracker` | MAUI-приложение: страницы XAML, ViewModel, SQLite-репозиторий, службы платформы |
| `HabitTracker.Core` | Модели, бизнес-правила, сервис, интерфейсы. Без зависимостей от MAUI |
| `HabitTracker.Tests` | Модульные тесты xUnit (только против `Core`) |

## Требования

- .NET SDK 10.0 (`dotnet --version`)
- Workload: `dotnet workload install maui`
- JetBrains Rider 2025.3+
- Целевая платформа задаётся в `HabitTracker/HabitTracker.csproj` (Android или Windows)

## Сборка

    dotnet build HabitTracker.Core/HabitTracker.Core.csproj
    dotnet build HabitTracker/HabitTracker.csproj

Запуск: открыть `HabitTracker.sln` в Rider, выбрать устройство/платформу и нажать Run.

## Тесты

    dotnet test HabitTracker.Tests/HabitTracker.Tests.csproj

Тесты не требуют эмулятора, реальной БД и текущего времени: используются
тестовые двойники `FakeClock` и `InMemoryHabitRepository`.

## Отчёт

Отчёт по работе — `docs/report.md`, скриншоты — `docs/images/`
(появятся на этапе оформления).
