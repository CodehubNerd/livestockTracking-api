namespace livestock_api.Models
{
    public class RegistrationReponse
    {
        public bool IsSuccess { get; set; }
        public string? Message { get; set; }
        public object? EmailAddress { get; set; }
        public object? UserId { get; set; }
        public bool Succeeded { get; set; }
    }
}
