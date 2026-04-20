using GymManagement.Web.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Text.Json;

namespace GymManagement.Web.Pages
{
    public class MinhasInscricoesModel : PageModel
    {
        private readonly ApiService _api;
        public MinhasInscricoesModel(ApiService api) => _api = api;
        public List<JsonElement> Inscricoes { get; set; } = new();

        public async Task OnGetAsync()
        {
            try
            {
                var json = await _api.GetMinhasInscricoesRawAsync();
                Inscricoes = JsonSerializer.Deserialize<List<JsonElement>>(json) ?? new();
            }
            catch { }
        }
    }
}

namespace GymManagement.Web.Pages.Aulas
{
    public class CancelarInscricaoModel : PageModel
    {
        private readonly ApiService _api;
        public CancelarInscricaoModel(ApiService api) => _api = api;

        public async Task<IActionResult> OnGetAsync(int id)
        {
            var (ok, msg) = await _api.CancelarInscricaoAsync(id);
            TempData[ok ? "Sucesso" : "Erro"] = msg;
            return RedirectToPage("/MinhasInscricoes");
        }
    }
}
