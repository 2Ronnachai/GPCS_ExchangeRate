namespace GPCS_ExchangeRate.Application.Dtos.Documents.Responses
{
    public class UserDto
    {
        public string NId { get; set; } = string.Empty;
        public string FullName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Department { get; set; } = string.Empty;
        public string Grade { get; set; } = string.Empty;
        public List<string> Roles { get; set; } = new List<string>();
    }
}
