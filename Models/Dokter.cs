namespace Klinik_PAA.Models
{
    public class Dokter
    {
        public int Id { get; set; }
        public string Nama { get; set; } = string.Empty;
        public string Spesialisasi { get; set; } = string.Empty;
        public string NoTelepon { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        public DateTime? DeletedAt { get; set; }
    }

    public class DokterRequest
    {
        public string Nama { get; set; } = string.Empty;
        public string Spesialisasi { get; set; } = string.Empty;
        public string NoTelepon { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
    }
}