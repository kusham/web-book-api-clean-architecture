# BookStore Clean Architecture Project

A comprehensive .NET 8 Web API project demonstrating Clean Architecture principles with Entity Framework Core, CQRS pattern using MediatR, Unit of Work pattern, JWT authentication, and comprehensive testing.

## 🏗️ Architecture Overview

This project follows Clean Architecture principles with the following layers:

- **Domain Layer**: Core business entities, value objects, enums, and domain exceptions
- **Application Layer**: Use cases, DTOs, interfaces, and business logic
- **Infrastructure Layer**: Data access, external services, and implementations
- **API Layer**: Controllers, authentication, and API endpoints
- **Tests**: Unit tests, integration tests, and API tests

## 🚀 Features

- **Clean Architecture**: Proper separation of concerns with dependency inversion
- **CQRS Pattern**: Command Query Responsibility Segregation using MediatR
- **Unit of Work Pattern**: Transaction management and data consistency
- **Entity Framework Core**: ORM with SQL Server support
- **JWT Authentication**: Secure API endpoints with role-based authorization
- **AutoMapper**: Object-to-object mapping
- **FluentValidation**: Input validation
- **Swagger/OpenAPI**: API documentation
- **Comprehensive Testing**: Unit tests, integration tests, and API tests
- **Repository Pattern**: Data access abstraction

## 🔄 CQRS + Unit of Work Combination

This project demonstrates how to effectively combine CQRS with MediatR and Unit of Work patterns:

### **Commands (Write Operations) with Transaction Management**

```csharp
// CreateOrderCommand - Complex business operation with transaction
public class CreateOrderCommandHandler : IRequestHandler<CreateOrderCommand, OrderDto>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public async Task<OrderDto> Handle(CreateOrderCommand request, CancellationToken cancellationToken)
    {
        // Start transaction for complex business operation
        await _unitOfWork.BeginTransactionAsync();

        try
        {
            // Validate customer exists
            var customer = await _unitOfWork.Customers.GetByIdAsync(request.CustomerId);
            if (customer == null)
                throw new InvalidOperationException($"Customer with ID {request.CustomerId} not found");

            // Create order
            var order = new Order(request.CustomerId, shippingAddress, request.PaymentMethod);

            // Process each order item
            foreach (var itemDto in request.OrderItems)
            {
                var book = await _unitOfWork.Books.GetByIdAsync(itemDto.BookId);
                if (book.StockQuantity < itemDto.Quantity)
                    throw new InvalidOperationException($"Insufficient stock for book '{book.Title}'");

                // Add item to order (reserves stock)
                order.AddOrderItem(book, itemDto.Quantity);
                await _unitOfWork.Books.UpdateAsync(book);
            }

            // Save order
            var createdOrder = await _unitOfWork.Orders.AddAsync(order);
            await _unitOfWork.SaveChangesAsync();

            // Commit transaction
            await _unitOfWork.CommitTransactionAsync();

            return _mapper.Map<OrderDto>(createdOrder);
        }
        catch
        {
            // Rollback transaction on any error
            await _unitOfWork.RollbackTransactionAsync();
            throw;
        }
    }
}
```

### **Queries (Read Operations) - Simple Data Retrieval**

```csharp
// GetAllBooksQuery - Simple read operation
public class GetAllBooksQueryHandler : IRequestHandler<GetAllBooksQuery, IEnumerable<BookDto>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public async Task<IEnumerable<BookDto>> Handle(GetAllBooksQuery request, CancellationToken cancellationToken)
    {
        var books = await _unitOfWork.Books.GetAllAsync();
        return _mapper.Map<IEnumerable<BookDto>>(books);
    }
}
```

### **Controller Using MediatR**

```csharp
[ApiController]
[Route("api/[controller]")]
[Authorize]
public class OrdersController : ControllerBase
{
    private readonly IMediator _mediator;

    public OrdersController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateOrderCommand command)
    {
        var order = await _mediator.Send(command);
        return CreatedAtAction(nameof(GetById), new { id = order.Id }, order);
    }

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var orders = await _mediator.Send(new GetAllOrdersQuery());
        return Ok(orders);
    }
}
```

## 📁 Project Structure

```
MyWebApi/
├── src/
│   ├── BookStore.Domain/           # Domain Layer
│   │   ├── Entities/               # Domain entities
│   │   ├── Enums/                  # Domain enums
│   │   ├── Exceptions/             # Domain exceptions
│   │   └── ValueObjects/           # Value objects
│   ├── BookStore.Application/      # Application Layer
│   │   ├── DTOs/                   # Data Transfer Objects
│   │   ├── Features/               # CQRS features
│   │   │   ├── Books/
│   │   │   │   ├── Commands/       # Write operations
│   │   │   │   └── Queries/        # Read operations
│   │   │   ├── Customers/
│   │   │   └── Orders/
│   │   ├── Interfaces/             # Repository interfaces
│   │   └── Mappings/               # AutoMapper profiles
│   ├── BookStore.Infrastructure/   # Infrastructure Layer
│   │   ├── Data/                   # EF Core context
│   │   └── Repositories/           # Repository implementations
│   └── BookStore.API/              # API Layer
│       ├── Controllers/            # API controllers
│       └── Services/               # JWT service
└── tests/
    └── BookStore.Tests/            # Test projects
        ├── Domain/                 # Domain tests
        ├── Application/            # Application tests
        └── API/                    # API tests
```

## 🛠️ Technologies Used

- **.NET 8**
- **ASP.NET Core Web API**
- **Entity Framework Core 8.0**
- **SQL Server** (with LocalDB for development)
- **MediatR** (CQRS implementation)
- **AutoMapper** (Object mapping)
- **FluentValidation** (Input validation)
- **JWT Bearer Authentication**
- **Swagger/OpenAPI**
- **xUnit** (Testing framework)
- **Moq** (Mocking framework)
- **FluentAssertions** (Assertion library)

## 🚀 Getting Started

### Prerequisites

- .NET 8 SDK
- SQL Server or SQL Server LocalDB
- Visual Studio 2022 or VS Code

### Installation

1. **Clone the repository**
   ```bash
   git clone <repository-url>
   cd MyWebApi
   ```

2. **Restore dependencies**
   ```bash
   dotnet restore
   ```

3. **Update connection string** (if needed)
   Edit `src/BookStore.API/appsettings.json` and update the connection string:
   ```json
   {
     "ConnectionStrings": {
       "DefaultConnection": "Server=(localdb)\\mssqllocaldb;Database=BookStoreDb;Trusted_Connection=True;"
     }
   }
   ```

4. **Create and apply database migrations**
   ```bash
   cd src/BookStore.API
   dotnet ef migrations add InitialCreate
   dotnet ef database update
   ```

5. **Run the application**
   ```bash
   dotnet run
   ```

6. **Access the API**
   - API: https://localhost:7001
   - Swagger UI: https://localhost:7001/swagger

### Authentication

The API uses JWT authentication. To get a token:

1. **Login** (POST `/api/auth/login`)
   ```json
   {
     "email": "admin@bookstore.com",
     "password": "admin123"
   }
   ```

2. **Use the token** in the Authorization header:
   ```
   Authorization: Bearer <your-token>
   ```

## 📚 API Endpoints

### Books
- `GET /api/books` - Get all books
- `GET /api/books/{id}` - Get book by ID
- `POST /api/books` - Create new book (Admin only)
- `PUT /api/books/{id}` - Update book (Admin only)
- `DELETE /api/books/{id}` - Delete book (Admin only)

### Customers
- `GET /api/customers` - Get all customers
- `GET /api/customers/{id}` - Get customer by ID
- `POST /api/customers` - Create new customer
- `PUT /api/customers/{id}` - Update customer
- `DELETE /api/customers/{id}` - Delete customer

### Orders
- `GET /api/orders` - Get all orders
- `GET /api/orders/{id}` - Get order by ID
- `POST /api/orders` - Create new order
- `PUT /api/orders/{id}/status` - Update order status (Admin only)
- `PUT /api/orders/{id}` - Update order
- `DELETE /api/orders/{id}` - Delete order (Admin only)

### Authentication
- `POST /api/auth/login` - Login and get JWT token
- `GET /api/auth/me` - Get current user info (Authenticated)

## 🧪 Running Tests

```bash
# Run all tests
dotnet test

# Run tests with coverage
dotnet test --collect:"XPlat Code Coverage"

# Run specific test project
dotnet test tests/BookStore.Tests/
```

## 🔧 Configuration

### JWT Settings
```json
{
  "JwtSettings": {
    "SecretKey": "your-super-secret-key-with-at-least-32-characters",
    "Issuer": "BookStoreAPI",
    "Audience": "BookStoreUsers",
    "ExpirationInMinutes": 60
  }
}
```

### Database
The project uses SQL Server LocalDB by default. You can change the connection string in `appsettings.json` to use:
- SQL Server
- SQLite
- InMemory (for testing)

## 🏗️ Clean Architecture Principles

### Dependency Rule
- Domain layer has no dependencies
- Application layer depends only on Domain
- Infrastructure layer depends on Application and Domain
- API layer depends on all other layers

### Separation of Concerns
- **Domain**: Business entities and logic
- **Application**: Use cases and business rules
- **Infrastructure**: External concerns (database, external APIs)
- **API**: Presentation and HTTP concerns

### SOLID Principles
- **Single Responsibility**: Each class has one reason to change
- **Open/Closed**: Open for extension, closed for modification
- **Liskov Substitution**: Derived classes can substitute base classes
- **Interface Segregation**: Clients depend only on interfaces they use
- **Dependency Inversion**: High-level modules don't depend on low-level modules

## 🔄 Pattern Benefits

### **CQRS + Unit of Work Benefits:**
- ✅ **Transaction Safety**: Complex operations are atomic
- ✅ **Separation of Concerns**: Read and write operations are separate
- ✅ **Testability**: Easy to mock and test individual components
- ✅ **Scalability**: Can optimize reads and writes independently
- ✅ **Maintainability**: Clear structure and responsibilities
- ✅ **Error Handling**: Proper rollback on failures

### **When to Use Each Pattern:**
- **Use Unit of Work**: For transaction management and data consistency
- **Use CQRS**: For separating read/write operations and business logic
- **Use Both**: For enterprise applications requiring both transaction safety and clean architecture

## 🤝 Contributing

1. Fork the repository
2. Create a feature branch
3. Make your changes
4. Add tests for new functionality
5. Ensure all tests pass
6. Submit a pull request

## 📄 License

This project is licensed under the MIT License.

## 🆘 Support

For support and questions, please open an issue in the repository. 

    dotnet user-secrets init --project src/BookStore.API/BookStore.API.csproj
    dotnet user-secrets set "JwtSettings:SecretKey" "key" --project src/BookStore.API/BookStore.API.csproj