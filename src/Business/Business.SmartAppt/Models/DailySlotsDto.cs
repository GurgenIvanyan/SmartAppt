namespace Business.SmartAppt.Models
{
    public class DailySlotsDto : ResponseBase
    {
        public DateOnly Date { get; set; }
        public List<TimeSpan> FreeSlots { get; set; } = new List<TimeSpan>();
    }
}