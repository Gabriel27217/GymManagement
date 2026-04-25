using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Net.Http.Headers;

namespace GymManagement.Web.Pages.Aulas
{
    [Authorize]
    public class CancelarInscricaoModel : PageModel
    {
        private readonly IHttpClientFactory _clientFactory;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public CancelarInscricaoModel(IHttpClientFactory clientFactory, IHttpContextAccessor httpContextAccessor)
        {
            _clientFactory = clientFactory;
            _httpContextAccessor = httpContextAccessor;
        }

        [BindProperty(SupportsGet = true)]
        public int InscricaoId { get; set; }

        [BindProperty(SupportsGet = true)]
        public int AulaId { get; set; }

        public string NomeAula { get; set; } = string.Empty;

        private HttpClient CreateClient()
        {
            var client = _clientFactory.CreateClient("GymAPI");
            var token = _httpContextAccessor.HttpContext?.Session.GetString("JwtToken");
            if (!string.IsNullOrEmpty(token))
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
            return client;
        }

        public async Task<IActionResult> OnGetAsync()
        {
            if (InscricaoId <= 0)
                return RedirectToPage("/MinhasInscricoes/Index");

            // Carregar nome da aula para confirmação
            var client = CreateClient();
            var response = await client.GetAsync($"api/aulas/{AulaId}");
            if (response.IsSuccessStatusCode)
            {
                var json = await response.Content.ReadAsStringAsync();
                using var doc = System.Text.Json.JsonDocument.Parse(json);
                NomeAula = doc.RootElement.GetProperty("nome").GetString() ?? "Aula";
            }

            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            var client = CreateClient();
            var response = await client.DeleteAsync($"api/inscricoes/{InscricaoId}");

            if (!response.IsSuccessStatusCode)
            {
                TempData["Erro"] = "Não foi possível cancelar a inscrição. Tente novamente.";
                return RedirectToPage("/MinhasInscricoes/Index");
            }

            TempData["Sucesso"] = "Inscrição cancelada com sucesso.";
            return RedirectToPage("/MinhasInscricoes/Index");
        }
    }
}
