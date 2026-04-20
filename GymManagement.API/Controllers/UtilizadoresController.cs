using GymManagement.Data;
using GymManagement.Models.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace GymManagement.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Roles = "Admin")]
    public class UtilizadoresController : ControllerBase
    {
        private readonly GymDbContext _db;
        public UtilizadoresController(GymDbContext db) => _db = db;

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var utilizadores = await _db.Utilizadores
                .OrderBy(u => u.Nome)
                .Select(u => new {
                    u.Id, u.Nome, u.Email, u.Telefone,
                    Role = u.Role.ToString(),
                    u.Ativo, u.DataRegisto
                })
                .ToListAsync();
            return Ok(utilizadores);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var u = await _db.Utilizadores.FindAsync(id);
            if (u == null) return NotFound();
            return Ok(new { u.Id, u.Nome, u.Email, u.Telefone, Role = u.Role.ToString(), u.Ativo, u.DataRegisto });
        }

        [HttpPut("{id}/ativar")]
        public async Task<IActionResult> Ativar(int id)
        {
            var u = await _db.Utilizadores.FindAsync(id);
            if (u == null) return NotFound();
            u.Ativo = !u.Ativo;
            await _db.SaveChangesAsync();
            return Ok(new { mensagem = u.Ativo ? "Utilizador ativado." : "Utilizador desativado.", u.Ativo });
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var u = await _db.Utilizadores.FindAsync(id);
            if (u == null) return NotFound();
            _db.Utilizadores.Remove(u);
            await _db.SaveChangesAsync();
            return NoContent();
        }
    }
}
