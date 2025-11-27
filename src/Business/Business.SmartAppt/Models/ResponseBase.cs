namespace Business.SmartAppt.Models
{
    public class ResponseBase
    {
        public int StatusCode { get; set; }
        public string? ErrorMessage { get; set; }

    }
}