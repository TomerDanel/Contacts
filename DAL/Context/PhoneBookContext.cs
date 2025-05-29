using DAL.Model;
using Microsoft.EntityFrameworkCore;

namespace DAL.Context;

public class PhoneBookContext : DbContext, IPhoneBookContext
{
    public PhoneBookContext(DbContextOptions<PhoneBookContext> options) : base(options)
    {
    }

    public DbSet<DbContact> Contacts { get; set; }
}