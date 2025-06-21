using BookStore.Domain.Enums;
using BookStore.Domain.Exceptions;
using BookStore.Domain.ValueObjects;

namespace BookStore.Domain.Entities;

public class Order : BaseEntity
{
    public Guid CustomerId { get; private set; }
    public Customer Customer { get; private set; }
    public OrderStatus Status { get; private set; }
    public DateTime OrderDate { get; private set; }
    public DateTime? ShippedDate { get; private set; }
    public DateTime? DeliveredDate { get; private set; }
    public Address ShippingAddress { get; private set; }
    public PaymentMethod PaymentMethod { get; private set; }
    public decimal Subtotal { get; private set; }
    public decimal Tax { get; private set; }
    public decimal ShippingCost { get; private set; }
    public decimal Total { get; private set; }
    public string? Notes { get; private set; }

    // Navigation properties
    public ICollection<OrderItem> OrderItems { get; private set; } = new List<OrderItem>();

    private Order() { } // For EF Core

    public Order(Guid customerId, Address shippingAddress, PaymentMethod paymentMethod, string? notes = null)
    {
        if (customerId == Guid.Empty)
            throw new DomainException("Customer ID cannot be empty.");
        
        CustomerId = customerId;
        ShippingAddress = shippingAddress ?? throw new DomainException("Shipping address cannot be null.");
        PaymentMethod = paymentMethod;
        Status = OrderStatus.Pending;
        OrderDate = DateTime.UtcNow;
        Notes = notes;
        
        CalculateTotals();
    }

    public void AddOrderItem(Book book, int quantity)
    {
        if (book == null)
            throw new DomainException("Book cannot be null.");
        
        if (quantity <= 0)
            throw new DomainException("Quantity must be positive.");
        
        if (Status != OrderStatus.Pending)
            throw new DomainException("Cannot add items to an order that is not pending.");
        
        var existingItem = OrderItems.FirstOrDefault(oi => oi.BookId == book.Id);
        
        if (existingItem != null)
        {
            existingItem.UpdateQuantity(existingItem.Quantity + quantity);
        }
        else
        {
            var orderItem = new OrderItem(Id, book.Id, book.Price, quantity);
            OrderItems.Add(orderItem);
        }
        
        CalculateTotals();
    }

    public void RemoveOrderItem(Guid bookId)
    {
        if (Status != OrderStatus.Pending)
            throw new DomainException("Cannot remove items from an order that is not pending.");
        
        var orderItem = OrderItems.FirstOrDefault(oi => oi.BookId == bookId);
        if (orderItem != null)
        {
            OrderItems.Remove(orderItem);
            CalculateTotals();
        }
    }

    public void UpdateOrderItemQuantity(Guid bookId, int newQuantity)
    {
        if (Status != OrderStatus.Pending)
            throw new DomainException("Cannot update items in an order that is not pending.");
        
        var orderItem = OrderItems.FirstOrDefault(oi => oi.BookId == bookId);
        if (orderItem != null)
        {
            orderItem.UpdateQuantity(newQuantity);
            CalculateTotals();
        }
    }

    public void ConfirmOrder()
    {
        if (Status != OrderStatus.Pending)
            throw new DomainException("Only pending orders can be confirmed.");
        
        if (!OrderItems.Any())
            throw new DomainException("Cannot confirm an order without items.");
        
        Status = OrderStatus.Confirmed;
    }

    public void ShipOrder()
    {
        if (Status != OrderStatus.Confirmed)
            throw new DomainException("Only confirmed orders can be shipped.");
        
        Status = OrderStatus.Shipped;
        ShippedDate = DateTime.UtcNow;
    }

    public void DeliverOrder()
    {
        if (Status != OrderStatus.Shipped)
            throw new DomainException("Only shipped orders can be delivered.");
        
        Status = OrderStatus.Delivered;
        DeliveredDate = DateTime.UtcNow;
    }

    public void CancelOrder()
    {
        if (Status == OrderStatus.Delivered)
            throw new DomainException("Cannot cancel a delivered order.");
        
        Status = OrderStatus.Cancelled;
    }

    public void UpdateShippingAddress(Address newAddress)
    {
        if (Status != OrderStatus.Pending)
            throw new DomainException("Cannot update shipping address for non-pending orders.");
        
        ShippingAddress = newAddress ?? throw new DomainException("Shipping address cannot be null.");
    }

    public void UpdateNotes(string notes)
    {
        Notes = notes;
    }

    private void CalculateTotals()
    {
        Subtotal = OrderItems.Sum(oi => oi.TotalPrice);
        Tax = Subtotal * 0.08m; // 8% tax rate
        ShippingCost = Subtotal > 50 ? 0 : 5.99m; // Free shipping over $50
        Total = Subtotal + Tax + ShippingCost;
    }
} 