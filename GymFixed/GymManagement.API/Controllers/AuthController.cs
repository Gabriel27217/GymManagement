using GymManagement.Data;
using GymManagement.Models.DTOs;
using GymManagement.Models.Entities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace GymManagement.API.Controllers
{
    /// <summary>
    /// Autenticação: login e registo de utilizadores.
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly GymDbContext _db;
        private readonly IConfiguration _config;

        public AuthController(GymDbContext db, IConfiguration config)
        {
            _db = db;
            _config = config;
        }

        /// <summary>POST /api/Auth/Login — autentica um utilizador e devolve JWT.</summary>
        [HttpPost("Login")]
        public async Task<IActionResult> Login([FromBody] LoginDto dto)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            var utilizador = await _db.Utilizadores
                .FirstOrDefaultAsync(u => u.Email == dto.Email && u.Ativo);

            if (utilizador == null || string.IsNullOrEmpty(utilizador.PasswordHash))
                return Unauthorized(new { mensagem = "Email ou password incorretos." });

            if (!BCrypt.Net.BCrypt.Verify(dto.Password, utilizador.PasswordHash))
                return Unauthorized(new { mensagem = "Email ou password incorretos." });

            var token = GerarToken(utilizador);
            return Ok(new AuthResponseDto
            {
                Token = token,
                Nome = utilizador.Nome,
                Email = utilizador.Email,
                Role = utilizador.Role.ToString(),
                Expira = DateTime.UtcNow.AddHours(int.Parse(_config["JwtSettings:ExpiracaoHoras"]!))
            });
        }

        /// <summary>POST /api/Auth/Register — regista um novo cliente.</summary>
        [HttpPost("Register")]
        public async Task<IActionResult> Register([FromBody] RegisterDto dto)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            if (await _db.Utilizadores.AnyAsync(u => u.Email == dto.Email))
                return Conflict(new { mensagem = "Este email já está registado." });

            var utilizador = new Utilizador
            {
                Nome = dto.Nome,
                Email = dto.Email,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.Password),
                Telefone = dto.Telefone,
                Role = TipoUtilizador.Cliente
            };

            _db.Utilizadores.Add(utilizador);
            await _db.SaveChangesAsync();

            return CreatedAtAction(nameof(Login), new { mensagem = "Conta criada com sucesso." });
        }

        // FIX #6: removido XML doc comment com texto aleatório "frdrtdfvrtfv"
        private string GerarToken(Utilizador utilizador)
        {
            var claims = new[]
            {
                new Claim(ClaimTypes.NameIdentifier, utilizador.Id.ToString()),
                new Claim(ClaimTypes.Name,  utilizador.Nome),
                new Claim(ClaimTypes.Email, utilizador.Email),
                new Claim(ClaimTypes.Role,  utilizador.Role.ToString())
            };

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_config["JwtSettings:SecretKey"]!));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
            var horas = int.Parse(_config["JwtSettings:ExpiracaoHoras"]!);

            var token = new JwtSecurityToken(
                issuer: _config["JwtSettings:Issuer"],
                audience: _config["JwtSettings:Audience"],
                claims: claims,
                expires: DateTime.UtcNow.AddHours(horas),
                signingCredentials: creds
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }
}
