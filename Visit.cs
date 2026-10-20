namespace FitnessEf;

// Посещение принадлежит одному клиенту и хранится в таблице visits.
public sealed class Visit
{
    // Первичный ключ посещения.
    public int VisitId { get; set; }
    // Внешний ключ на clients.client_id.
    public int ClientId { get; set; }
    public DateOnly VisitDate { get; set; }
    public string? Note { get; set; }

    // Навигационное свойство позволяет использовать Include и получить клиента вместе с посещением.
    public Client Client { get; set; } = null!;
}
