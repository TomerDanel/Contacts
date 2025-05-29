using System.ComponentModel.DataAnnotations;

namespace DAL.Model;

public class DbContact
{
    [Key]
    public int Id { get; set; }
    public string FirstName { get; set; } = null!;
    public string LastName { get; set; } = null!;
    public string PhoneNumber { get; set; } = null!;
    public string? Address { get; set; }
    public DateTime CreatedDateUtc { get; set; }
    public DateTime UpdateDateUtc { get; set; }
}