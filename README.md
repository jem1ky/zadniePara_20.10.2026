# FitnessEf

Учебное консольное приложение на .NET 8 и Entity Framework Core для работы с PostgreSQL.

## Автор

ФИО: Гашеев Никита

Репозиторий: https://github.com/jem1ky/zadniePara_20.10.2026

## Запуск

1. Установить .NET 8 SDK и PostgreSQL.
2. Установить `dotnet-ef` версии 8.x:

   ```powershell
   dotnet tool install --global dotnet-ef --version 8.*
   ```

3. Задать строку подключения без публикации пароля в репозитории:

   ```powershell
   $env:FITNESS_EF_CONNECTION = "Host=localhost;Port=5432;Database=FitnessEfTraining;Username=postgres;Password=YOUR_PASSWORD"
   ```

4. Запустить приложение:

   ```powershell
   dotnet run
   ```

Практические задания выполняются последовательно. Каждый этап имеет отдельный коммит с номером задания.

## Реализованные запросы

Класс `ClientQueries` содержит фильтрацию по статусу и фамилии, поиск по идентификатору, `CountAsync`, `AnyAsync`, сортировку и асинхронную материализацию через `ToListAsync`.

Изменение и удаление выполняются в `ClientCommands` через отслеживаемую сущность и `SaveChangesAsync`; пустые обязательные имя и фамилия отклоняются до обращения к базе.

`BirthDate`, `Phone` и `Email` допускают `NULL`, а Fluent API явно задаёт snake_case-имена, размеры строк и PostgreSQL-тип `date`.
