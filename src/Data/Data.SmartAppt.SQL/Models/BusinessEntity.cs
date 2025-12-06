namespace Data.SmartAppt.SQL.Models
{
    public  class BusinessEntity
    {
        public int BusinessId { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? Email { get; set; }
        public string? Phone { get; set; }
        public string TimeZone { get; set; } = "Asia/Yerevan";
        public string? SettingsJson { get; set; }
        public DateTime CreatedAtUtc { get; set; }
    }
}