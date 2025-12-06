namespace Data.SmartAppt.SQL.Models
{
    public class BookingFilter
    {
        public int? BusinessId { get; set; }
        public int? CustomerId { get; set; }
        public int? ServiceId { get; set; }
        public string? Status { get; set; }
        public DateTime? Date { get; set; }
        public int? Skip { get; set; }
        public int? Take { get; set; }

    }
}  