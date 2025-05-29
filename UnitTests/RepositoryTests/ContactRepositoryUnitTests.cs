using DAL.Context;
using DAL.Factory.Interface;
using DAL.Model;
using DAL.Repository;
using DAL.Transformer.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using MockQueryable.Moq;
using Models.Contacts;
using Moq;

namespace UnitTests.RepositoryTests;

[TestClass]
public class ContactRepositoryTests
{
    private Mock<IContextFactory<IPhoneBookContext>> _mockContextFactory;
    private Mock<IPhoneBookContext> _mockContext;
    private Mock<IContactTransformer> _mockTransformer;
    private Mock<ILogger<ContactRepository>> _mockLogger;
    private ContactRepository _repository;

    [TestInitialize]
    public void Setup()
    {
        _mockContextFactory = new Mock<IContextFactory<IPhoneBookContext>>();
        _mockContext = new Mock<IPhoneBookContext>();
        _mockTransformer = new Mock<IContactTransformer>();
        _mockLogger = new Mock<ILogger<ContactRepository>>();

        _mockContextFactory.Setup(f => f.CreateContext()).Returns(_mockContext.Object);
        _repository = new ContactRepository(_mockContextFactory.Object, _mockLogger.Object, _mockTransformer.Object);
    }

    #region GetContactsAsync Tests

    [TestMethod]
    public async Task GetContactsAsync_ReturnsPaginatedContacts()
    {
        // Arrange
        List<DbContact> dbContacts = new List<DbContact>
        {
            new() { FirstName = "Alice", PhoneNumber = "111" },
            new() { FirstName = "Bob", PhoneNumber = "222" }
        };
        var mockSet = dbContacts.AsQueryable().BuildMockDbSet();
        _mockContext.Setup(c => c.Contacts).Returns(mockSet.Object);
        _mockTransformer.Setup(t => t.TransformToContactEntity(It.IsAny<DbContact>()))
            .Returns<DbContact>(c => new ContactEntity { PhoneNumber = c.PhoneNumber });

        // Act
        IReadOnlyCollection<ContactEntity> result = await _repository.GetContactsAsync(1, 10);

        // Assert
        Assert.AreEqual(2, result.Count);
    }

    [TestMethod]
    public async Task GetContactsAsync_WithPagination_ReturnsCorrectPage()
    {
        // Arrange
        List<DbContact> dbContacts = new List<DbContact>
        {
            new() { FirstName = "Alice", PhoneNumber = "111" },
            new() { FirstName = "Bob", PhoneNumber = "222" },
            new() { FirstName = "Charlie", PhoneNumber = "333" }
        };
        var mockSet = dbContacts.AsQueryable().BuildMockDbSet();
        _mockContext.Setup(c => c.Contacts).Returns(mockSet.Object);
        _mockTransformer.Setup(t => t.TransformToContactEntity(It.IsAny<DbContact>()))
            .Returns<DbContact>(c => new ContactEntity { FirstName = c.FirstName, PhoneNumber = c.PhoneNumber });

        // Act
        IReadOnlyCollection<ContactEntity> result = await _repository.GetContactsAsync(2, 1);

        // Assert
        Assert.AreEqual(1, result.Count);
        Assert.AreEqual("Bob", result.First().FirstName); // Second item alphabetically
    }

    #endregion

    #region SearchContactAsync Tests

    [TestMethod]
    public async Task SearchContactAsync_ContactExists_ReturnsContact()
    {
        // Arrange
        var phoneNumber = "123456789";
        var dbContact = new DbContact { FirstName = "John", PhoneNumber = phoneNumber };
        var expectedEntity = new ContactEntity { FirstName = "John", PhoneNumber = phoneNumber };

        var mockSet = new List<DbContact> { dbContact }.AsQueryable().BuildMockDbSet();
        _mockContext.Setup(c => c.Contacts).Returns(mockSet.Object);
        _mockTransformer.Setup(t => t.TransformToContactEntity(dbContact))
            .Returns(expectedEntity);

        // Act
        ContactEntity? result = await _repository.SearchContactAsync(phoneNumber);

        // Assert
        Assert.IsNotNull(result);
        Assert.AreEqual(phoneNumber, result.PhoneNumber);
        Assert.AreEqual("John", result.FirstName);
    }

    [TestMethod]
    public async Task SearchContactAsync_ContactNotExists_ReturnsNull()
    {
        // Arrange
        var phoneNumber = "999999999";
        var mockSet = new List<DbContact>().AsQueryable().BuildMockDbSet();
        _mockContext.Setup(c => c.Contacts).Returns(mockSet.Object);

        // Act
        ContactEntity? result = await _repository.SearchContactAsync(phoneNumber);

        // Assert
        Assert.IsNull(result);
    }

    [TestMethod]
    public async Task SearchContactAsync_ContactNotExists_LogsWarning()
    {
        // Arrange
        var phoneNumber = "999999999";
        var mockSet = new List<DbContact>().AsQueryable().BuildMockDbSet();
        _mockContext.Setup(c => c.Contacts).Returns(mockSet.Object);

        // Act
        await _repository.SearchContactAsync(phoneNumber);

        // Assert
        _mockLogger.Verify(
            x => x.Log(
                LogLevel.Warning,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("didn't found the requested phone number")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    #endregion

    #region CreateContactAsync Tests

    [TestMethod]
    public async Task CreateContactAsync_ValidEntity_CreatesContact()
    {
        // Arrange
        var entity = new ContactEntity { FirstName = "John", PhoneNumber = "123456789" };
        var dbContact = new DbContact { FirstName = "John", PhoneNumber = "123456789" };

        var mockSet = new Mock<DbSet<DbContact>>();
        _mockContext.Setup(c => c.Contacts).Returns(mockSet.Object);
        _mockTransformer.Setup(t => t.TransformToDbContact(entity)).Returns(dbContact);

        // Act
        await _repository.CreateContactAsync(entity);

        // Assert
        mockSet.Verify(m => m.Add(It.Is<DbContact>(c => c.PhoneNumber == "123456789")), Times.Once);
        _mockContext.Verify(c => c.SaveChangesAsync(default), Times.Once);
        Assert.IsTrue(dbContact.CreatedDateUtc > DateTime.MinValue);
    }

    [TestMethod]
    public async Task CreateContactAsync_SetsCreatedDateUtc()
    {
        // Arrange
        var entity = new ContactEntity { FirstName = "John", PhoneNumber = "123456789" };
        var dbContact = new DbContact { FirstName = "John", PhoneNumber = "123456789" };
        var beforeCreate = DateTime.UtcNow;

        var mockSet = new Mock<DbSet<DbContact>>();
        _mockContext.Setup(c => c.Contacts).Returns(mockSet.Object);
        _mockTransformer.Setup(t => t.TransformToDbContact(entity)).Returns(dbContact);

        // Act
        await _repository.CreateContactAsync(entity);

        // Assert
        Assert.IsTrue(dbContact.CreatedDateUtc >= beforeCreate);
        Assert.IsTrue(dbContact.CreatedDateUtc <= DateTime.UtcNow);
    }

    #endregion

    #region UpdateAsync Tests

    [TestMethod]
    public async Task UpdateAsync_ContactExists_UpdatesContact()
    {
        // Arrange
        var phoneNumber = "123456789";
        var existingContact = new DbContact
        {
            FirstName = "John",
            LastName = "Doe",
            PhoneNumber = phoneNumber,
            Address = "Old Address"
        };
        var updateEntity = new ContactEntity
        {
            FirstName = "Jane",
            LastName = "Smith",
            PhoneNumber = phoneNumber,
            Address = "New Address"
        };

        var mockSet = new List<DbContact> { existingContact }.AsQueryable().BuildMockDbSet();
        _mockContext.Setup(c => c.Contacts).Returns(mockSet.Object);

        // Act
        await _repository.UpdateAsync(updateEntity);

        // Assert
        Assert.AreEqual("Jane", existingContact.FirstName);
        Assert.AreEqual("Smith", existingContact.LastName);
        Assert.AreEqual("New Address", existingContact.Address);
        Assert.IsTrue(existingContact.UpdateDateUtc > DateTime.MinValue);
        _mockContext.Verify(c => c.SaveChangesAsync(default), Times.Once);
    }

    [TestMethod]
    public async Task UpdateAsync_ContactNotExists_ThrowsInvalidOperationException()
    {
        // Arrange
        var updateEntity = new ContactEntity { PhoneNumber = "999999999" };
        var mockSet = new List<DbContact>().AsQueryable().BuildMockDbSet();
        _mockContext.Setup(c => c.Contacts).Returns(mockSet.Object);

        // Act & Assert
        var exception = await Assert.ThrowsExceptionAsync<InvalidOperationException>(
            () => _repository.UpdateAsync(updateEntity));

        Assert.AreEqual("Contact not found.", exception.Message);
    }

    [TestMethod]
    public async Task UpdateAsync_NullFieldsInEntity_KeepsExistingValues()
    {
        // Arrange
        var phoneNumber = "123456789";
        var existingContact = new DbContact
        {
            FirstName = "John",
            LastName = "Doe",
            PhoneNumber = phoneNumber,
            Address = "Old Address"
        };
        var updateEntity = new ContactEntity
        {
            FirstName = null, // Should keep existing
            LastName = "Smith", // Should update
            PhoneNumber = phoneNumber,
            Address = null // Should keep existing
        };

        var mockSet = new List<DbContact> { existingContact }.AsQueryable().BuildMockDbSet();
        _mockContext.Setup(c => c.Contacts).Returns(mockSet.Object);

        // Act
        await _repository.UpdateAsync(updateEntity);

        // Assert
        Assert.AreEqual("John", existingContact.FirstName); // Kept existing
        Assert.AreEqual("Smith", existingContact.LastName); // Updated
        Assert.AreEqual("Old Address", existingContact.Address); // Kept existing
    }

    #endregion

    #region DeleteAsync Tests

    [TestMethod]
    public async Task DeleteAsync_ContactExists_DeletesContact()
    {
        // Arrange
        var phoneNumber = "123456789";
        var existingContact = new DbContact { PhoneNumber = phoneNumber };

        var mockSet = new List<DbContact> { existingContact }.AsQueryable().BuildMockDbSet();
        var mockDbSet = new Mock<DbSet<DbContact>>();
        _mockContext.Setup(c => c.Contacts).Returns(mockSet.Object);

        // Act
        await _repository.DeleteAsync(phoneNumber);

        // Assert
        _mockContext.Verify(c => c.SaveChangesAsync(default), Times.Once);
    }

    [TestMethod]
    public async Task DeleteAsync_ContactNotExists_ThrowsException()
    {
        // Arrange
        var phoneNumber = "999999999";
        var mockSet = new List<DbContact>().AsQueryable().BuildMockDbSet();
        _mockContext.Setup(c => c.Contacts).Returns(mockSet.Object);

        // Act & Assert
        var exception = await Assert.ThrowsExceptionAsync<Exception>(
            () => _repository.DeleteAsync(phoneNumber));

        Assert.AreEqual("The requested phone number to be deleted is not exist", exception.Message);
    }

    [TestMethod]
    public async Task DeleteAsync_ContactNotExists_LogsWarning()
    {
        // Arrange
        var phoneNumber = "999999999";
        var mockSet = new List<DbContact>().AsQueryable().BuildMockDbSet();
        _mockContext.Setup(c => c.Contacts).Returns(mockSet.Object);

        // Act & Assert
        await Assert.ThrowsExceptionAsync<Exception>(() => _repository.DeleteAsync(phoneNumber));

        _mockLogger.Verify(
            x => x.Log(
                LogLevel.Warning,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("contact not found")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    #endregion

    #region SaveAsync Tests

    [TestMethod]
    public async Task SaveAsync_CallsSaveChanges()
    {
        // Act
        await _repository.SaveAsync();

        // Assert
        _mockContext.Verify(c => c.SaveChangesAsync(default), Times.Once);
    }

    #endregion

    #region Exception Handling Tests

    [TestMethod]
    public async Task GetContactsAsync_ThrowsException_LogsErrorAndRethrows()
    {
        // Arrange
        _mockContext.Setup(c => c.Contacts).Throws(new Exception("Database error"));

        // Act & Assert
        await Assert.ThrowsExceptionAsync<Exception>(() => _repository.GetContactsAsync(1, 10));

        _mockLogger.Verify(
            x => x.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Exception occured during GetContactsAsync")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [TestMethod]
    public async Task CreateContactAsync_ThrowsException_LogsErrorAndRethrows()
    {
        // Arrange
        var entity = new ContactEntity { PhoneNumber = "123" };
        var mockSet = new Mock<DbSet<DbContact>>();
        _mockContext.Setup(c => c.Contacts).Returns(mockSet.Object);
        _mockContext.Setup(c => c.SaveChangesAsync(default)).Throws(new Exception("Save failed"));
        _mockTransformer.Setup(t => t.TransformToDbContact(entity)).Returns(new DbContact
        {
            PhoneNumber = "123",
            FirstName = "t",
            LastName = "d"
        });

        // Act & Assert
        await Assert.ThrowsExceptionAsync<Exception>(() => _repository.CreateContactAsync(entity));

        _mockLogger.Verify(
            x => x.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Failed to create contact")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    #endregion
}