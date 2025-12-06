namespace Business.SmartAppt.Models
{
    public class DailySlotsDto : BaseResponse
    {
        public DateOnly Date { get; set; }
        public List<TimeSpan> FreeSlots { get; set; } = new List<TimeSpan>();
    }
}