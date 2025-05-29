using DAL.Model;
using Microsoft.EntityFrameworkCore;

namespace DAL.Context;

public class PhoneBookContext : DbContext, IPhoneBookContext
{
    public PhoneBookContext(DbContextOptions<PhoneBookContext> options) : base(options)
    {
    }

    public DbSet<DbContact> Contacts { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<DbContact>(entity =>
        {
            entity.HasKey(e => e.Id);

            entity.Property(e => e.FirstName)
                .IsRequired()
                .HasMaxLength(100);

            entity.Property(e => e.LastName)
                .IsRequired()
                .HasMaxLength(100);

            entity.Property(e => e.PhoneNumber)
                .IsRequired()
                .HasMaxLength(20);

            entity.Property(e => e.Address)
                .HasMaxLength(200);

            entity.Property(e => e.CreatedDateUtc)
                .IsRequired();

            entity.Property(e => e.UpdateDateUtc)
                .IsRequired();
        });
    }
}