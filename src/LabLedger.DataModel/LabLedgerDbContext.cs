using LabLedger.DataModel.Entities;
using Microsoft.EntityFrameworkCore;

namespace LabLedger.DataModel;

public class LabLedgerDbContext : DbContext
{
    public LabLedgerDbContext(DbContextOptions<LabLedgerDbContext> options)
        : base(options)
    {
    }

    public DbSet<User> Users => Set<User>();
    public DbSet<Sample> Samples => Set<Sample>();
    public DbSet<Entities.Test> Tests => Set<Entities.Test>();
    public DbSet<Result> Results => Set<Result>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(LabLedgerDbContext).Assembly);
    }
}