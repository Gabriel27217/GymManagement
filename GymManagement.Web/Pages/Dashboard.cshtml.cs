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
        private readonly IConfiguration _config;

        public DashboardModel(ApiService api, IHttpClientFactory factory, IConfiguration config)
        {
            _api     = api;
            _factory = factory;
            _config  = config;
        }

        public List<AulaListDto> Aulas        { get; set; } = new();
        public int TotalAulas      { get; set; }
        public int TotalPlanos     { get; set; }
        public int TotalInstrutores { get; set; }
        public int AulasLotadas    { get; set; }

        public async Task<IActionResult> OnGetAsync()
        {
            if (HttpContext.Session.GetString("Role") != "Admin")
                return RedirectToPage("/Account/Login");

            try
            {
                Aulas           = await _api.GetAulasAsync();
                TotalAulas      = Aulas.Count;
                AulasLotadas    = Aulas.Count(a => a.Lotada);
                TotalPlanos     = (await _api.GetPlanosAsync()).Count;

                // Buscar total de instrutores
                var http = _factory.CreateClient();
                http.BaseAddress = new Uri(_config["ApiSettings:BaseUrl"]!);
                http.DefaultRequestHeaders.Authorization =
                    new AuthenticationHeaderValue("Bearer", HttpContext.Session.GetString("JWT"));
                var resp = await http.GetStringAsync("api/Instrutores");
                var instrutores = JsonSerializer.Deserialize<List<object>>(resp);
                TotalInstrutores = instrutores?.Count ?? 0;
            }
            catch { }

            return Page();
        }
    }
}
