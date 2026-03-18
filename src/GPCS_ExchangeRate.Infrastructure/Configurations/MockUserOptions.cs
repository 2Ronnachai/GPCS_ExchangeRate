namespace GPCS_ExchangeRate.Infrastructure.Configurations
{
    public class MockUserOptions
    {
        public const string SectionName = "MockUsers";

        public bool Enabled { get; set; }
        public List<MockUserDto> Users { get; set; } = [];
    }

    public class MockUserDto
    {
        public string NId { get; set; } = string.Empty;
        public string FullName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Department { get; set; } = string.Empty;
        public string Grade { get; set; } = string.Empty;
        public List<string> Roles { get; set; } = [];
    }
}
