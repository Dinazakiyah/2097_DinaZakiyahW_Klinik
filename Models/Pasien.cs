namespace Klinik_PAA.Models
{
    public class Pasien
    {
        public int Id { get; set; }
        public string Nama { get; set; } = string.Empty;
        public DateOnly TanggalLahir { get; set; }
        public string JenisKelamin { get; set; } = string.Empty;
        public string NoTelepon { get; set; } = string.Empty;
        public string Alamat { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        public DateTime? DeletedAt { get; set; }
    }

    public class PasienRequest
    {
        public string Nama { get; set; } = string.Empty;
        public DateOnly TanggalLahir { get; set; }
        public string JenisKelamin { get; set; } = string.Empty;
        public string NoTelepon { get; set; } = string.Empty;
        public string Alamat { get; set; } = string.Empty;
    }
}