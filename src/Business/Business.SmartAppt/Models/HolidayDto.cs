using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Business.SmartAppt.Models
{
    public class HolidayDto:BaseResponse
    {
        public int HolidayId { get; set; }
        public int BusinessId { get; set; }
        public DateTime HolidayDate { get; set; }
        public string? Reason { get; set; }

    }
}
