namespace Business.SmartAppt.Models
{
    public class DayAvailability
    {
        public int Day { get; set; }
        public bool IsOpen { get; set; }
        public bool HasFreeSlots { get; set; }

    }
}