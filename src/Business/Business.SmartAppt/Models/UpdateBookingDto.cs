namespace Business.SmartAppt.Models
{
    public class UpdateBookingDto
    {
        public DateTime StartAtUtc { get; set; }
        public string? Notes { get; set; }

    }
}