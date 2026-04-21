using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Npgsql;
using Klinik_PAA.Models;

namespace Klinik_PAA.Controllers
{
    [ApiController]
    [Route("api/pasien")]
    [Authorize]
    public class PasienController : ControllerBase
    {
        private readonly string _connStr;

        public PasienController(IConfiguration configuration)
        {
            _connStr = configuration.GetConnectionString("koneksi")!;
        }

        [HttpGet]
        public IActionResult GetAll()
        {
            try
            {
                var list = new List<object>();
                using var conn = new NpgsqlConnection(_connStr);
                conn.Open();

                using var cmd = new NpgsqlCommand(
                    "SELECT id, nama, tanggal_lahir, jenis_kelamin, no_telepon, alamat, " +
                    "created_at, updated_at FROM pasien WHERE deleted_at IS NULL ORDER BY id", conn);

                using var r = cmd.ExecuteReader();
                while (r.Read())
                    list.Add(new
                    {
                        id = r.GetInt32(0),
                        nama = r.GetString(1),
                        tanggalLahir = r.GetFieldValue<DateOnly>(2),
                        jenisKelamin = r.GetString(3),
                        noTelepon = r.GetString(4),
                        alamat = r.GetString(5),
                        createdAt = r.GetDateTime(6),
                        updatedAt = r.GetDateTime(7)
                    });

                return Ok(new { status = true, data = list, meta = new { total = list.Count } });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { status = false, message = ex.Message });
            }
        }

 
        [HttpGet("{id}")]
        public IActionResult GetById(int id)
        {
            try
            {
                using var conn = new NpgsqlConnection(_connStr);
                conn.Open();

                using var cmd = new NpgsqlCommand(
                    "SELECT id, nama, tanggal_lahir, jenis_kelamin, no_telepon, alamat, " +
                    "created_at, updated_at FROM pasien WHERE id=@id AND deleted_at IS NULL", conn);
                cmd.Parameters.AddWithValue("id", id);

                using var r = cmd.ExecuteReader();
                if (!r.Read())
                    return NotFound(new { status = false, message = "Pasien tidak ditemukan." });

                return Ok(new
                {
                    status = true,
                    data = new
                    {
                        id = r.GetInt32(0),
                        nama = r.GetString(1),
                        tanggalLahir = r.GetFieldValue<DateOnly>(2),
                        jenisKelamin = r.GetString(3),
                        noTelepon = r.GetString(4),
                        alamat = r.GetString(5),
                        createdAt = r.GetDateTime(6),
                        updatedAt = r.GetDateTime(7)
                    }
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { status = false, message = ex.Message });
            }
        }

 
        [HttpPost]
        public IActionResult Create([FromBody] PasienRequest req)
        {
            if (string.IsNullOrWhiteSpace(req.Nama) || string.IsNullOrWhiteSpace(req.Alamat))
                return BadRequest(new { status = false, message = "Nama dan alamat wajib diisi." });

            var validGender = new[] { "Laki-laki", "Perempuan" };
            if (!validGender.Contains(req.JenisKelamin))
                return BadRequest(new { status = false, message = "Jenis kelamin harus 'Laki-laki' atau 'Perempuan'." });

            try
            {
                using var conn = new NpgsqlConnection(_connStr);
                conn.Open();

                using var cmd = new NpgsqlCommand(
                    @"INSERT INTO pasien (nama, tanggal_lahir, jenis_kelamin, no_telepon, alamat, created_at, updated_at)
                      VALUES (@nama, @tl, @jk, @tel, @alamat, NOW(), NOW())
                      RETURNING id, nama, tanggal_lahir, created_at", conn);
                cmd.Parameters.AddWithValue("nama", req.Nama);
                cmd.Parameters.AddWithValue("tl", req.TanggalLahir);
                cmd.Parameters.AddWithValue("jk", req.JenisKelamin);
                cmd.Parameters.AddWithValue("tel", req.NoTelepon);
                cmd.Parameters.AddWithValue("alamat", req.Alamat);

                using var r = cmd.ExecuteReader();
                r.Read();
                return Created("", new
                {
                    status = true,
                    message = "Pasien berhasil ditambahkan.",
                    data = new
                    {
                        id = r.GetInt32(0),
                        nama = r.GetString(1),
                        tanggalLahir = r.GetFieldValue<DateOnly>(2),
                        createdAt = r.GetDateTime(3)
                    }
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { status = false, message = ex.Message });
            }
        }

   
        [HttpPut("{id}")]
        public IActionResult Update(int id, [FromBody] PasienRequest req)
        {
            if (string.IsNullOrWhiteSpace(req.Nama))
                return BadRequest(new { status = false, message = "Nama wajib diisi." });

            try
            {
                using var conn = new NpgsqlConnection(_connStr);
                conn.Open();

                using var cmd = new NpgsqlCommand(
                    @"UPDATE pasien SET nama=@nama, tanggal_lahir=@tl, jenis_kelamin=@jk,
                      no_telepon=@tel, alamat=@alamat, updated_at=NOW()
                      WHERE id=@id AND deleted_at IS NULL RETURNING id", conn);
                cmd.Parameters.AddWithValue("nama", req.Nama);
                cmd.Parameters.AddWithValue("tl", req.TanggalLahir);
                cmd.Parameters.AddWithValue("jk", req.JenisKelamin);
                cmd.Parameters.AddWithValue("tel", req.NoTelepon);
                cmd.Parameters.AddWithValue("alamat", req.Alamat);
                cmd.Parameters.AddWithValue("id", id);

                var result = cmd.ExecuteScalar();
                if (result == null)
                    return NotFound(new { status = false, message = "Pasien tidak ditemukan." });

                return Ok(new { status = true, message = "Data pasien berhasil diperbarui." });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { status = false, message = ex.Message });
            }
        }

   
        [HttpDelete("{id}")]
        public IActionResult Delete(int id)
        {
            try
            {
                using var conn = new NpgsqlConnection(_connStr);
                conn.Open();

                using var cmd = new NpgsqlCommand(
                    "UPDATE pasien SET deleted_at=NOW(), updated_at=NOW() " +
                    "WHERE id=@id AND deleted_at IS NULL RETURNING id", conn);
                cmd.Parameters.AddWithValue("id", id);

                var result = cmd.ExecuteScalar();
                if (result == null)
                    return NotFound(new { status = false, message = "Pasien tidak ditemukan." });

                return Ok(new { status = true, message = "Pasien berhasil dihapus." });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { status = false, message = ex.Message });
            }
        }
    }
}