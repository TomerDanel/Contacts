using DAL.Model;
using Models.Contacts;

namespace DAL.Transformer.Interfaces;

public interface IContactTransformer
{
    ContactEntity TransformToContactEntity(DbContact dbContact);

    DbContact TransformToDbContact(ContactEntity contactEntity);
}