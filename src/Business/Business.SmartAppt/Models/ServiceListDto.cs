using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Data.SmartAppt.SQL.Models;

namespace Business.SmartAppt.Models
{
    public class ServiceListDto : BaseResponse
    {
        public IEnumerable<ServiceEntity>? Services { get; set; }
    }
}
