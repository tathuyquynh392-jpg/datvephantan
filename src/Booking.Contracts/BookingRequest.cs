using System;
using System.Collections.Generic;
using System.Text;

namespace Booking.Contracts
{
    public class BookingRequest
    {
        public Guid EventId { get; set; }

        public string UserId { get; set; } = string.Empty;

        public int Quantity { get; set; }
    }
}
