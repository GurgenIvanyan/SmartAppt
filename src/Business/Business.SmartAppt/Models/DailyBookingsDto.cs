using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Business.SmartAppt.Models
{
    public class DailyBookingsDto : BaseResponse
    {
        public DateOnly Date { get; set; }
        public List<BookingWithCustomerDto> Bookings { get; set; } = new List<BookingWithCustomerDto>();
    }
}
