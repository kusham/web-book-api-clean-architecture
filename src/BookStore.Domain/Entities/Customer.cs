using BookStore.Domain.Enums;
using BookStore.Domain.Exceptions;
using BookStore.Domain.ValueObjects;

namespace BookStore.Domain.Entities;

public class Customer : BaseEntity
{
    public string FirstName { get; private set; }
    public string LastName { get; private set; }
    public string Email { get; private set; }
    public string PhoneNumber { get; private set; }
    public Address Address { get; private set; }
    public CustomerStatus Status { get; private set; }
    public DateTime RegistrationDate { get; private set; }
    public DateTime? LastLoginDate { get; private set; }

    // Navigation properties
    public ICollection<Order> Orders { get; private set; } = new List<Order>();

    private Customer() { } // For EF Core

    public Customer(string firstName, string lastName, string email, string phoneNumber, Address address)
    {
        ValidateCustomerData(firstName, lastName, email, phoneNumber);
        
        FirstName = firstName;
        LastName = lastName;
        Email = email;
        PhoneNumber = phoneNumber;
        Address = address;
        Status = CustomerStatus.Active;
        RegistrationDate = DateTime.UtcNow;
    }

    public string FullName => $"{FirstName} {LastName}";

    public void UpdateContactInfo(string email, string phoneNumber)
    {
        if (string.IsNullOrWhiteSpace(email))
            throw new DomainException("Email cannot be empty.");
        
        if (string.IsNullOrWhiteSpace(phoneNumber))
            throw new DomainException("Phone number cannot be empty.");
        
        Email = email;
        PhoneNumber = phoneNumber;
    }

    public void UpdateAddress(Address newAddress)
    {
        Address = newAddress ?? throw new DomainException("Address cannot be null.");
    }

    public void UpdateName(string firstName, string lastName)
    {
        if (string.IsNullOrWhiteSpace(firstName))
            throw new DomainException("First name cannot be empty.");
        
        if (string.IsNullOrWhiteSpace(lastName))
            throw new DomainException("Last name cannot be empty.");
        
        FirstName = firstName;
        LastName = lastName;
    }

    public void Deactivate()
    {
        Status = CustomerStatus.Inactive;
    }

    public void Activate()
    {
        Status = CustomerStatus.Active;
    }

    public void UpdateLastLogin()
    {
        LastLoginDate = DateTime.UtcNow;
    }

    private static void ValidateCustomerData(string firstName, string lastName, string email, string phoneNumber)
    {
        if (string.IsNullOrWhiteSpace(firstName))
            throw new DomainException("First name cannot be empty.");
        
        if (string.IsNullOrWhiteSpace(lastName))
            throw new DomainException("Last name cannot be empty.");
        
        if (string.IsNullOrWhiteSpace(email))
            throw new DomainException("Email cannot be empty.");
        
        if (string.IsNullOrWhiteSpace(phoneNumber))
            throw new DomainException("Phone number cannot be empty.");
    }
} 