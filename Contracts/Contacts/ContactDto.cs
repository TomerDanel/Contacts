﻿namespace Contracts.Contacts;

public class ContactDto
{
    public string FirstName { get; set; }
    public string LastName { get; set; }
    public string? PhoneNumber { get; set; }
    public string? Address { get; set; }
}