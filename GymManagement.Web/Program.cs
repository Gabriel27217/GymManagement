using Microsoft.AspNetCore.Authentication.Cookies;

var builder = WebApplication.CreateBuilder(args);

// ── Razor Pages ──────────────────────────────────────────────────
builder.Services.AddRazorPages(options =>
{
    // Páginas que requerem autenticação Admin
    options.Conventions.AuthorizeFolder("/Salas", "AdminOuInstrutor");
    options.Conventions.AuthorizeFolder("/Instrutores", "AdminOuInstrutor");
    options.Conventions.AuthorizeFolder("/Utilizadores", "AdminOnly");
    options.Conventions.AuthorizeFolder("/Frequencias", "AdminOuInstrutor");
    options.Conventions.AuthorizeFolder("/PlanosTreino", "AdminOuInstrutor");

    // Páginas autenticadas (qualquer role)
    options.Conventions.AuthorizePage("/Dashboard");
    options.Conventions.AuthorizeFolder("/MinhasInscricoes");

    // Páginas públicas
    options.Conventions.AllowAnonymousToPage("/Index");
    options.Conventions.AllowAnonymousToPage("/Sobre");
    options.Conventions.AllowAnonymousToPage("/Account/Login");
    options.Conventions.AllowAnonymousToPage("/Account/Register");
});

// ── HttpClient para a API ────────────────────────────────────────
var apiBaseUrl = builder.Configuration["ApiSettings:BaseUrl"]
                 ?? "https://localhost:7000/";

builder.Services.AddHttpClient("GymAPI", client =>
{
    client.BaseAddress = new Uri(apiBaseUrl);
    client.Timeout = TimeSpan.FromSeconds(30);
});

// ── Session (para guardar o JWT) ─────────────────────────────────
builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromHours(8);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
    options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
    options.Cookie.SameSite = SameSiteMode.Lax;
});

// ── HttpContextAccessor ─────────────────────────────────────────
builder.Services.AddHttpContextAccessor();

// ── ApiService ───────────────────────────────────────────────────
builder.Services.AddScoped<GymManagement.Web.Services.ApiService>(sp =>
{
    var factory = sp.GetRequiredService<IHttpClientFactory>();
    var ctx = sp.GetRequiredService<IHttpContextAccessor>();
    var client = factory.CreateClient("GymAPI");
    return new GymManagement.Web.Services.ApiService(client, ctx);
});

// ── Autenticação via Cookie (para [Authorize] funcionar nas Pages)
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/Account/Login";
        options.LogoutPath = "/Account/Logout";
        options.AccessDeniedPath = "/Erro/Acesso";
        options.ExpireTimeSpan = TimeSpan.FromHours(8);
        options.SlidingExpiration = true;
    });

// ── Autorização com políticas ────────────────────────────────────
builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("AdminOnly", policy =>
        policy.RequireRole("Admin"));

    options.AddPolicy("AdminOuInstrutor", policy =>
        policy.RequireRole("Admin", "Instrutor"));
});

var app = builder.Build();

// ── Pipeline ─────────────────────────────────────────────────────
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();

app.UseSession();          // antes de Authentication
app.UseAuthentication();
app.UseAuthorization();

app.MapRazorPages();

app.Run();