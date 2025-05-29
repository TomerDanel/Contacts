using BL.Services;
using DAL.Repository.Interface;
using Microsoft.Extensions.Logging;
using Models.Contacts;
using Moq;

namespace UnitTests.ServiceTests;

[TestClass]
public class ContactsServiceTests
{
    private Mock<IContactsRepository> _mockRepository;
    private Mock<ILogger<ContactsService>> _mockLogger;
    private ContactsService _service;

    [TestInitialize]
    public void Setup()
    {
        _mockRepository = new Mock<IContactsRepository>();
        _mockLogger = new Mock<ILogger<ContactsService>>();
        _service = new ContactsService(_mockRepository.Object, _mockLogger.Object);
    }

    #region Constructor Tests

    [TestMethod]
    public void Constructor_NullRepository_ThrowsArgumentNullException()
    {
        // Act & Assert
        var exception = Assert.ThrowsException<ArgumentNullException>(
            () => new ContactsService(null, _mockLogger.Object));

        Assert.AreEqual("contactsRepository", exception.ParamName);
    }

    [TestMethod]
    public void Constructor_NullLogger_ThrowsArgumentNullException()
    {
        // Act & Assert
        var exception = Assert.ThrowsException<ArgumentNullException>(
            () => new ContactsService(_mockRepository.Object, null));

        Assert.AreEqual("logger", exception.ParamName);
    }

    #endregion

    #region GetContactsAsync Tests

    [TestMethod]
    public async Task GetContactsAsync_Success_ReturnsContacts()
    {
        // Arrange
        var expectedContacts = new List<ContactEntity>
        {
            new() { FirstName = "John", PhoneNumber = "123" },
            new() { FirstName = "Jane", PhoneNumber = "456" }
        };
        _mockRepository.Setup(r => r.GetContactsAsync(1, 10))
            .ReturnsAsync(expectedContacts);

        // Act
        var result = await _service.GetContactsAsync(1, 10);

        // Assert
        Assert.AreEqual(2, result.Count);
        Assert.AreEqual("John", result.First().FirstName);
        _mockRepository.Verify(r => r.GetContactsAsync(1, 10), Times.Once);
    }

    [TestMethod]
    public async Task GetContactsAsync_RepositoryThrows_LogsErrorAndRethrows()
    {
        // Arrange
        var exception = new Exception("Repository error");
        _mockRepository.Setup(r => r.GetContactsAsync(1, 10))
            .ThrowsAsync(exception);

        // Act & Assert
        var thrownException = await Assert.ThrowsExceptionAsync<Exception>(
            () => _service.GetContactsAsync(1, 10));

        Assert.AreEqual("Repository error", thrownException.Message);
        _mockLogger.Verify(
            x => x.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Error getting contacts for page 1 with pageSize 10")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    #endregion

    #region SearchContactAsync Tests

    [TestMethod]
    public async Task SearchContactAsync_ContactExists_ReturnsContact()
    {
        // Arrange
        var phoneNumber = "123456789";
        var expectedContact = new ContactEntity { FirstName = "John", PhoneNumber = phoneNumber };
        _mockRepository.Setup(r => r.SearchContactAsync(phoneNumber))
            .ReturnsAsync(expectedContact);

        // Act
        var result = await _service.SearchContactAsync(phoneNumber);

        // Assert
        Assert.IsNotNull(result);
        Assert.AreEqual("John", result.FirstName);
        Assert.AreEqual(phoneNumber, result.PhoneNumber);
        _mockRepository.Verify(r => r.SearchContactAsync(phoneNumber), Times.Once);
    }

    [TestMethod]
    public async Task SearchContactAsync_ContactNotExists_ReturnsNull()
    {
        // Arrange
        var phoneNumber = "999999999";
        _mockRepository.Setup(r => r.SearchContactAsync(phoneNumber))
            .ReturnsAsync((ContactEntity?)null);

        // Act
        var result = await _service.SearchContactAsync(phoneNumber);

        // Assert
        Assert.IsNull(result);
    }

    [TestMethod]
    public async Task SearchContactAsync_RepositoryThrows_LogsErrorAndRethrows()
    {
        // Arrange
        var phoneNumber = "123456789";
        var exception = new Exception("Repository error");
        _mockRepository.Setup(r => r.SearchContactAsync(phoneNumber))
            .ThrowsAsync(exception);

        // Act & Assert
        var thrownException = await Assert.ThrowsExceptionAsync<Exception>(
            () => _service.SearchContactAsync(phoneNumber));

        Assert.AreEqual("Repository error", thrownException.Message);
        _mockLogger.Verify(
            x => x.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString().Contains($"Error searching contact by phone number {phoneNumber}")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    #endregion

    #region CreateContactAsync Tests

    [TestMethod]
    public async Task CreateContactAsync_Success_CallsRepository()
    {
        // Arrange
        var contact = new ContactEntity { FirstName = "John", PhoneNumber = "123" };

        // Act
        await _service.CreateContactAsync(contact);

        // Assert
        _mockRepository.Verify(r => r.CreateContactAsync(contact), Times.Once);
    }

    [TestMethod]
    public async Task CreateContactAsync_RepositoryThrows_LogsErrorAndRethrows()
    {
        // Arrange
        var contact = new ContactEntity { FirstName = "John", PhoneNumber = "123" };
        var exception = new Exception("Repository error");
        _mockRepository.Setup(r => r.CreateContactAsync(contact))
            .ThrowsAsync(exception);

        // Act & Assert
        var thrownException = await Assert.ThrowsExceptionAsync<Exception>(
            () => _service.CreateContactAsync(contact));

        Assert.AreEqual("Repository error", thrownException.Message);
        _mockLogger.Verify(
            x => x.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString().Contains($"Error creating contact: {contact}")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    #endregion

    #region UpdateAsync Tests

    [TestMethod]
    public async Task UpdateAsync_Success_CallsRepository()
    {
        // Arrange
        var contact = new ContactEntity { FirstName = "John", PhoneNumber = "123" };

        // Act
        await _service.UpdateAsync(contact);

        // Assert
        _mockRepository.Verify(r => r.UpdateAsync(contact), Times.Once);
    }

    [TestMethod]
    public async Task UpdateAsync_RepositoryThrows_LogsErrorAndRethrows()
    {
        // Arrange
        var contact = new ContactEntity { FirstName = "John", PhoneNumber = "123" };
        var exception = new Exception("Repository error");
        _mockRepository.Setup(r => r.UpdateAsync(contact))
            .ThrowsAsync(exception);

        // Act & Assert
        var thrownException = await Assert.ThrowsExceptionAsync<Exception>(
            () => _service.UpdateAsync(contact));

        Assert.AreEqual("Repository error", thrownException.Message);
        _mockLogger.Verify(
            x => x.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString().Contains($"Error updating contact: {contact}")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    #endregion

    #region DeleteAsync Tests

    [TestMethod]
    public async Task DeleteAsync_Success_CallsRepository()
    {
        // Arrange
        var phoneNumber = "123456789";

        // Act
        await _service.DeleteAsync(phoneNumber);

        // Assert
        _mockRepository.Verify(r => r.DeleteAsync(phoneNumber), Times.Once);
    }

    [TestMethod]
    public async Task DeleteAsync_RepositoryThrows_LogsErrorAndRethrows()
    {
        // Arrange
        var phoneNumber = "123456789";
        var exception = new Exception("Repository error");
        _mockRepository.Setup(r => r.DeleteAsync(phoneNumber))
            .ThrowsAsync(exception);

        // Act & Assert
        var thrownException = await Assert.ThrowsExceptionAsync<Exception>(
            () => _service.DeleteAsync(phoneNumber));

        Assert.AreEqual("Repository error", thrownException.Message);
        _mockLogger.Verify(
            x => x.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString().Contains($"Error deleting contact with phone number: {phoneNumber}")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    #endregion

    #region IsValidPhoneNumber Tests

    [TestMethod]
    public void IsValidPhoneNumber_ValidPhoneNumber_ReturnsTrue()
    {
        // Arrange
        var validPhoneNumber = "+972536260988"; // Valid international format

        // Act
        var result = _service.IsValidPhoneNumber(validPhoneNumber);

        // Assert
        Assert.IsTrue(result);
    }

    [TestMethod]
    public void IsValidPhoneNumber_InvalidPhoneNumber_ReturnsFalse()
    {
        // Arrange
        var invalidPhoneNumber = "invalid";

        // Act
        var result = _service.IsValidPhoneNumber(invalidPhoneNumber);

        // Assert
        Assert.IsFalse(result);
    }

    [TestMethod]
    public void IsValidPhoneNumber_EmptyString_ReturnsFalse()
    {
        // Arrange
        var emptyPhoneNumber = "";

        // Act
        var result = _service.IsValidPhoneNumber(emptyPhoneNumber);

        // Assert
        Assert.IsFalse(result);
    }

    [TestMethod]
    public void IsValidPhoneNumber_NullString_ReturnsFalse()
    {
        // Arrange
        string nullPhoneNumber = null;

        // Act
        var result = _service.IsValidPhoneNumber(nullPhoneNumber);

        // Assert
        Assert.IsFalse(result);
    }

    [TestMethod]
    [DataRow("123")]
    [DataRow("abc")]
    [DataRow("++++++")]
    [DataRow("1234567890123456")] // Too long
    public void IsValidPhoneNumber_InvalidNumbers_ReturnsFalse(string phoneNumber)
    {
        // Act
        var result = _service.IsValidPhoneNumber(phoneNumber);

        // Assert
        Assert.IsFalse(result);
    }

    #endregion

    #region IsContactExist Tests

    [TestMethod]
    public async Task IsContactExist_ContactExists_ReturnsTrue()
    {
        // Arrange
        var phoneNumber = "123456789";
        var existingContact = new ContactEntity { PhoneNumber = phoneNumber };
        _mockRepository.Setup(r => r.SearchContactAsync(phoneNumber))
            .ReturnsAsync(existingContact);

        // Act
        var result = await _service.IsContactExist(phoneNumber);

        // Assert
        Assert.IsTrue(result);
        _mockRepository.Verify(r => r.SearchContactAsync(phoneNumber), Times.Once);
    }

    [TestMethod]
    public async Task IsContactExist_ContactNotExists_ReturnsFalse()
    {
        // Arrange
        var phoneNumber = "999999999";
        _mockRepository.Setup(r => r.SearchContactAsync(phoneNumber))
            .ReturnsAsync((ContactEntity?)null);

        // Act
        var result = await _service.IsContactExist(phoneNumber);

        // Assert
        Assert.IsFalse(result);
        _mockRepository.Verify(r => r.SearchContactAsync(phoneNumber), Times.Once);
    }

    [TestMethod]
    public async Task IsContactExist_SearchThrowsException_ExceptionPropagates()
    {
        // Arrange
        var phoneNumber = "123456789";
        var exception = new Exception("Search failed");
        _mockRepository.Setup(r => r.SearchContactAsync(phoneNumber))
            .ThrowsAsync(exception);

        // Act & Assert
        var thrownException = await Assert.ThrowsExceptionAsync<Exception>(
            () => _service.IsContactExist(phoneNumber));

        Assert.AreEqual("Search failed", thrownException.Message);
        _mockRepository.Verify(r => r.SearchContactAsync(phoneNumber), Times.Once);
    }

    #endregion
}