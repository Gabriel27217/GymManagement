using GymManagement.Models.DTOs;
using GymManagement.Web.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Net.Http.Headers;
using System.Text.Json;

namespace GymManagement.Web.Pages
{
    public class DashboardModel : PageModel
    {
        private readonly ApiService _api;
        private readonly IHttpClientFactory _factory;

        public DashboardModel(ApiService api, IHttpClientFactory factory)
        {
            _api     = api;
            _factory = factory;
        }

        public List<AulaListDto> Aulas         { get; set; } = new();
        public int TotalAulas       { get; set; }
        public int TotalPlanos      { get; set; }
        public int TotalInstrutores { get; set; }
        public int AulasLotadas     { get; set; }

        public async Task<IActionResult> OnGetAsync()
        {
            // Verificar via claims E via sessão (dupla segurança)
            var roleFromClaims  = User.IsInRole("Admin");
            var roleFromSession = HttpContext.Session.GetString("Role") == "Admin";

            if (!roleFromClaims && !roleFromSession)
                return RedirectToPage("/MinhasInscricoes");

            try
            {
                Aulas            = await _api.GetAulasAsync();
                TotalAulas       = Aulas.Count;
                AulasLotadas     = Aulas.Count(a => a.Lotada);
                TotalPlanos      = (await _api.GetPlanosAsync()).Count;

                var http = _factory.CreateClient("GymAPI");
                http.DefaultRequestHeaders.Authorization =
                    new AuthenticationHeaderValue("Bearer", HttpContext.Session.GetString("JWT"));
                var resp = await http.GetStringAsync("api/Instrutores");
                TotalInstrutores = JsonSerializer.Deserialize<List<object>>(resp)?.Count ?? 0;
            }
            catch { }

            return Page();
        }
    }
}
