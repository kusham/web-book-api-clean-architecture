using BookStore.Domain.Exceptions;

namespace BookStore.Domain.Entities;

public class OrderItem : BaseEntity
{
    public Guid OrderId { get; private set; }
    public Guid BookId { get; private set; }
    public Book Book { get; private set; }
    public decimal UnitPrice { get; private set; }
    public int Quantity { get; private set; }
    public decimal TotalPrice { get; private set; }

    private OrderItem() { } // For EF Core

    public OrderItem(Guid orderId, Guid bookId, decimal unitPrice, int quantity)
    {
        if (orderId == Guid.Empty)
            throw new DomainException("Order ID cannot be empty.");
        
        if (bookId == Guid.Empty)
            throw new DomainException("Book ID cannot be empty.");
        
        if (unitPrice < 0)
            throw new DomainException("Unit price cannot be negative.");
        
        if (quantity <= 0)
            throw new DomainException("Quantity must be positive.");
        
        OrderId = orderId;
        BookId = bookId;
        UnitPrice = unitPrice;
        Quantity = quantity;
        CalculateTotalPrice();
    }

    public void UpdateQuantity(int newQuantity)
    {
        if (newQuantity <= 0)
            throw new DomainException("Quantity must be positive.");
        
        Quantity = newQuantity;
        CalculateTotalPrice();
    }

    public void UpdateUnitPrice(decimal newUnitPrice)
    {
        if (newUnitPrice < 0)
            throw new DomainException("Unit price cannot be negative.");
        
        UnitPrice = newUnitPrice;
        CalculateTotalPrice();
    }

    private void CalculateTotalPrice()
    {
        TotalPrice = UnitPrice * Quantity;
    }
} 