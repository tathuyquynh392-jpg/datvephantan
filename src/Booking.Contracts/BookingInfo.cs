namespace Booking.Contracts;

public class BookingInfo
{
    //yêu cầu đặt vé
    public Guid BookingId { get; set; }
    //sự kiện mà người dùng muốn đặt vé
    public Guid EventId { get; set; }
    //người dùng muốn đặt vé
    public string UserId { get; set; } = string.Empty;
    //số lượng vé mà người dùng muốn đặt
    public int Quantity { get; set; }
    //trạng thái của yêu cầu đặt vé
    public string Status { get; set; } = "Pending";
}
