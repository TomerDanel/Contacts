namespace Models.Contacts;

public class ContactEntity
{
    public string FirstName { get; set; } = null!;
    public string LastName { get; set; } = null!;
    public string PhoneNumber { get; set; } = null!;
    public string? Address { get; set; }

    public override string ToString()
    {
        return $"Name: {FirstName} {LastName}, Phone: {PhoneNumber}, Address: {Address ?? "N/A"}";
    }
}
