using DAL.Model;
using Microsoft.EntityFrameworkCore;

namespace DAL.Context;

public interface IPhoneBookContext : IDisposable
{
    DbSet<DbContact> Contacts { get; set; }
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}