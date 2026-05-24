using LabLedger.Core.Interfaces;

namespace LabLedger.DataModel.Repositories;

public class EFUnitOfWork : IUnitOfWork
{
    private readonly LabLedgerDbContext _context;
    private readonly Dictionary<Type, object> _repositories = new();

    public EFUnitOfWork(LabLedgerDbContext context)
    {
        _context = context;
    }

    public IRepository<T> GetRepository<T>() where T : class
    {
        var type = typeof(T);

        if (_repositories.TryGetValue(type, out var repository))
            return (IRepository<T>)repository;

        var newRepository = new EFRepository<T>(_context);
        _repositories[type] = newRepository;
        return newRepository;
    }

    public Task<int> SaveChangesAsync(CancellationToken cancellationToken = default) =>
        _context.SaveChangesAsync(cancellationToken);

    public void Dispose()
    {
        _repositories.Clear();
    }
}
