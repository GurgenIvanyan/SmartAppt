using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Business.SmartAppt.Models
{
    public class CreateOpeningHoursDto
    {
        public int BusinessId { get; set; }
        public byte DayOfWeek { get; set; }
        public TimeSpan OpenTime { get; set; }
        public TimeSpan CloseTime { get; set; }
    }
}
