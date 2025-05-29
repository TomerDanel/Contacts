using Models.Contacts;

namespace BL.Services.Interfaces;

public interface IContactsService
{
    Task<IReadOnlyCollection<ContactEntity>> GetContactsAsync(int page, int pageSize);
    Task<ContactEntity?> SearchContactAsync(string phoneNumber);
    Task CreateContactAsync(ContactEntity contact);
    Task UpdateAsync(ContactEntity updatedContact);
    Task DeleteAsync(string phoneNumber);
    bool IsValidPhoneNumber(string phoneNumber);
    Task<bool> IsContactExist(string phoneNumber);
}