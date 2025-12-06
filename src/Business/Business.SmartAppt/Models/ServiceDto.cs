using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Business.SmartAppt.Models
{
    public class ServiceDto : BaseResponse
    {
        public int ServiceId { get; set; }
        public int BusinessId { get; set; }
        public string Name { get; set; } = null!;
        public int DurationMin { get; set; }
        public Decimal Price { get; set; }
        public bool IsActive { get; set; }
    }
}
