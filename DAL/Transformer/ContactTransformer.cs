using DAL.Model;
using DAL.Transformer.Interfaces;
using Models.Contacts;

namespace DAL.Transformer;

public class ContactTransformer : IContactTransformer
{
    public ContactEntity TransformToContactEntity(DbContact dbContact)
    {
        if (dbContact == null)
        {
            throw new ArgumentNullException(nameof(dbContact), "The dbContact cannot be null.");
        }

        return new ContactEntity
        {
            PhoneNumber = dbContact.PhoneNumber,
            FirstName = dbContact.FirstName,
            LastName = dbContact.LastName,
            Address = dbContact.Address
        };
    }

    public DbContact TransformToDbContact(ContactEntity contactEntity)
    {
        if (contactEntity == null)
        {
            throw new ArgumentNullException(nameof(contactEntity), "The contactEntity cannot be null.");
        }

        return new DbContact()
        {
            PhoneNumber = contactEntity.PhoneNumber,
            FirstName = contactEntity.FirstName,
            LastName = contactEntity.LastName,
            Address = contactEntity.Address,
            CreatedDateUtc = DateTime.UtcNow,
            UpdateDateUtc = DateTime.UtcNow
        };
    }
}