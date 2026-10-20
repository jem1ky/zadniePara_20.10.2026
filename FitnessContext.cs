using Microsoft.EntityFrameworkCore;

namespace FitnessEf;

public sealed class FitnessContext : DbContext
{
    public DbSet<Client> Clients => Set<Client>();
    public DbSet<Visit> Visits => Set<Visit>();

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        if (optionsBuilder.IsConfigured)
            return;

        var connectionString = Environment.GetEnvironmentVariable("FITNESS_EF_CONNECTION");
        if (string.IsNullOrWhiteSpace(connectionString))
        {
            throw new InvalidOperationException(
                "Не задана переменная FITNESS_EF_CONNECTION.");
        }

        optionsBuilder.UseNpgsql(connectionString);
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Client>(entity =>
        {
            entity.ToTable("clients");
            entity.HasKey(client => client.ClientId);

            entity.Property(client => client.ClientId)
                .HasColumnName("client_id")
                .ValueGeneratedOnAdd();
            entity.Property(client => client.FirstName)
                .HasColumnName("first_name")
                .HasMaxLength(100)
                .IsRequired();
            entity.Property(client => client.LastName)
                .HasColumnName("last_name")
                .HasMaxLength(100)
                .IsRequired();
            entity.Property(client => client.BirthDate)
                .HasColumnName("birth_date")
                .HasColumnType("date");
            entity.Property(client => client.Phone)
                .HasColumnName("phone")
                .HasMaxLength(30);
            entity.Property(client => client.Email)
                .HasColumnName("email")
                .HasMaxLength(254);
            entity.Property(client => client.RegistrationDate)
                .HasColumnName("registration_date")
                .HasColumnType("date")
                .IsRequired();
            entity.Property(client => client.Status)
                .HasColumnName("status")
                .HasMaxLength(20)
                .IsRequired();
            entity.Property(client => client.Note)
                .HasColumnName("note")
                .HasMaxLength(500);
        });

        modelBuilder.Entity<Visit>(entity =>
        {
            entity.ToTable("visits");
            entity.HasKey(visit => visit.VisitId);
            entity.Property(visit => visit.VisitId)
                .HasColumnName("visit_id")
                .ValueGeneratedOnAdd();
            entity.Property(visit => visit.ClientId)
                .HasColumnName("client_id")
                .IsRequired();
            entity.Property(visit => visit.VisitDate)
                .HasColumnName("visit_date")
                .HasColumnType("date")
                .IsRequired();
            entity.Property(visit => visit.Note)
                .HasColumnName("note")
                .HasMaxLength(500);
            entity.HasOne(visit => visit.Client)
                .WithMany(client => client.Visits)
                .HasForeignKey(visit => visit.ClientId)
                .OnDelete(DeleteBehavior.Restrict);
        });

    }
}
