using Data.SmartAppt.SQL.Models;

namespace Business.SmartAppt.Models
{
    public class BookingListDto : ResponseBase
    {
        public IEnumerable<BookingEntity>? Bookings { get; set; }
    }
}