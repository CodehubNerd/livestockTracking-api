namespace livestock_api.Models
{
    public class ConfirmRegisterResponse
    {
        public string? EmailAddress { get; set; }
        public object? UserId { get; set; }
        public string? Message { get; set; }
        public bool IsSuccess { get; set; }
    }
}
