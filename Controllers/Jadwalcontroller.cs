using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Npgsql;
using Klinik_PAA.Models;

namespace Klinik_PAA.Controllers
{
    [ApiController]
    [Route("api/jadwal")]
    [Authorize]
    public class JadwalController : ControllerBase
    {
        private readonly string _connStr;

        public JadwalController(IConfiguration configuration)
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
                    @"SELECT j.id, j.pasien_id, p.nama AS nama_pasien,
                             j.dokter_id, d.nama AS nama_dokter, d.spesialisasi,
                             j.tanggal, j.jam, j.keluhan, j.status,
                             j.created_at, j.updated_at
                      FROM jadwal j
                      JOIN pasien p ON p.id = j.pasien_id
                      JOIN dokter d ON d.id = j.dokter_id
                      ORDER BY j.tanggal DESC, j.jam ASC", conn);

                using var r = cmd.ExecuteReader();
                while (r.Read())
                    list.Add(new
                    {
                        id = r.GetInt32(0),
                        pasienId = r.GetInt32(1),
                        namaPasien = r.GetString(2),
                        dokterId = r.GetInt32(3),
                        namaDokter = r.GetString(4),
                        spesialisasi = r.GetString(5),
                        tanggal = r.GetFieldValue<DateOnly>(6),
                        jam = r.GetFieldValue<TimeOnly>(7),
                        keluhan = r.GetString(8),
                        status = r.GetString(9),
                        createdAt = r.GetDateTime(10),
                        updatedAt = r.GetDateTime(11)
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
                    @"SELECT j.id, j.pasien_id, p.nama AS nama_pasien,
                             j.dokter_id, d.nama AS nama_dokter, d.spesialisasi,
                             j.tanggal, j.jam, j.keluhan, j.status,
                             j.created_at, j.updated_at
                      FROM jadwal j
                      JOIN pasien p ON p.id = j.pasien_id
                      JOIN dokter d ON d.id = j.dokter_id
                      WHERE j.id = @id", conn);
                cmd.Parameters.AddWithValue("id", id);

                using var r = cmd.ExecuteReader();
                if (!r.Read())
                    return NotFound(new { status = false, message = "Jadwal tidak ditemukan." });

                return Ok(new
                {
                    status = true,
                    data = new
                    {
                        id = r.GetInt32(0),
                        pasienId = r.GetInt32(1),
                        namaPasien = r.GetString(2),
                        dokterId = r.GetInt32(3),
                        namaDokter = r.GetString(4),
                        spesialisasi = r.GetString(5),
                        tanggal = r.GetFieldValue<DateOnly>(6),
                        jam = r.GetFieldValue<TimeOnly>(7),
                        keluhan = r.GetString(8),
                        status = r.GetString(9),
                        createdAt = r.GetDateTime(10),
                        updatedAt = r.GetDateTime(11)
                    }
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { status = false, message = ex.Message });
            }
        }

        [HttpPost]
        public IActionResult Create([FromBody] JadwalRequest req)
        {
            if (req.PasienId <= 0 || req.DokterId <= 0)
                return BadRequest(new { status = false, message = "PasienId dan DokterId wajib diisi." });

            if (string.IsNullOrWhiteSpace(req.Keluhan))
                return BadRequest(new { status = false, message = "Keluhan wajib diisi." });

            var validStatus = new[] { "menunggu", "selesai", "batal" };
            if (!validStatus.Contains(req.Status))
                return BadRequest(new { status = false, message = "Status harus: menunggu / selesai / batal." });

            try
            {
                using var conn = new NpgsqlConnection(_connStr);
                conn.Open();

                using var chkPasien = new NpgsqlCommand(
                    "SELECT COUNT(1) FROM pasien WHERE id=@id AND deleted_at IS NULL", conn);
                chkPasien.Parameters.AddWithValue("id", req.PasienId);
                if ((long)(chkPasien.ExecuteScalar() ?? 0L) == 0)
                    return NotFound(new { status = false, message = "Pasien tidak ditemukan." });

                using var chkDokter = new NpgsqlCommand(
                    "SELECT COUNT(1) FROM dokter WHERE id=@id AND deleted_at IS NULL", conn);
                chkDokter.Parameters.AddWithValue("id", req.DokterId);
                if ((long)(chkDokter.ExecuteScalar() ?? 0L) == 0)
                    return NotFound(new { status = false, message = "Dokter tidak ditemukan." });

                using var cmd = new NpgsqlCommand(
                    @"INSERT INTO jadwal (pasien_id, dokter_id, tanggal, jam, keluhan, status, created_at, updated_at)
                      VALUES (@pid, @did, @tgl, @jam, @keluhan, @status, NOW(), NOW())
                      RETURNING id, created_at", conn);
                cmd.Parameters.AddWithValue("pid", req.PasienId);
                cmd.Parameters.AddWithValue("did", req.DokterId);
                cmd.Parameters.AddWithValue("tgl", req.Tanggal);
                cmd.Parameters.AddWithValue("jam", req.Jam);
                cmd.Parameters.AddWithValue("keluhan", req.Keluhan);
                cmd.Parameters.AddWithValue("status", req.Status);

                using var r = cmd.ExecuteReader();
                r.Read();
                return Created("", new
                {
                    status = true,
                    message = "Jadwal berhasil dibuat.",
                    data = new { id = r.GetInt32(0), createdAt = r.GetDateTime(1) }
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { status = false, message = ex.Message });
            }
        }

        [HttpPut("{id}")]
        public IActionResult Update(int id, [FromBody] JadwalRequest req)
        {
            var validStatus = new[] { "menunggu", "selesai", "batal" };
            if (!validStatus.Contains(req.Status))
                return BadRequest(new { status = false, message = "Status harus: menunggu / selesai / batal." });

            try
            {
                using var conn = new NpgsqlConnection(_connStr);
                conn.Open();

                using var cmd = new NpgsqlCommand(
                    @"UPDATE jadwal SET pasien_id=@pid, dokter_id=@did, tanggal=@tgl,
                      jam=@jam, keluhan=@keluhan, status=@status, updated_at=NOW()
                      WHERE id=@id RETURNING id", conn);
                cmd.Parameters.AddWithValue("pid", req.PasienId);
                cmd.Parameters.AddWithValue("did", req.DokterId);
                cmd.Parameters.AddWithValue("tgl", req.Tanggal);
                cmd.Parameters.AddWithValue("jam", req.Jam);
                cmd.Parameters.AddWithValue("keluhan", req.Keluhan);
                cmd.Parameters.AddWithValue("status", req.Status);
                cmd.Parameters.AddWithValue("id", id);

                var result = cmd.ExecuteScalar();
                if (result == null)
                    return NotFound(new { status = false, message = "Jadwal tidak ditemukan." });

                return Ok(new { status = true, message = "Jadwal berhasil diperbarui." });
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
                    "DELETE FROM jadwal WHERE id=@id RETURNING id", conn);
                cmd.Parameters.AddWithValue("id", id);

                var result = cmd.ExecuteScalar();
                if (result == null)
                    return NotFound(new { status = false, message = "Jadwal tidak ditemukan." });

                return Ok(new { status = true, message = "Jadwal berhasil dihapus." });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { status = false, message = ex.Message });
            }
        }

        [HttpPatch("{id}/status")]
        public IActionResult UpdateStatus(int id, [FromBody] UpdateStatusRequest req)
        {
            var validStatus = new[] { "menunggu", "selesai", "batal" };
            if (!validStatus.Contains(req.Status))
                return BadRequest(new { status = false, message = "Status harus: menunggu / selesai / batal." });

            try
            {
                using var conn = new NpgsqlConnection(_connStr);
                conn.Open();

                using var cmd = new NpgsqlCommand(
                    "UPDATE jadwal SET status=@status, updated_at=NOW() WHERE id=@id RETURNING id", conn);
                cmd.Parameters.AddWithValue("status", req.Status);
                cmd.Parameters.AddWithValue("id", id);

                var result = cmd.ExecuteScalar();
                if (result == null)
                    return NotFound(new { status = false, message = "Jadwal tidak ditemukan." });

                return Ok(new { status = true, message = $"Status jadwal diubah menjadi '{req.Status}'." });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { status = false, message = ex.Message });
            }
        }
    }

    public class UpdateStatusRequest
    {
        public string Status { get; set; } = string.Empty;
    }
}