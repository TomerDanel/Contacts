using BL.Services.Interfaces;
using DAL.Repository.Interface;
using Microsoft.Extensions.Logging;
using Models.Contacts;
using PhoneNumbers;

namespace BL.Services;

public class ContactsService : IContactsService
{
    private readonly IContactsRepository _contactsRepository;
    private readonly ILogger<ContactsService> _logger;

    public ContactsService(IContactsRepository contactsRepository, ILogger<ContactsService> logger)
    {
        _contactsRepository = contactsRepository ?? throw new ArgumentNullException(nameof(contactsRepository));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<IReadOnlyCollection<ContactEntity>> GetContactsAsync(int page, int pageSize)
    {
        try
        {
            return await _contactsRepository.GetContactsAsync(page, pageSize);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error getting contacts for page {page} with pageSize {pageSize}");
            throw;
        }
    }

    public async Task<ContactEntity?> SearchContactAsync(string phoneNumber)
    {
        try
        {
            return await _contactsRepository.SearchContactAsync(phoneNumber);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error searching contact by phone number {phoneNumber}");
            throw;
        }
    }

    public async Task CreateContactAsync(ContactEntity contact)
    {
        try
        {
            await _contactsRepository.CreateContactAsync(contact);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error creating contact: {contact}");
            throw;
        }
    }

    public async Task UpdateAsync(ContactEntity contact)
    {
        try
        {
            await _contactsRepository.UpdateAsync(contact);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error updating contact: {contact}");
            throw;
        }
    }

    public async Task DeleteAsync(string phoneNumber)
    {
        try
        {
            await _contactsRepository.DeleteAsync(phoneNumber);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error deleting contact with phone number: {phoneNumber}");
            throw;
        }
    }

    public bool IsValidPhoneNumber(string phoneNumber)
    {
        try
        {
            PhoneNumberUtil? phoneUtil = PhoneNumberUtil.GetInstance();
            PhoneNumber? numberProto = phoneUtil.Parse(phoneNumber, null); // null = no default region, or specify e.g. "US"
            return phoneUtil.IsValidNumber(numberProto);
        }
        catch (NumberParseException)
        {
            return false;
        }
    }

    public async Task<bool> IsContactExist(string phoneNumber)
    {
        ContactEntity? existing = await SearchContactAsync(phoneNumber);

        if (existing is null)
        {
            return false;
        }

        return true;
    }
}