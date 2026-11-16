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

- .NET SDK 10.0
- Workload: `dotnet workload install maui`
- JetBrains Rider 2025.3+
- Android SDK / устройство или эмулятор

## Сборка

    dotnet build HabitTracker.Core/HabitTracker.Core.csproj
    dotnet build HabitTracker/HabitTracker.csproj

При сборке приложения автоматически запускаются тесты Core с порогом покрытия.
Чтобы пропустить тесты во время быстрой UI-итерации:

    dotnet build HabitTracker/HabitTracker.csproj -p:SkipTests=true

Запуск: открыть `HabitTracker.sln` в Rider, выбрать Android-устройство и нажать Run.

## Тесты и покрытие

    dotnet test HabitTracker.Tests/HabitTracker.Tests.csproj

С порогом покрытия строк Core не ниже 80%:

    dotnet test HabitTracker.Tests/HabitTracker.Tests.csproj \
      /p:CollectCoverage=true \
      /p:CoverletOutputFormat=cobertura \
      /p:Include="[HabitTracker.Core]*" \
      /p:ExcludeByAttribute=ExcludeFromCodeCoverage \
      /p:Threshold=80 /p:ThresholdType=line /p:ThresholdStat=total

## Git-хук pre-push

Хук запускает тесты с порогом покрытия перед отправкой:

    git config core.hooksPath .githooks
    chmod +x .githooks/pre-push

## CI

GitHub Actions (`.github/workflows/tests.yml`) собирает Core, прогоняет тесты
с покрытием и сохраняет HTML-отчёт как артефакт `coverage-report`.

## Отчёт

Отчёт по работе — `docs/report.md`, скриншоты — `docs/images/`.
