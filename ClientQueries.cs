using Microsoft.EntityFrameworkCore;

namespace FitnessEf;

// Методы этого класса только читают данные и не меняют состояние базы.
public static class ClientQueries
{
    public static async Task<IReadOnlyList<Client>> ListAsync(
        FitnessContext db,
        CancellationToken cancellationToken = default)
    {
        // Сортировка выполняется в SQL, а ToListAsync материализует результат асинхронно.
        return await db.Clients
            .AsNoTracking()
            .OrderBy(client => client.LastName)
            .ThenBy(client => client.FirstName)
            .ToListAsync(cancellationToken);
    }

    public static async Task<Client?> FindByIdAsync(
        FitnessContext db,
        int id,
        CancellationToken cancellationToken = default)
    {
        // FirstOrDefaultAsync возвращает null, если клиента с таким ключом нет.
        return await db.Clients
            .AsNoTracking()
            .FirstOrDefaultAsync(client => client.ClientId == id, cancellationToken);
    }

    public static async Task<IReadOnlyList<Client>> SearchByLastNameAsync(
        FitnessContext db,
        string term,
        CancellationToken cancellationToken = default)
    {
        // Where фильтрует строки на стороне БД, а не после загрузки всего списка.
        return await db.Clients
            .AsNoTracking()
            .Where(client => client.LastName.Contains(term))
            .OrderBy(client => client.LastName)
            .ThenBy(client => client.FirstName)
            .ToListAsync(cancellationToken);
    }

    public static Task<int> CountAsync(
        FitnessContext db,
        CancellationToken cancellationToken = default) =>
        // CountAsync переводится в SELECT COUNT(*).
        db.Clients.CountAsync(cancellationToken);

    public static Task<int> CountActiveAsync(
        FitnessContext db,
        CancellationToken cancellationToken = default) =>
        db.Clients.CountAsync(client => client.Status == "active", cancellationToken);

    public static Task<bool> HasInactiveAsync(
        FitnessContext db,
        CancellationToken cancellationToken = default) =>
        // AnyAsync проверяет наличие хотя бы одной строки эффективнее полного списка.
        db.Clients.AnyAsync(client => client.Status == "inactive", cancellationToken);

    public static async Task<IReadOnlyList<Client>> ListWithVisitsAsync(
        FitnessContext db,
        CancellationToken cancellationToken = default)
    {
        // Include загружает связанные записи visits для каждого клиента.
        return await db.Clients
            .AsNoTracking()
            .Include(client => client.Visits)
            .OrderBy(client => client.LastName)
            .ThenBy(client => client.FirstName)
            .ToListAsync(cancellationToken);
    }

}
