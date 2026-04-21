using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Npgsql;
using Klinik_PAA.Models;

namespace Klinik_PAA.Controllers
{
    [ApiController]
    [Route("api/auth")]
    public class AuthController : ControllerBase
    {
        private readonly string _connStr;
        private readonly IConfiguration _config;

        public AuthController(IConfiguration configuration)
        {
            _config = configuration;
            _connStr = configuration.GetConnectionString("koneksi")!;
        }

        [HttpPost("register")]
        public IActionResult Register([FromBody] RegisterRequest req)
        {
            if (string.IsNullOrWhiteSpace(req.Username) || string.IsNullOrWhiteSpace(req.Password))
                return BadRequest(new { status = false, message = "Username dan password wajib diisi." });

            string hashedPassword = BCrypt.Net.BCrypt.HashPassword(req.Password);

            try
            {
                using var conn = new NpgsqlConnection(_connStr);
                conn.Open();

                using var checkCmd = new NpgsqlCommand(
                    "SELECT COUNT(1) FROM users WHERE username = @username AND deleted_at IS NULL", conn);
                checkCmd.Parameters.AddWithValue("username", req.Username);
                var count = (long)(checkCmd.ExecuteScalar() ?? 0L);

                if (count > 0)
                    return Conflict(new { status = false, message = "Username sudah digunakan." });

                using var cmd = new NpgsqlCommand(
                    @"INSERT INTO users (username, password, role, created_at, updated_at)
                      VALUES (@username, @password, @role, NOW(), NOW())
                      RETURNING id, username, role, created_at", conn);
                cmd.Parameters.AddWithValue("username", req.Username);
                cmd.Parameters.AddWithValue("password", hashedPassword);
                cmd.Parameters.AddWithValue("role", req.Role);

                using var reader = cmd.ExecuteReader();
                if (reader.Read())
                {
                    return Created("", new
                    {
                        status = true,
                        message = "Registrasi berhasil.",
                        data = new
                        {
                            id = reader.GetInt32(0),
                            username = reader.GetString(1),
                            role = reader.GetString(2),
                            createdAt = reader.GetDateTime(3)
                        }
                    });
                }

                return StatusCode(500, new { status = false, message = "Gagal menyimpan user." });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { status = false, message = "Server error: " + ex.Message });
            }
        }

        [HttpPost("login")]
        public IActionResult Login([FromBody] LoginRequest req)
        {
            if (string.IsNullOrWhiteSpace(req.Username) || string.IsNullOrWhiteSpace(req.Password))
                return BadRequest(new { status = false, message = "Username dan password wajib diisi." });

            try
            {
                using var conn = new NpgsqlConnection(_connStr);
                conn.Open();

                using var cmd = new NpgsqlCommand(
                    "SELECT id, username, password, role FROM users WHERE username = @username AND deleted_at IS NULL",
                    conn);
                cmd.Parameters.AddWithValue("username", req.Username);

                using var reader = cmd.ExecuteReader();
                if (!reader.Read())
                    return Unauthorized(new { status = false, message = "Username atau password salah." });

                int userId = reader.GetInt32(0);
                string username = reader.GetString(1);
                string hashPw = reader.GetString(2);
                string role = reader.GetString(3);

                if (!BCrypt.Net.BCrypt.Verify(req.Password, hashPw))
                    return Unauthorized(new { status = false, message = "Username atau password salah." });

                string token = GenerateJwt(userId, username, role);

                return Ok(new
                {
                    status = true,
                    message = "Login berhasil.",
                    data = new { id = userId, username, role, token }
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { status = false, message = "Server error: " + ex.Message });
            }
        }

        private string GenerateJwt(int userId, string username, string role)
        {
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_config["Jwt:Key"]!));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var claims = new[]
            {
                new Claim(ClaimTypes.NameIdentifier, userId.ToString()),
                new Claim(ClaimTypes.Name,           username),
                new Claim(ClaimTypes.Role,           role)
            };

            var token = new JwtSecurityToken(
                issuer: _config["Jwt:Issuer"],
                audience: _config["Jwt:Audience"],
                claims: claims,
                expires: DateTime.UtcNow.AddMinutes(
                                        double.Parse(_config["Jwt:ExpireMinutes"]!)),
                signingCredentials: creds
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }
}