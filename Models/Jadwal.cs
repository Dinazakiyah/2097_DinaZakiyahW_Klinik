namespace Klinik_PAA.Models
{
    public class Jadwal
    {
        public int Id { get; set; }
        public int PasienId { get; set; }
        public int DokterId { get; set; }
        public DateOnly Tanggal { get; set; }
        public TimeOnly Jam { get; set; }
        public string Keluhan { get; set; } = string.Empty;
        public string Status { get; set; } = "menunggu";
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }

        public string? NamaPasien { get; set; }
        public string? NamaDokter { get; set; }
        public string? Spesialisasi { get; set; }
    }

    public class JadwalRequest
    {
        public int PasienId { get; set; }
        public int DokterId { get; set; }
        public DateOnly Tanggal { get; set; }
        public TimeOnly Jam { get; set; }
        public string Keluhan { get; set; } = string.Empty;
        public string Status { get; set; } = "menunggu";
    }
}