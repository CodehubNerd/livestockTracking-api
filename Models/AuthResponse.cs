namespace livestock_api.Models
{
    public class AuthResponse
    {
        public int UserId { get; set; }
        public string? UserName { get; set; }
        public string? EmailAddress { get; set; }
        public string? Tokens { get; set; }
        public string? Message { get; set; }
        public bool IsSuccess { get; set; }
    }
}
