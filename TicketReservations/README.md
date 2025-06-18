# Concert Ticket Reservation Management System

A comprehensive .NET 9 Web API application for managing concert events, venues, ticket types, reservations, and sales with Entity Framework Core and an in-memory database.

## Core Features

- **Event Management**: Create and manage concert events with multiple ticket types and pricing
- **Venue Capacity Management**: Manage venues with detailed capacity information and constraints
- **Real-Time Ticket Reservations**: Time-limited reservations with automatic expiration and cleanup
- **Instant Ticket Sales**: Purchase tickets directly or convert reservations to completed sales
- **Real-Time Availability Tracking**: Accurate, real-time calculation of available, reserved, and sold tickets
- **Multiple Ticket Categories**: Support for General, VIP, Premium, Student, and other ticket categories
- **Automatic Cleanup**: Background service automatically expires and releases old reservations
- **Overbooking Prevention**: Robust business logic prevents double-booking scenarios
- **API Versioning**: Multiple versioning strategies (URL path, query string, headers)
- **In-Memory Database**: Uses Entity Framework Core with an in-memory database for development
- **RESTful API**: Full REST API with proper HTTP status codes and error handling
- **Swagger Documentation**: Interactive API documentation available at the root URL
- **CORS Support**: Configured for cross-origin requests

## Technologies Used

- .NET 9
- ASP.NET Core Web API
- Entity Framework Core
- Entity Framework In-Memory Database
- Background Services (Hosted Services)
- Swagger/OpenAPI
- C# 13
- LINQ for data querying
- JSON serialization

## Getting Started

### Prerequisites

- .NET 9 SDK

### Running the Application

1. Navigate to the project directory:

   ```bash
   cd TicketReservations
   ```

2. Build the project:

   ```bash
   dotnet build
   ```

3. Run the application:

   ```bash
   dotnet run
   ```

4. Open your browser and navigate to:

   - **HTTP**: `http://localhost:5094`
   - **HTTPS**: `https://localhost:7254` (if HTTPS is configured)

   To access the Swagger UI for interactive API testing.

## API Endpoints

### Events

- `GET /api/v1/events` - Get all events (with optional status and onSale filters)
- `GET /api/v1/events/{id}` - Get a specific event with full details including ticket types
- `POST /api/v1/events` - Create a new event
- `PUT /api/v1/events/{id}` - Update an existing event
- `DELETE /api/v1/events/{id}` - Delete an event

### Venues

- `GET /api/v1/venues` - Get all venues
- `GET /api/v1/venues/{id}` - Get a specific venue by ID
- `POST /api/v1/venues` - Create a new venue
- `PUT /api/v1/venues/{id}` - Update an existing venue
- `DELETE /api/v1/venues/{id}` - Delete a venue

### Ticket Types

- `GET /api/v1/events/{eventId}/ticket-types` - Get all ticket types for an event with real-time availability
- `GET /api/v1/events/{eventId}/ticket-types?availableOnly=true` - Get only currently available ticket types
- `GET /api/v1/events/{eventId}/ticket-types/{id}` - Get a specific ticket type with availability details
- `POST /api/v1/events/{eventId}/ticket-types` - Create a new ticket type
- `PUT /api/v1/events/{eventId}/ticket-types/{id}` - Update a ticket type
- `DELETE /api/v1/events/{eventId}/ticket-types/{id}` - Delete a ticket type

### Reservations

- `POST /api/events/{eventId}/reservations` - Create a time-limited reservation
- `GET /api/events/{eventId}/reservations/{reservationCode}` - Get reservation details by code
- `GET /api/events/{eventId}/reservations/{reservationCode}/status` - Check reservation status and validity
- `POST /api/events/{eventId}/reservations/{reservationCode}/purchase` - Convert reservation to completed sale
- `POST /api/events/{eventId}/reservations/{reservationCode}/cancel` - Cancel a reservation
- Auto-expiration of reservations after timeout (configurable timeout)

### Sales

- Sales are automatically completed when purchasing from reservations
- No pending sales state - purchases complete immediately
- Includes automatic calculation of service fees (5%) and taxes (8%)

### Health Check

- `GET /health` - Application health check with timestamp

## Reservation System Business Logic

### Reservation Workflow

1. **Create Reservation**: Customer reserves tickets for a specified time period (configurable timeout)
2. **Automatic Hold**: Tickets are immediately held and not available to other customers
3. **Purchase or Expire**: Customer can purchase within the time limit, or reservation automatically expires
4. **Real-Time Availability**: System shows accurate counts at all times

### Key Features

- **Real-Time Availability Calculation**: Uses actual reservations and sales data, not cached counters
- **Automatic Expiration**: Background service releases expired reservations every minute
- **Overbooking Prevention**: Robust validation prevents double-booking scenarios
- **Flexible Timeouts**: Configurable reservation timeout (default 15 minutes)
- **Unique Reservation Codes**: Each reservation gets a unique code (e.g., "RES1A2B3C4D")

## Configuration

### Reservation Settings

```json
{
  "ReservationSettings": {
    "DefaultTimeoutMinutes": 15
  }
}
```

## Architecture

The application follows clean architecture principles:

- **Controllers**: Thin HTTP handlers with proper error handling
- **Core/Services**: Business logic interfaces (contracts)
- **Infrastructure/Services**: Business logic implementations with data access
- **Models**: Domain entities with proper validation
- **DTOs**: Data transfer objects for API communication
- **Background Services**: Automatic cleanup and maintenance
- **Middleware**: Correlation ID tracking for requests

## API Examples

### Create a Reservation

```bash
curl -X POST "http://localhost:5094/api/events/1/reservations" \
  -H "Content-Type: application/json" \
  -d '{
    "ticketTypeId": 1,
    "customerName": "John Doe",
    "customerEmail": "john.doe@example.com",
    "customerPhone": "+1-555-123-4567",
    "quantity": 2
  }'
```

**Response:**

```json
{
  "id": 1,
  "reservationCode": "RES1A2B3C4D",
  "quantity": 2,
  "totalAmount": 179.98,
  "status": 0,
  "expirationDate": "2024-01-01T12:15:00Z",
  "isExpired": false
}
```

### Purchase from Reservation

```bash
curl -X POST "http://localhost:5094/api/events/1/reservations/RES1A2B3C4D/purchase" \
  -H "Content-Type: application/json" \
  -d '{
    "paymentMethod": "Credit Card",
    "notes": "Purchase via website"
  }'
```

**Response:**

```json
{
  "id": 1,
  "confirmationCode": "CONF5E6F7G8H",
  "quantity": 2,
  "totalAmount": 179.98,
  "serviceFee": 8.99,
  "tax": 15.12,
  "finalAmount": 204.09,
  "status": 2,
  "paymentDate": "2024-01-01T12:10:00Z"
}
```

### Check Real-Time Availability

```bash
curl -X GET "http://localhost:5094/api/v1/events/1/ticket-types/1"
```

**Response:**

```json
{
  "id": 1,
  "name": "General Admission",
  "price": 89.99,
  "totalQuantity": 1000,
  "availableQuantity": 850,
  "reservedQuantity": 100,
  "soldQuantity": 50,
  "isAvailable": true,
  "unavailableReason": null
}
```

## Potential improvements for the system:

- Payment gateway integration
- Email notifications for reservations and purchases
- Customer accounts and purchase history
- Seat selection for venues with specific seating
- Bulk reservation handling
- Administrative dashboard
- Reporting and analytics
- Rate limiting and security enhancements
