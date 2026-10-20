using Microsoft.EntityFrameworkCore;

namespace FitnessEf;

public static class ClientCommands
{
    public static async Task<Client> AddAsync(
        FitnessContext db,
        string firstName,
        string lastName,
        DateOnly? birthDate = null,
        string? phone = null,
        string? email = null,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(firstName) || string.IsNullOrWhiteSpace(lastName))
            throw new ArgumentException("Имя и фамилия обязательны.");

        var client = new Client
        {
            FirstName = firstName.Trim(),
            LastName = lastName.Trim(),
            BirthDate = birthDate,
            Phone = string.IsNullOrWhiteSpace(phone) ? null : phone.Trim(),
            Email = string.IsNullOrWhiteSpace(email) ? null : email.Trim(),
            RegistrationDate = DateOnly.FromDateTime(DateTime.Today),
            Status = "active"
        };

        db.Clients.Add(client);
        await db.SaveChangesAsync(cancellationToken);
        return client;
    }

    public static async Task<bool> UpdateAsync(
        FitnessContext db,
        int id,
        string? phone,
        string? status,
        CancellationToken cancellationToken = default)
    {
        var client = await db.Clients.FirstOrDefaultAsync(
            item => item.ClientId == id,
            cancellationToken);
        if (client is null)
            return false;

        if (phone is not null)
            client.Phone = string.IsNullOrWhiteSpace(phone) ? null : phone.Trim();
        if (status is not null)
            client.Status = status.Trim();

        await db.SaveChangesAsync(cancellationToken);
        return true;
    }

    public static async Task<bool> DeleteAsync(
        FitnessContext db,
        int id,
        CancellationToken cancellationToken = default)
    {
        var client = await db.Clients.FindAsync([id], cancellationToken);
        if (client is null)
            return false;

        db.Clients.Remove(client);
        await db.SaveChangesAsync(cancellationToken);
        return true;
    }

    public static async Task AddVisitAsync(
        FitnessContext db,
        int clientId,
        DateOnly visitDate,
        string? note,
        CancellationToken cancellationToken = default)
    {
        if (!await db.Clients.AnyAsync(client => client.ClientId == clientId, cancellationToken))
            throw new ArgumentException("Клиент не найден.");

        db.Visits.Add(new Visit
        {
            ClientId = clientId,
            VisitDate = visitDate,
            Note = string.IsNullOrWhiteSpace(note) ? null : note.Trim()
        });
        await db.SaveChangesAsync(cancellationToken);
    }

}
