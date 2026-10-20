using Microsoft.EntityFrameworkCore;

namespace FitnessEf;

public static class ClientQueries
{
    public static async Task<IReadOnlyList<Client>> ListAsync(
        FitnessContext db,
        CancellationToken cancellationToken = default)
    {
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
        return await db.Clients
            .AsNoTracking()
            .FirstOrDefaultAsync(client => client.ClientId == id, cancellationToken);
    }

    public static async Task<IReadOnlyList<Client>> SearchByLastNameAsync(
        FitnessContext db,
        string term,
        CancellationToken cancellationToken = default)
    {
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
        db.Clients.CountAsync(cancellationToken);

    public static Task<int> CountActiveAsync(
        FitnessContext db,
        CancellationToken cancellationToken = default) =>
        db.Clients.CountAsync(client => client.Status == "active", cancellationToken);

    public static Task<bool> HasInactiveAsync(
        FitnessContext db,
        CancellationToken cancellationToken = default) =>
        db.Clients.AnyAsync(client => client.Status == "inactive", cancellationToken);

    public static async Task<IReadOnlyList<Client>> ListWithVisitsAsync(
        FitnessContext db,
        CancellationToken cancellationToken = default)
    {
        return await db.Clients
            .AsNoTracking()
            .Include(client => client.Visits)
            .OrderBy(client => client.LastName)
            .ThenBy(client => client.FirstName)
            .ToListAsync(cancellationToken);
    }

}
