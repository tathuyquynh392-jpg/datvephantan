using System;
using System.Collections.Generic;
using System.Text;

namespace Booking.Contracts
{
    public class TicketEvent
    {
        //Mã sự kiện
        public Guid Id { get; set; }
        //Tên sự kiện
        public string Name { get; set; } = string.Empty;
        //địa điểm tổ chức sự kiện
        public string Location { get; set; } = string.Empty;
        //ngày tổ chức sự kiện
        public DateTime EventDate { get; set; }
        //giá vé của sự kiện
        public decimal Price { get; set; }
        //số lượng vé còn lại của sự kiện
        public int AvailableTickets { get; set; }
    }
}
