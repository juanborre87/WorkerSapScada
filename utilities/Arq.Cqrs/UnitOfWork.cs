using Arq.Core;
using Microsoft.EntityFrameworkCore.Storage;
using System.Collections.Concurrent;

namespace Arq.Cqrs;

public class UnitOfWork : IUnitOfWork, IDisposable
{
    private readonly IDbContextProvider _dbContextProvider;
    private readonly ConcurrentDictionary<string, object> _repos = new();
    private readonly ConcurrentDictionary<string, IDbContextTransaction> _transactions = new();

    public UnitOfWork(IDbContextProvider dbContextProvider)
        => _dbContextProvider = dbContextProvider
        ?? throw new ArgumentNullException(nameof(dbContextProvider));


    public IEFCommandRepository<T> CommandRepository<T>(string dbChoice) where T : class
    {
        var key = $"CMD_{dbChoice}_{typeof(T).FullName}";
        return (IEFCommandRepository<T>)_repos.GetOrAdd(key, _ =>
        {
            return new EFCommandRepository<T>(_dbContextProvider.GetDbContext(dbChoice));
        });
    }

    public IEFQueryRepository<T> QueryRepository<T>(string dbChoice) where T : class
    {
        var key = $"QRY_{dbChoice}_{typeof(T).FullName}";
        return (IEFQueryRepository<T>)_repos.GetOrAdd(key, _ =>
        {
            return new EFQueryRepository<T>(_dbContextProvider.GetDbContext(dbChoice));
        });
    }

    public IDapperRepository DapperRepository(string dbChoice)
    {
        var key = $"DAP_{dbChoice}";
        return (IDapperRepository)_repos.GetOrAdd(key, _ =>
        {
            return new DapperRepository(_dbContextProvider.GetDbContext(dbChoice));
        });
    }

    public async Task BeginTransactionAsync(string dbChoice)
    {
        if (!_transactions.ContainsKey(dbChoice))
        {
            var ctx = _dbContextProvider.GetDbContext(dbChoice);
            var tx = await ctx.Database.BeginTransactionAsync();
            //_transactions[dbChoice] = tx;
            _transactions.TryAdd(dbChoice, tx);
        }
    }

    public async Task CommitAsync(string dbChoice)
    {
        if (_transactions.TryGetValue(dbChoice, out var tx))
        {
            var ctx = _dbContextProvider.GetDbContext(dbChoice);

            await ctx.SaveChangesAsync();
            await tx.CommitAsync();
            await tx.DisposeAsync();
            _transactions.TryRemove(dbChoice, out _);
            _dbContextProvider.DisposeScopeFor(dbChoice);
        }
    }

    public async Task CommitAllAsync()
    {
        foreach (var kv in _transactions.ToArray())
        {
            var dbChoice = kv.Key;
            var tx = kv.Value;

            var ctx = _dbContextProvider.GetDbContext(dbChoice);
            await ctx.SaveChangesAsync();
            await tx.CommitAsync();
            await tx.DisposeAsync();
            _transactions.TryRemove(dbChoice, out _);
            _dbContextProvider.DisposeScopeFor(dbChoice);
        }

    }

    public async Task RollbackAsync(string dbChoice)
    {
        if (_transactions.TryGetValue(dbChoice, out var tx))
        {
            await tx.RollbackAsync();
            await tx.DisposeAsync();
            _transactions.TryRemove(dbChoice, out _);
            _dbContextProvider.DisposeScopeFor(dbChoice);
        }
    }

    public async Task RollbackAllAsync()
    {
        foreach (var kv in _transactions.ToArray())
        {
            var dbChoice = kv.Key;
            var tx = kv.Value;

            await tx.RollbackAsync();
            await tx.DisposeAsync();
            _transactions.TryRemove(dbChoice, out _);
            _dbContextProvider.DisposeScopeFor(dbChoice);
        }
    }

    public void Dispose()
    {
        foreach (var kv in _transactions)
        {
            try { kv.Value.Dispose(); } catch { }
        }
        _transactions.Clear();
        _repos.Clear();
        _dbContextProvider.DisposeAllScopes();
    }

}
