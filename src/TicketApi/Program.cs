
using Booking.Contracts;
using TicketApi.Hubs;
using Microsoft.AspNetCore.SignalR;

var builder = WebApplication.CreateBuilder(args);

// Đăng ký SignalR
builder.Services.AddSignalR();

var app = builder.Build();

// Cấu hình phục vụ giao diện Web
app.UseDefaultFiles();
app.UseStaticFiles();

// Đăng ký SignalR Hub
app.MapHub<BookingHub>("/hubs/bookings");

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
    }
};

// Lưu đơn đặt vé trong bộ nhớ
var bookings = new Dictionary<Guid, BookingInfo>();

// API lấy danh sách sự kiện
app.MapGet("/api/events", () =>
{
    return Results.Ok(events);
});

// API lấy chi tiết một sự kiện
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

// API tạo đơn đặt vé
app.MapPost("/api/bookings", async (
    BookingRequest bookingRequest,
    IHubContext<BookingHub> hubContext) =>
{
    // Kiểm tra UserId
    if (string.IsNullOrWhiteSpace(bookingRequest.UserId))
    {
        return Results.BadRequest(new
        {
            message = "UserId is required."
        });
    }

    // Kiểm tra số lượng vé
    if (bookingRequest.Quantity <= 0)
    {
        return Results.BadRequest(new
        {
            message = "Quantity must be greater than 0."
        });
    }

    // Kiểm tra sự kiện
    var ticketEvent = events.FirstOrDefault(
        e => e.Id == bookingRequest.EventId
    );

    if (ticketEvent == null)
    {
        return Results.NotFound(new
        {
            message = "Event not found."
        });
    }

    // Kiểm tra số lượng vé còn lại
    if (ticketEvent.AvailableTickets < bookingRequest.Quantity)
    {
        return Results.BadRequest(new
        {
            message = "Not enough tickets available."
        });
    }

    // Tạo đơn đặt vé
    var bookingInfo = new BookingInfo
    {
        BookingId = Guid.NewGuid(),
        EventId = bookingRequest.EventId,
        UserId = bookingRequest.UserId,
        Quantity = bookingRequest.Quantity,
        Status = "Pending"
    };

    // Lưu đơn đặt vé
    bookings[bookingInfo.BookingId] = bookingInfo;

    // Giả lập xử lý đơn đặt vé trong nền
    _ = Task.Run(async () =>
    {
        await Task.Delay(3000);

        bookingInfo.Status = "Confirmed";

        var message = new
        {
            BookingId = bookingInfo.BookingId,
            Status = bookingInfo.Status,
            UpdatedAt = DateTime.UtcNow
        };

        // Gửi thông báo qua SignalR
        await hubContext.Clients
            .Group($"booking:{bookingInfo.BookingId}")
            .SendAsync("BookingStatusChanged", message);
    });

    // Trả về kết quả tạo đơn
    return Results.Created(
        $"/api/bookings/{bookingInfo.BookingId}",
        bookingInfo
    );
});

// API lấy thông tin đơn đặt vé
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

// API lấy danh sách đơn đặt vé của người dùng
app.MapGet("/api/users/{userId}/bookings", (string userId) =>
{
    var userBookings = bookings.Values
        .Where(b => b.UserId == userId)
        .ToList();

    return Results.Ok(userBookings);
});

app.Run();
