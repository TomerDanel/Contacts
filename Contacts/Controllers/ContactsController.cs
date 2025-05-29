using BL.Services.Interfaces;
using Contacts.Transformer.Interface;
using Contracts.Contacts;
using Microsoft.AspNetCore.Mvc;
using Models.Contacts;

namespace Contacts.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ContactsController : ControllerBase
{
    private readonly IContactsService _contactsService;
    private readonly IContactDtoTransformer _contactDtoTransformer;
    private readonly ILogger<ContactsController> _logger;

    public ContactsController(IContactsService contactsService, ILogger<ContactsController> logger, IContactDtoTransformer contactDtoTransformer)
    {
        _contactsService = contactsService ?? throw new ArgumentNullException(nameof(contactsService));
        _contactDtoTransformer = contactDtoTransformer ?? throw new ArgumentNullException(nameof(contactDtoTransformer));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]        // Success, returns contacts list
    [ProducesResponseType(StatusCodes.Status400BadRequest)] // Invalid query params
    [ProducesResponseType(StatusCodes.Status500InternalServerError)] // Unexpected error
    public async Task<IActionResult> GetContacts([FromQuery] int page = 1, [FromQuery] int pageSize = 10)
    {
        try
        {
            if (page < 1 || pageSize < 1 || pageSize > 10)
            {
                _logger.LogWarning("Invalid paging parameters: page={Page}, pageSize={PageSize}", page, pageSize);
                return BadRequest("Page must be >= 1 and PageSize must be between 1 and 10.");
            }

            IReadOnlyCollection<ContactEntity> result = await _contactsService.GetContactsAsync(page, pageSize);

            IReadOnlyCollection<ContactDto> contacts = result.Select(_contactDtoTransformer.TransformToContactDto).ToList();

            return Ok(contacts);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while getting contacts.");
            return StatusCode(StatusCodes.Status500InternalServerError, "An error occurred while processing your request.");
        }
    }

    [HttpGet("search")]
    [ProducesResponseType(StatusCodes.Status200OK)]          // Found
    [ProducesResponseType(StatusCodes.Status400BadRequest)]   // Missing phone number
    [ProducesResponseType(StatusCodes.Status404NotFound)]     // No matching contacts
    [ProducesResponseType(StatusCodes.Status500InternalServerError)] // Unexpected error
    public async Task<IActionResult> SearchContact([FromQuery] string phoneNumber)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(phoneNumber))
            {
                _logger.LogWarning("Phone number search attempted with empty phoneNumber.");
                return BadRequest("Phone number must be provided.");
            }

            ContactEntity? result = await _contactsService.SearchContactAsync(phoneNumber);

            if (result is null)
            {
                _logger.LogInformation("No contact found for phone number: {PhoneNumber}", phoneNumber);
                return NotFound("No contact found with the provided phone number.");
            }

            ContactDto contact = _contactDtoTransformer.TransformToContactDto(result);

            return Ok(contact);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while searching for contact with phone number: {PhoneNumber}", phoneNumber);
            return StatusCode(StatusCodes.Status500InternalServerError, "An error occurred while processing your request.");
        }
    }

    [HttpPost]
    [ProducesResponseType(StatusCodes.Status201Created)]       // Created
    [ProducesResponseType(StatusCodes.Status400BadRequest)]    // Validation failure
    [ProducesResponseType(StatusCodes.Status409Conflict)]      // Duplicate phone number
    [ProducesResponseType(StatusCodes.Status500InternalServerError)] // Unexpected error
    public async Task<IActionResult> CreateContact([FromBody] ContactDto contactDto)
    {
        try
        {
            ContactEntity contact = _contactDtoTransformer.TransformToContactEntity(contactDto);

            bool isContactExist = await _contactsService.IsContactExist(contact.PhoneNumber);

            if (isContactExist)
            {
                _logger.LogWarning("Attempt to create duplicate contact with phone number: {PhoneNumber}", contact.PhoneNumber);
                return Conflict("A contact with the same phone number already exists.");
            }

            bool isValidPhoneNumber = _contactsService.IsValidPhoneNumber(contact.PhoneNumber);

            if (!isValidPhoneNumber)
            {
                _logger.LogWarning("Invalid phone number format: {PhoneNumber}", contact.PhoneNumber);
                return BadRequest("Invalid phone number format.");
            }

            await _contactsService.CreateContactAsync(contact);

            return StatusCode(StatusCodes.Status201Created, contact);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while creating contact.");
            return StatusCode(StatusCodes.Status500InternalServerError,"An error occurred while processing your request.");
        }
    }

    [HttpPut("{phoneNumber}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]     // Updated successfully
    [ProducesResponseType(StatusCodes.Status400BadRequest)]    // Validation failure
    [ProducesResponseType(StatusCodes.Status404NotFound)]      // Contact not found
    [ProducesResponseType(StatusCodes.Status500InternalServerError)] // Unexpected error
    public async Task<IActionResult> UpdateContact(string phoneNumber, [FromBody] ContactDto contactDto)
    {
        try
        {
            ContactEntity contact = _contactDtoTransformer.TransformToContactEntity(contactDto);

            bool isContactExist = await _contactsService.IsContactExist(contact.PhoneNumber);

            if (isContactExist)
            {
                _logger.LogInformation("Attempt to update non-existent contact with phone number: {PhoneNumber}", phoneNumber);
                return NotFound("Contact not found.");
            }

            if (contactDto.PhoneNumber != null && contactDto.PhoneNumber != phoneNumber)
            {
                //check if the new phone number already exists
                bool isPhoneNumberExist = await _contactsService.IsContactExist(contactDto.PhoneNumber);

                if (isPhoneNumberExist)
                {
                    _logger.LogWarning("Attempt to update contact with duplicate phone number: {PhoneNumber}", contactDto.PhoneNumber);
                    return Conflict("A contact with the same phone number already exists.");
                }

                bool isValidPhoneNumber = _contactsService.IsValidPhoneNumber(contactDto.PhoneNumber);

                if (!isValidPhoneNumber)
                {
                    _logger.LogWarning("Invalid new phone number format: {PhoneNumber}", contact.PhoneNumber);
                    return BadRequest("Invalid new phone number format.");
                }
            }

            await _contactsService.UpdateAsync(contact);

            return Accepted();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while updating contact with phone number: {PhoneNumber}", phoneNumber);
            return StatusCode(StatusCodes.Status500InternalServerError, "An error occurred while processing your request.");
        }
    }

    [HttpDelete("{phoneNumber}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]     // Deleted successfully
    [ProducesResponseType(StatusCodes.Status404NotFound)]      // Contact not found
    [ProducesResponseType(StatusCodes.Status500InternalServerError)] // Unexpected error
    public async Task<IActionResult> DeleteContact(string phoneNumber)
    {
        try
        {
            bool isContactExist = await _contactsService.IsContactExist(phoneNumber);
            if (!isContactExist)
            {
                _logger.LogInformation("Attempt to delete non-existent contact with phone number: {PhoneNumber}", phoneNumber);
                return NotFound("Contact not found.");
            }

            await _contactsService.DeleteAsync(phoneNumber);

            return Accepted();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while deleting contact with phone number: {PhoneNumber}", phoneNumber);
            return StatusCode(StatusCodes.Status500InternalServerError, "An error occurred while processing your request.");
        }
    }
}