using Models.Contacts;

namespace DAL.Repository.Interface;

/// <summary>
/// Defines methods for performing CRUD operations on Contact entity
/// </summary>
public interface IContactsRepository 
{
    Task<IReadOnlyCollection<ContactEntity>> GetContactsAsync(int page, int pageSize);
    Task<ContactEntity?> SearchContactAsync(string phoneNumber);
    Task CreateContactAsync(ContactEntity entity);
    Task UpdateAsync(ContactEntity entity);
    Task DeleteAsync(string phoneNumber);
    Task SaveAsync();
}