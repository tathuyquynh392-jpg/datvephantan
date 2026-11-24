using Booking.Contracts;

var builder = WebApplication.CreateBuilder(args);

var app = builder.Build();
app.UseDefaultFiles();
app.UseStaticFiles();
var bookings = new Dictionary<Guid, BookingInfo>();
// Dữ liệu sự kiện giả lập
var events = new List<TicketEvent>
{
    new TicketEvent
    {
        Id = Guid.NewGuid(),
        Name = "Festival âm nhạc mùa hè",
        Location = "Công viên Yên Sở, Hà Nội",
        EventDate = new DateTime(2027, 1, 10, 16, 0, 0),
        Price = 1200000m,
        AvailableTickets = 2000
    },

    new TicketEvent
    {
        Id = Guid.NewGuid(),
        Name = "Hội chợ công nghệ Tech Expo",
        Location = "Cung triển lãm Kiến trúc, Hà Nội",
        EventDate = new DateTime(2027, 1, 15, 9, 0, 0),
        Price = 200000m,
        AvailableTickets = 300
    },
};
// API lấy danh sách sự kiện
app.MapGet("/api/events", () =>
{
    return Results.Ok(events);
});
// API lấy thông tin chi tiết của một sự kiện theo ID
app.MapGet("/api/events/{id:guid}", (Guid id) =>
{
    var ticketEvent = events.FirstOrDefault(e => e.Id == id);
    if (ticketEvent == null)
    {
        return Results.NotFound(new
        {
            message = $"Event with ID {id} not found."
        });
    }
    return Results.Ok(ticketEvent);
});
// API đặt vé cho một sự kiện
app.MapPost("/api/bookings", (BookingRequest bookingRequest) =>
{
    // Kiểm tra UserId
    if (string.IsNullOrWhiteSpace(bookingRequest.UserId))
    {
        return Results.BadRequest(new
        {
            message = "UserId must not be empty."
        });
    }

    // Kiểm tra số lượng vé
    if (bookingRequest.Quantity <= 0)
    {
        return Results.BadRequest(new
        {
            message = "Quantity must be greater than zero."
        });
    }

    // Kiểm tra sự kiện có tồn tại
    var ticketEvent = events.FirstOrDefault(
        e => e.Id == bookingRequest.EventId);

    if (ticketEvent == null)
    {
        return Results.NotFound(new
        {
            message = $"Event with ID {bookingRequest.EventId} not found."
        });
    }

    // Kiểm tra sơ bộ số vé còn lại
    if (bookingRequest.Quantity > ticketEvent.AvailableTickets)
    {
        return Results.BadRequest(new
        {
            message = $"Not enough tickets available. Only {ticketEvent.AvailableTickets} tickets left."
        });
    }

    // Tạo booking
    var bookingInfo = new BookingInfo
    {
        BookingId = Guid.NewGuid(),
        EventId = bookingRequest.EventId,
        UserId = bookingRequest.UserId,
        Quantity = bookingRequest.Quantity,
        Status = "Pending"
    };

    // Lưu booking
    bookings[bookingInfo.BookingId] = bookingInfo;

    // Trả HTTP 201 Created
    return Results.Created(
        $"/api/bookings/{bookingInfo.BookingId}",
        bookingInfo
    );
});
// API tra cứu booking theo ID
app.MapGet("/api/bookings/{id:guid}", (Guid id) =>
{
    if (!bookings.TryGetValue(id, out var bookingInfo))
    {
        return Results.NotFound(new
        {
            message = $"Booking with ID {id} not found."
        });
    }

    return Results.Ok(bookingInfo);
});
// API lấy danh sách booking theo UserId
app.MapGet("/api/users/{userId}/bookings", (string userId) =>
{
    var userBookings = bookings.Values
        .Where(b => b.UserId == userId)
        .ToList();

    return Results.Ok(userBookings);
});

app.Run();