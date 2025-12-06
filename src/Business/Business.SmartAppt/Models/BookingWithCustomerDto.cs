using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Business.SmartAppt.Models
{
    public class BookingWithCustomerDto
    {
        public int BookingId { get; set; }
        public int BusinessId { get; set; }
        public int ServiceId { get; set; }
        public DateTime StartAtUtc { get; set; }
        public DateTime EndAtUtc { get; set; }
        public string Status { get; set; } = null!;
        public string? Notes { get; set; }

        public CustomerShortDto Customer { get; set; } = new CustomerShortDto();
    }
}
