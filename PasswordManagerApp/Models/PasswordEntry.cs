namespace PasswordManagerApp.Models
{
    public class PasswordEntry
    {
        public int Id { get; set; }
        public string? ProjectName { get; set; }
        public string? Address { get; set; }
        public string? Account { get; set; }
        public string? Password { get; set; }
        public string? Notes { get; set; }
        public string? ExtraCol1 { get; set; }
        public string? ExtraCol2 { get; set; }
        public string? ExtraCol3 { get; set; }
        public string? Category { get; set; } // e.g., "密码库", "自定义库"
    }
}
