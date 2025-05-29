using Contracts.Contacts;
using Models.Contacts;

namespace Contacts.Transformer.Interface;
public interface IContactDtoTransformer
{
    ContactEntity TransformToContactEntity(ContactDto dbContact);

    ContactDto TransformToContactDto(ContactEntity contactEntity);
}