using GymManagement.Models.DTOs;
using GymManagement.Web.Services;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace GymManagement.Web.Pages
{
    public class IndexModel : PageModel
    {
        private readonly ApiService _api;
        public IndexModel(ApiService api) => _api = api;

        public List<AulaListDto> ProximasAulas { get; set; } = new();
        public int TotalAulas   { get; set; }
        public int TotalPlanos  { get; set; }
        public int AulasLotadas { get; set; }

        public async Task OnGetAsync()
        {
            try
            {
                ProximasAulas = await _api.GetAulasAsync();
                TotalAulas    = ProximasAulas.Count;
                AulasLotadas  = ProximasAulas.Count(a => a.Lotada);
                TotalPlanos   = (await _api.GetPlanosAsync()).Count;
            }
            catch { }
        }
    }
}
