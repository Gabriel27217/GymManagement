using GymManagement.Models.DTOs;
using GymManagement.Web.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.ComponentModel.DataAnnotations;

namespace GymManagement.Web.Pages.Aulas
{
    public class IndexModel : PageModel
    {
        private readonly ApiService _api;
        public IndexModel(ApiService api) => _api = api;
        public List<AulaListDto> Aulas { get; set; } = new();
        public async Task OnGetAsync() { try { Aulas = await _api.GetAulasAsync(); } catch { } }
    }

    public class CreateModel : PageModel
    {
        private readonly ApiService _api;
        public CreateModel(ApiService api) => _api = api;

        [BindProperty, Required, Display(Name = "Nome da Aula")]
        public string Nome { get; set; } = string.Empty;

        [BindProperty, Required, Display(Name = "Dia da Semana")]
        public int DiaSemana { get; set; } = 1;

        [BindProperty, Required, Display(Name = "Hora")]
        public string Hora { get; set; } = "09:00";

        [BindProperty, Range(15, 180), Display(Name = "Duração (minutos)")]
        public int DuracaoMinutos { get; set; } = 60;

        [BindProperty, Required, Display(Name = "Plano de Treino")]
        public int PlanoTreinoId { get; set; }

        [BindProperty, Required, Display(Name = "Instrutor")]
        public int InstrutorId { get; set; }

        [BindProperty, Required, Display(Name = "Sala")]
        public int SalaId { get; set; }

        public List<PlanoTreinoDto> Planos { get; set; } = new();

        public async Task OnGetAsync() => Planos = await _api.GetPlanosAsync();

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid) { Planos = await _api.GetPlanosAsync(); return Page(); }

            var ok = await _api.CreateAulaAsync(new AulaCreateDto
            {
                Nome = Nome, DiaSemana = DiaSemana, Hora = Hora,
                DuracaoMinutos = DuracaoMinutos,
                PlanoTreinoId = PlanoTreinoId, InstrutorId = InstrutorId, SalaId = SalaId
            });

            if (!ok) { ModelState.AddModelError("", "Erro ao criar aula."); Planos = await _api.GetPlanosAsync(); return Page(); }

            TempData["Sucesso"] = $"Aula '{Nome}' criada com sucesso.";
            return RedirectToPage("Index");
        }
    }

    public class EditModel : PageModel
    {
        private readonly ApiService _api;
        public EditModel(ApiService api) => _api = api;

        [BindProperty] public AulaCreateDto Aula { get; set; } = new();
        [BindProperty] public int AulaId { get; set; }
        public List<PlanoTreinoDto> Planos { get; set; } = new();

        public async Task<IActionResult> OnGetAsync(int id)
        {
            var aula = await _api.GetAulaAsync(id);
            if (aula == null) return NotFound();
            AulaId = id;
            Aula = new AulaCreateDto
            {
                Nome = aula.Nome, DiaSemana = aula.DiaSemana,
                Hora = aula.Hora, DuracaoMinutos = aula.DuracaoMinutos
            };
            Planos = await _api.GetPlanosAsync();
            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid) { Planos = await _api.GetPlanosAsync(); return Page(); }
            await _api.UpdateAulaAsync(AulaId, Aula);
            TempData["Sucesso"] = "Aula atualizada com sucesso.";
            return RedirectToPage("Index");
        }
    }

    public class DeleteModel : PageModel
    {
        private readonly ApiService _api;
        public DeleteModel(ApiService api) => _api = api;
        public AulaListDto? Aula { get; set; }

        public async Task<IActionResult> OnGetAsync(int id)
        {
            Aula = await _api.GetAulaAsync(id);
            if (Aula == null) return NotFound();
            return Page();
        }

        public async Task<IActionResult> OnPostAsync(int id)
        {
            await _api.DeleteAulaAsync(id);
            TempData["Sucesso"] = "Aula eliminada com sucesso.";
            return RedirectToPage("Index");
        }
    }

    public class InscreverModel : PageModel
    {
        private readonly ApiService _api;
        public InscreverModel(ApiService api) => _api = api;

        public async Task<IActionResult> OnGetAsync(int id)
        {
            var (ok, msg) = await _api.InscreverAsync(id);
            TempData[ok ? "Sucesso" : "Erro"] = msg;
            return RedirectToPage("Index");
        }
    }
}
