using DAL.Context;
using DAL.Factory.Interface;
using DAL.Model;
using DAL.Repository.Interface;
using DAL.Transformer.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Models.Contacts;

namespace DAL.Repository;

public class ContactRepository : IContactsRepository
{
    private readonly IContextFactory<IPhoneBookContext> _phoneBookContextFactory;
    private readonly IContactTransformer _contactTransformer;
    private readonly ILogger<ContactRepository> _logger;

    public ContactRepository(IContextFactory<IPhoneBookContext> phoneBookContextFactory, ILogger<ContactRepository> logger, IContactTransformer contactTransformer) 
    {
        _phoneBookContextFactory = phoneBookContextFactory ?? throw new ArgumentNullException(nameof(phoneBookContextFactory));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _contactTransformer = contactTransformer ?? throw new ArgumentNullException(nameof(contactTransformer));
    }

    //Handle Pagination correctly
    public async Task<IReadOnlyCollection<ContactEntity>> GetContactsAsync(int page, int pageSize)
    {
        try
        {
            using IPhoneBookContext context = _phoneBookContextFactory.CreateContext();

            IReadOnlyCollection<DbContact> contacts = await context.Contacts
                .OrderBy(c => c.FirstName)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            IReadOnlyCollection<ContactEntity> contactEntities =
                contacts.Select(_contactTransformer.TransformToContactEntity).ToList();

            return contactEntities;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Exception occured during {nameof(GetContactsAsync)} for {nameof(page)}: [{page}] and {nameof(pageSize)}: [{pageSize}]");
            throw;
        }
    }

    public async Task<ContactEntity?> SearchContactAsync(string phoneNumber)
    {
        try
        {
            using IPhoneBookContext context = _phoneBookContextFactory.CreateContext();

            DbContact? dbContact = await context.Contacts
                .FirstOrDefaultAsync(c => c.PhoneNumber == phoneNumber);

            if (dbContact is null)
            {
                _logger.LogWarning(
                    $"{nameof(SearchContactAsync)} didn't found the requested phone number {phoneNumber}");
                return null;
            }

            return _contactTransformer.TransformToContactEntity(dbContact);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Exception occured during {nameof(SearchContactAsync)} for {nameof(phoneNumber)}: [{phoneNumber}]");
            throw;
        }
    }

    public async Task CreateContactAsync(ContactEntity entity)
    {
        try
        {
            using IPhoneBookContext context = _phoneBookContextFactory.CreateContext();

            DbContact dbContact = _contactTransformer.TransformToDbContact(entity);
            dbContact.CreatedDateUtc = DateTime.UtcNow;

            context.Contacts.Add(dbContact);

            await context.SaveChangesAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"{nameof(CreateContactAsync)}: Failed to create contact for {nameof(ContactEntity)}: {entity}");
            throw;
        }
    }

    public async Task UpdateAsync(ContactEntity entity)
    {
        try
        {
            using IPhoneBookContext context = _phoneBookContextFactory.CreateContext();

            DbContact? dbContact = await context.Contacts
                .FirstOrDefaultAsync(c => c.PhoneNumber == entity.PhoneNumber);

            if (dbContact == null)
            {
                _logger.LogWarning($"{nameof(UpdateAsync)}: Contact not found for {nameof(entity.PhoneNumber)}: [{entity.PhoneNumber}]");
                throw new InvalidOperationException("Contact not found.");
            }

            dbContact.FirstName = entity.FirstName ?? dbContact.FirstName;
            dbContact.LastName = entity.LastName ?? dbContact.LastName;
            dbContact.Address = entity.Address ?? dbContact.Address;
            dbContact.PhoneNumber = entity.PhoneNumber ?? dbContact.PhoneNumber;
            dbContact.UpdateDateUtc = DateTime.UtcNow;

            //context.Contacts.Update(dbUpdateContact);

            await context.SaveChangesAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"{nameof(UpdateAsync)}: Failed to update contact for {nameof(ContactEntity)}: {entity}");
            throw;
        }
    }

    public async Task DeleteAsync(string phoneNumber)
    {
        try
        {
            using IPhoneBookContext context = _phoneBookContextFactory.CreateContext();

            DbContact? contact = await context.Contacts
                .FirstOrDefaultAsync(c => c.PhoneNumber == phoneNumber);

            if (contact == null)
            {
                _logger.LogWarning($"{nameof(DeleteAsync)}: contact not found with {nameof(phoneNumber)}: [{phoneNumber}]");
                throw new Exception("The requested phone number to be deleted is not exist");
            }

            context.Contacts.Remove(contact);
            await context.SaveChangesAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"{nameof(DeleteAsync)}: Failed to delete contact with {nameof(phoneNumber)}: [{phoneNumber}]");
            throw;
        }
    }

    public async Task SaveAsync()
    {
        try
        {
            using IPhoneBookContext context = _phoneBookContextFactory.CreateContext();

            await context.SaveChangesAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"{nameof(SaveAsync)}: Failed to save changes to the context.");
            throw;
        }
    }
}