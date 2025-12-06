using Data.SmartAppt.SQL.Models;

namespace Business.SmartAppt.Models
{
    public class BookingListDto : BaseResponse
    {
        public IEnumerable<BookingEntity>? Bookings { get; set; }
    }
}