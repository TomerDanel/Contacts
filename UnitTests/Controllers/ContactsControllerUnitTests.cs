using BL.Services.Interfaces;
using Contacts.Controllers;
using Contacts.Transformer.Interface;
using Contracts.Contacts;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Models.Contacts;
using Moq;

namespace UnitTests.Controllers;

[TestClass]
public class ContactsControllerTests
{
    private Mock<IContactsService> _contactsServiceMock;
    private Mock<IContactDtoTransformer> _transformerMock;
    private Mock<ILogger<ContactsController>> _loggerMock;
    private ContactsController _controller;

    [TestInitialize]
    public void Setup()
    {
        _contactsServiceMock = new Mock<IContactsService>();
        _transformerMock = new Mock<IContactDtoTransformer>();
        _loggerMock = new Mock<ILogger<ContactsController>>();

        _controller = new ContactsController(
            _contactsServiceMock.Object,
            _loggerMock.Object,
            _transformerMock.Object);
    }

    [TestMethod]
    public async Task TestGetContacts_WhenValidRequest_ShouldReturnsOk()
    {
        var contacts = new List<ContactEntity> { new ContactEntity { PhoneNumber = "123" } };
        var contactDtos = new List<ContactDto> { new ContactDto { PhoneNumber = "123" } };

        _contactsServiceMock.Setup(s => s.GetContactsAsync(1, 10)).ReturnsAsync(contacts);
        _transformerMock.Setup(t => t.TransformToContactDto(It.IsAny<ContactEntity>())).Returns(contactDtos[0]);

        var result = await _controller.GetContacts(1, 10);

        Assert.IsInstanceOfType(result, typeof(OkObjectResult));
    }

    [TestMethod]
    public async Task TestGetContacts_WhenInvalidPaging_ShouldReturnsBadRequest()
    {
        var result = await _controller.GetContacts(0, 100);
        Assert.IsInstanceOfType(result, typeof(BadRequestObjectResult));
    }

    [TestMethod]
    public async Task TestSearchContact_WhenValidPhone_ShouldReturnsOk()
    {
        var entity = new ContactEntity { PhoneNumber = "123" };
        var dto = new ContactDto { PhoneNumber = "123" };

        _contactsServiceMock.Setup(s => s.SearchContactAsync("123")).ReturnsAsync(entity);
        _transformerMock.Setup(t => t.TransformToContactDto(entity)).Returns(dto);

        var result = await _controller.SearchContact("123");
        Assert.IsInstanceOfType(result, typeof(OkObjectResult));
    }

    [TestMethod]
    public async Task TestSearchContact_WhenNotFound_ShouldReturnsNotFound()
    {
        _contactsServiceMock.Setup(s => s.SearchContactAsync("999")).ReturnsAsync((ContactEntity)null);

        var result = await _controller.SearchContact("999");
        Assert.IsInstanceOfType(result, typeof(NotFoundObjectResult));
    }

    [TestMethod]
    public async Task TestCreateContact_WhenValidContact_ShouldReturnsCreated()
    {
        var dto = new ContactDto { PhoneNumber = "123" };
        var entity = new ContactEntity { PhoneNumber = "123" };

        _transformerMock.Setup(t => t.TransformToContactEntity(dto)).Returns(entity);
        _contactsServiceMock.Setup(s => s.IsContactExist("123")).ReturnsAsync(false);
        _contactsServiceMock.Setup(s => s.IsValidPhoneNumber("123")).Returns(true);

        var result = await _controller.CreateContact(dto);
        Assert.IsInstanceOfType(result, typeof(ObjectResult));
        Assert.AreEqual(201, (result as ObjectResult).StatusCode);
    }

    [TestMethod]
    public async Task TestCreateContact_WhenDuplicatePhone_ShouldReturnsConflict()
    {
        var dto = new ContactDto { PhoneNumber = "123" };
        var entity = new ContactEntity { PhoneNumber = "123" };

        _transformerMock.Setup(t => t.TransformToContactEntity(dto)).Returns(entity);
        _contactsServiceMock.Setup(s => s.IsContactExist("123")).ReturnsAsync(true);

        var result = await _controller.CreateContact(dto);
        Assert.IsInstanceOfType(result, typeof(ConflictObjectResult));
    }

    [TestMethod]
    public async Task TestCreateContact_WhenInvalidPhone_ShouldReturnsBadRequest()
    {
        var dto = new ContactDto { PhoneNumber = "123" };
        var entity = new ContactEntity { PhoneNumber = "123" };

        _transformerMock.Setup(t => t.TransformToContactEntity(dto)).Returns(entity);
        _contactsServiceMock.Setup(s => s.IsContactExist("123")).ReturnsAsync(false);
        _contactsServiceMock.Setup(s => s.IsValidPhoneNumber("123")).Returns(false);

        var result = await _controller.CreateContact(dto);
        Assert.IsInstanceOfType(result, typeof(BadRequestObjectResult));
    }

    [TestMethod]
    public async Task TestUpdateContact_WhenValidRequest_ShouldReturnsAccepted()
    {
        var dto = new ContactDto { PhoneNumber = "123" };
        var entity = new ContactEntity { PhoneNumber = "123" };

        _transformerMock.Setup(t => t.TransformToContactEntity(dto)).Returns(entity);
        _contactsServiceMock.Setup(s => s.IsContactExist("123")).ReturnsAsync(false);
        _contactsServiceMock.Setup(s => s.IsValidPhoneNumber("123")).Returns(true);

        var result = await _controller.UpdateContact("123", dto);
        Assert.IsInstanceOfType(result, typeof(AcceptedResult));
    }

    [TestMethod]
    public async Task TestUpdateContact_WhenContactExists_ShouldReturnsNotFound()
    {
        var dto = new ContactDto { PhoneNumber = "123" };
        var entity = new ContactEntity { PhoneNumber = "123" };

        _transformerMock.Setup(t => t.TransformToContactEntity(dto)).Returns(entity);
        _contactsServiceMock.Setup(s => s.IsContactExist("123")).ReturnsAsync(true);

        var result = await _controller.UpdateContact("123", dto);
        Assert.IsInstanceOfType(result, typeof(NotFoundObjectResult));
    }

    [TestMethod]
    public async Task TestDeleteContact_WhenExists_ShouldReturnsAccepted()
    {
        _contactsServiceMock.Setup(s => s.IsContactExist("123")).ReturnsAsync(true);

        var result = await _controller.DeleteContact("123");
        Assert.IsInstanceOfType(result, typeof(AcceptedResult));
    }

    [TestMethod]
    public async Task TestDeleteContact_WhenNotFound_ShouldReturnsNotFound()
    {
        _contactsServiceMock.Setup(s => s.IsContactExist("999")).ReturnsAsync(false);

        var result = await _controller.DeleteContact("999");
        Assert.IsInstanceOfType(result, typeof(NotFoundObjectResult));
    }
}
