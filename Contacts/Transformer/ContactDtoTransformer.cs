using Contacts.Transformer.Interface;
using Contracts.Contacts;
using Models.Contacts;

namespace Contacts.Transformer;

public class ContactDtoTransformer : IContactDtoTransformer
{
    public ContactEntity TransformToContactEntity(ContactDto dbContact)
    {
        if (dbContact == null) 
            throw new ArgumentNullException(nameof(dbContact));

        return new ContactEntity
        {
            FirstName = dbContact.FirstName,
            LastName = dbContact.LastName,
            PhoneNumber = dbContact.PhoneNumber,
            Address = dbContact.Address
        };
    }

    public ContactDto TransformToContactDto(ContactEntity contactEntity)
    {
        if (contactEntity == null) 
            throw new ArgumentNullException(nameof(contactEntity));

        return new ContactDto
        {
            FirstName = contactEntity.FirstName,
            LastName = contactEntity.LastName,
            PhoneNumber = contactEntity.PhoneNumber,
            Address = contactEntity.Address
        };
    }
}