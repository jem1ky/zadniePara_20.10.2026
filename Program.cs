using FitnessEf;
using Microsoft.EntityFrameworkCore;

// Точка входа консольного приложения: меню повторяется, пока пользователь не выберет выход.
while (true)
{
    Console.WriteLine();
    Console.WriteLine("1. Список клиентов");
    Console.WriteLine("2. Найти клиента по ID");
    Console.WriteLine("3. Найти по фамилии");
    Console.WriteLine("4. Добавить клиента");
    Console.WriteLine("5. Изменить телефон или статус");
    Console.WriteLine("6. Удалить клиента");
    Console.WriteLine("7. Статистика");
    Console.WriteLine("8. Клиенты с посещениями");
    Console.WriteLine("9. Показать SQL активных клиентов");
    Console.WriteLine("0. Выход");
    Console.Write("Выбор: ");

    // Trim убирает случайные пробелы из пользовательского ввода.
    var choice = Console.ReadLine()?.Trim();
    if (choice == "0")
        break;

    // Ошибки отдельной операции не завершают всё приложение: пользователь возвращается в меню.
    try
    {
        await ExecuteChoiceAsync(choice);
    }
    catch (InvalidOperationException ex)
    {
        Console.WriteLine($"Ошибка конфигурации: {ex.Message}");
    }
    catch (DbUpdateException ex)
    {
        Console.WriteLine("Ошибка сохранения данных или ограничения внешнего ключа.");
        Console.WriteLine(ex.GetBaseException().Message);
    }
    catch (Exception ex)
    {
        Console.WriteLine($"Непредвиденная ошибка: {ex.Message}");
    }
}

static async Task ExecuteChoiceAsync(string? choice)
{
    // Для каждого пункта меню вызывается отдельный метод с короткоживущим DbContext.
    switch (choice)
    {
        case "1": await ListClientsAsync(); break;
        case "2": await FindClientAsync(); break;
        case "3": await SearchClientsAsync(); break;
        case "4": await AddClientAsync(); break;
        case "5": await UpdateClientAsync(); break;
        case "6": await DeleteClientAsync(); break;
        case "7": await ShowStatisticsAsync(); break;
        case "8": await ListClientsWithVisitsAsync(); break;
        case "9": await ShowSqlAsync(); break;
        default: Console.WriteLine("Неизвестный пункт меню."); break;
    }
}

static async Task ListClientsAsync()
{
    // AsNoTracking подходит для чтения: EF Core не тратит память на отслеживание изменений.
    await using var db = new FitnessContext();
    var clients = await ClientQueries.ListAsync(db);
    foreach (var client in clients)
        Console.WriteLine($"{client.ClientId}: {client.FirstName} {client.LastName}; статус: {client.Status}");
}

static async Task FindClientAsync()
{
    // Сначала проверяем формат ID, затем выполняем параметризованный запрос через EF Core.
    if (!TryReadId(out var id))
        return;

    await using var db = new FitnessContext();
    var client = await ClientQueries.FindByIdAsync(db, id);
    Console.WriteLine(client is null
        ? "Клиент не найден."
        : $"{client.ClientId}: {client.FirstName} {client.LastName}; телефон: {client.Phone ?? "не указан"}");
}

static async Task SearchClientsAsync()
{
    // Поиск по фамилии превращается провайдером Npgsql в SQL с параметром.
    Console.Write("Фрагмент фамилии: ");
    var term = Console.ReadLine()?.Trim() ?? string.Empty;
    if (term.Length == 0)
    {
        Console.WriteLine("Строка поиска не задана.");
        return;
    }

    await using var db = new FitnessContext();
    var clients = await ClientQueries.SearchByLastNameAsync(db, term);
    foreach (var client in clients)
        Console.WriteLine($"{client.ClientId}: {client.FirstName} {client.LastName}");
}

static async Task AddClientAsync()
{
    // Данные читаются из консоли, проверяются в ClientCommands и сохраняются одним SaveChangesAsync.
    Console.Write("Имя: ");
    var firstName = Console.ReadLine()?.Trim() ?? string.Empty;
    Console.Write("Фамилия: ");
    var lastName = Console.ReadLine()?.Trim() ?? string.Empty;
    Console.Write("Дата рождения (ГГГГ-ММ-ДД): ");
    var birthDateText = Console.ReadLine()?.Trim() ?? string.Empty;
    if (!DateOnly.TryParse(birthDateText, out var birthDate))
    {
        Console.WriteLine("Дата рождения обязательна и должна быть в формате ГГГГ-ММ-ДД.");
        return;
    }
    Console.Write("Телефон (необязательно): ");
    var phone = Console.ReadLine();
    Console.Write("Email (необязательно): ");
    var email = Console.ReadLine();

    await using var db = new FitnessContext();
    var client = await ClientCommands.AddAsync(
        db,
        firstName,
        lastName,
        birthDate,
        phone,
        email);
    Console.WriteLine($"Клиент добавлен. ID: {client.ClientId}");
}

static async Task UpdateClientAsync()
{
    // Загруженная без AsNoTracking сущность отслеживается контекстом, поэтому изменения обнаруживаются автоматически.
    if (!TryReadId(out var id))
        return;

    Console.Write("Новый телефон (пусто - оставить без изменения): ");
    var phone = Console.ReadLine();
    Console.Write("Новый статус (пусто - оставить без изменения): ");
    var status = Console.ReadLine();
    phone = string.IsNullOrWhiteSpace(phone) ? null : phone;
    status = string.IsNullOrWhiteSpace(status) ? null : status;

    await using var db = new FitnessContext();
    Console.WriteLine(await ClientCommands.UpdateAsync(db, id, phone, status)
        ? "Клиент изменён."
        : "Клиент не найден.");
}

static async Task DeleteClientAsync()
{
    // Remove помечает сущность на удаление; фактический DELETE отправляется при SaveChangesAsync.
    if (!TryReadId(out var id))
        return;

    await using var db = new FitnessContext();
    Console.WriteLine(await ClientCommands.DeleteAsync(db, id)
        ? "Клиент удалён."
        : "Клиент не найден.");
}

static async Task ShowStatisticsAsync()
{
    // CountAsync и AnyAsync выполняются на стороне PostgreSQL и не загружают весь список в память.
    await using var db = new FitnessContext();
    var total = await ClientQueries.CountAsync(db);
    var active = await ClientQueries.CountActiveAsync(db);
    var hasInactive = await ClientQueries.HasInactiveAsync(db);
    Console.WriteLine($"Всего: {total}; активных: {active}; есть неактивные: {hasInactive}");
}

static async Task ListClientsWithVisitsAsync()
{
    // Include загружает связанные посещения одним запросом EF Core.
    await using var db = new FitnessContext();
    var clients = await ClientQueries.ListWithVisitsAsync(db);
    foreach (var client in clients)
    {
        Console.WriteLine($"{client.FirstName} {client.LastName}");
        foreach (var visit in client.Visits)
            Console.WriteLine($"  {visit.VisitDate}: {visit.Note ?? "без заметки"}");
    }
}

static async Task ShowSqlAsync()
{
    // ToQueryString показывает SQL, который EF Core построил из LINQ-выражения.
    await using var db = new FitnessContext();
    var query = db.Clients
        .Where(client => client.Status == "active")
        .OrderBy(client => client.LastName);
    Console.WriteLine(query.ToQueryString());
    await query.CountAsync();
}

static bool TryReadId(out int id)
{
    // Метод возвращает false вместо исключения, если пользователь ввёл не положительный integer.
    Console.Write("ID клиента: ");
    if (int.TryParse(Console.ReadLine(), out id) && id > 0)
        return true;

    Console.WriteLine("ID должен быть положительным целым числом.");
    return false;
}
