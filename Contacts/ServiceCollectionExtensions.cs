using DAL.Context;
using DAL.Factory.Interface;
using DAL.Factory;
using Microsoft.EntityFrameworkCore;

namespace Contacts;

public static class ServiceCollectionExtensions
{
    public static void RegisterPhoneBookServices(this IServiceCollection services, IConfiguration configuration)
    {
        string connectionString = configuration.GetConnectionString("DefaultConnection")!;

        Func<IPhoneBookContext> phoneBookContextProvider = () =>
        {
            DbContextOptions<PhoneBookContext> dbOptions = new DbContextOptionsBuilder<PhoneBookContext>()
                .UseSqlServer(connectionString)
                .Options;

            return new PhoneBookContext(dbOptions);
        };

        services.AddSingleton<IContextFactory<IPhoneBookContext>>(sp => new ContextFactory<IPhoneBookContext>(phoneBookContextProvider));
    }
}