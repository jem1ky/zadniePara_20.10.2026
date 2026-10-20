namespace FitnessEf;

// Сущность клиента: EF Core сопоставляет её с таблицей clients в PostgreSQL.
public sealed class Client
{
    // Первичный ключ генерируется базой данных при добавлении записи.
    public int ClientId { get; set; }

    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;

    // Nullable-типы позволяют хранить SQL NULL для необязательных данных.
    public DateOnly? BirthDate { get; set; }
    public string? Phone { get; set; }
    public string? Email { get; set; }

    // Дата регистрации получает значение по умолчанию при создании объекта в C#.
    public DateOnly RegistrationDate { get; set; } =
        DateOnly.FromDateTime(DateTime.Today);

    public string Status { get; set; } = "active";
    // Один клиент может иметь несколько посещений: это сторона "один" связи one-to-many.
    public List<Visit> Visits { get; set; } = new();
    public string? Note { get; set; }
}
