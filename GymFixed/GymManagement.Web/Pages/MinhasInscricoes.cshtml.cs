using GymManagement.Web.Services;
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
