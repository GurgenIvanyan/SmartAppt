namespace Business.SmartAppt.Models
{
    public class CalendarDto : ResponseBase
    {
        public int Month { get; set; }
        public int Year { get; set; }
        public List<DayAvailability> Days { get; set; } = new List<DayAvailability>();
    }
}