using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Npgsql;
using Klinik_PAA.Models;

namespace Klinik_PAA.Controllers
{
    [ApiController]
    [Route("api/dokter")]
    [Authorize]
    public class DokterController : ControllerBase
    {
        private readonly string _connStr;

        public DokterController(IConfiguration configuration)
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
                    "SELECT id, nama, spesialisasi, no_telepon, email, created_at, updated_at " +
                    "FROM dokter WHERE deleted_at IS NULL ORDER BY id", conn);

                using var r = cmd.ExecuteReader();
                while (r.Read())
                    list.Add(new
                    {
                        id = r.GetInt32(0),
                        nama = r.GetString(1),
                        spesialisasi = r.GetString(2),
                        noTelepon = r.GetString(3),
                        email = r.GetString(4),
                        createdAt = r.GetDateTime(5),
                        updatedAt = r.GetDateTime(6)
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
                    "SELECT id, nama, spesialisasi, no_telepon, email, created_at, updated_at " +
                    "FROM dokter WHERE id = @id AND deleted_at IS NULL", conn);
                cmd.Parameters.AddWithValue("id", id);

                using var r = cmd.ExecuteReader();
                if (!r.Read())
                    return NotFound(new { status = false, message = "Dokter tidak ditemukan." });

                return Ok(new
                {
                    status = true,
                    data = new
                    {
                        id = r.GetInt32(0),
                        nama = r.GetString(1),
                        spesialisasi = r.GetString(2),
                        noTelepon = r.GetString(3),
                        email = r.GetString(4),
                        createdAt = r.GetDateTime(5),
                        updatedAt = r.GetDateTime(6)
                    }
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { status = false, message = ex.Message });
            }
        }

        [HttpPost]
        public IActionResult Create([FromBody] DokterRequest req)
        {
            if (string.IsNullOrWhiteSpace(req.Nama) || string.IsNullOrWhiteSpace(req.Email))
                return BadRequest(new { status = false, message = "Nama dan email wajib diisi." });

            try
            {
                using var conn = new NpgsqlConnection(_connStr);
                conn.Open();

                using var cmd = new NpgsqlCommand(
                    @"INSERT INTO dokter (nama, spesialisasi, no_telepon, email, created_at, updated_at)
                      VALUES (@nama, @spes, @tel, @email, NOW(), NOW())
                      RETURNING id, nama, spesialisasi, no_telepon, email, created_at", conn);
                cmd.Parameters.AddWithValue("nama", req.Nama);
                cmd.Parameters.AddWithValue("spes", req.Spesialisasi);
                cmd.Parameters.AddWithValue("tel", req.NoTelepon);
                cmd.Parameters.AddWithValue("email", req.Email);

                using var r = cmd.ExecuteReader();
                r.Read();
                return Created("", new
                {
                    status = true,
                    message = "Dokter berhasil ditambahkan.",
                    data = new
                    {
                        id = r.GetInt32(0),
                        nama = r.GetString(1),
                        spesialisasi = r.GetString(2),
                        noTelepon = r.GetString(3),
                        email = r.GetString(4),
                        createdAt = r.GetDateTime(5)
                    }
                });
            }
            catch (PostgresException pgEx) when (pgEx.SqlState == "23505")
            {
                return Conflict(new { status = false, message = "Email dokter sudah terdaftar." });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { status = false, message = ex.Message });
            }
        }

        [HttpPut("{id}")]
        public IActionResult Update(int id, [FromBody] DokterRequest req)
        {
            if (string.IsNullOrWhiteSpace(req.Nama) || string.IsNullOrWhiteSpace(req.Email))
                return BadRequest(new { status = false, message = "Nama dan email wajib diisi." });

            try
            {
                using var conn = new NpgsqlConnection(_connStr);
                conn.Open();

                using var cmd = new NpgsqlCommand(
                    @"UPDATE dokter
                      SET nama=@nama, spesialisasi=@spes, no_telepon=@tel,
                          email=@email, updated_at=NOW()
                      WHERE id=@id AND deleted_at IS NULL
                      RETURNING id", conn);
                cmd.Parameters.AddWithValue("nama", req.Nama);
                cmd.Parameters.AddWithValue("spes", req.Spesialisasi);
                cmd.Parameters.AddWithValue("tel", req.NoTelepon);
                cmd.Parameters.AddWithValue("email", req.Email);
                cmd.Parameters.AddWithValue("id", id);

                var result = cmd.ExecuteScalar();
                if (result == null)
                    return NotFound(new { status = false, message = "Dokter tidak ditemukan." });

                return Ok(new { status = true, message = "Data dokter berhasil diperbarui." });
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
                    "UPDATE dokter SET deleted_at=NOW(), updated_at=NOW() " +
                    "WHERE id=@id AND deleted_at IS NULL RETURNING id", conn);
                cmd.Parameters.AddWithValue("id", id);

                var result = cmd.ExecuteScalar();
                if (result == null)
                    return NotFound(new { status = false, message = "Dokter tidak ditemukan." });

                return Ok(new { status = true, message = "Dokter berhasil dihapus." });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { status = false, message = ex.Message });
            }
        }
    }
}