using BookStore.Domain.Exceptions;

namespace BookStore.Domain.ValueObjects;

public class Address
{
    public string Street { get; }
    public string City { get; }
    public string State { get; }
    public string ZipCode { get; }
    public string Country { get; }

    public Address(string street, string city, string state, string zipCode, string country)
    {
        ValidateAddress(street, city, state, zipCode, country);
        
        Street = street;
        City = city;
        State = state;
        ZipCode = zipCode;
        Country = country;
    }

    private static void ValidateAddress(string street, string city, string state, string zipCode, string country)
    {
        if (string.IsNullOrWhiteSpace(street))
            throw new DomainException("Street cannot be empty.");
        
        if (string.IsNullOrWhiteSpace(city))
            throw new DomainException("City cannot be empty.");
        
        if (string.IsNullOrWhiteSpace(state))
            throw new DomainException("State cannot be empty.");
        
        if (string.IsNullOrWhiteSpace(zipCode))
            throw new DomainException("Zip code cannot be empty.");
        
        if (string.IsNullOrWhiteSpace(country))
            throw new DomainException("Country cannot be empty.");
    }

    public string FullAddress => $"{Street}, {City}, {State} {ZipCode}, {Country}";

    public override string ToString()
    {
        return FullAddress;
    }

    public override bool Equals(object? obj)
    {
        if (obj is null) return false;
        if (obj is not Address other) return false;
        if (ReferenceEquals(this, other)) return true;
        
        return Street == other.Street &&
               City == other.City &&
               State == other.State &&
               ZipCode == other.ZipCode &&
               Country == other.Country;
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(Street, City, State, ZipCode, Country);
    }

    public static bool operator ==(Address? left, Address? right)
    {
        return Equals(left, right);
    }

    public static bool operator !=(Address? left, Address? right)
    {
        return !Equals(left, right);
    }
} 