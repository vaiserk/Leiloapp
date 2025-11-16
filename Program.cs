using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Leiloapp.Data;
using Leiloapp.Models.Entities;
using Leiloapp.Hubs;
using Leiloapp.Services;
using Leiloapp.Services.Interfaces;
using Serilog;
using System.Globalization;
using Microsoft.AspNetCore.Localization;

var builder = WebApplication.CreateBuilder(args);

// Configure Serilog
Log.Logger = new LoggerConfiguration()
    .ReadFrom.Configuration(builder.Configuration)
    .CreateLogger();

builder.Host.UseSerilog();

// Add services to the container.
builder.Services.AddControllersWithViews();
builder.Services.Configure<RequestLocalizationOptions>(options =>
{
    var cultures = new[] { new CultureInfo("pt-BR") };
    options.DefaultRequestCulture = new RequestCulture("pt-BR");
    options.SupportedCultures = cultures;
    options.SupportedUICultures = cultures;
});

// Configure Entity Framework with PostgreSQL
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("PostgreSQL")));

// Configure Identity
builder.Services.AddIdentity<Usuario, IdentityRole<int>>(options =>
{
    options.Password.RequireDigit = true;
    options.Password.RequiredLength = 8;
    options.Password.RequireNonAlphanumeric = false;
    options.Password.RequireUppercase = true;
    options.Password.RequireLowercase = true;
    
    options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(15);
    options.Lockout.MaxFailedAccessAttempts = 5;
    options.Lockout.AllowedForNewUsers = true;
    
    options.User.RequireUniqueEmail = true;
    options.User.AllowedUserNameCharacters = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789-._@+";
})
.AddEntityFrameworkStores<ApplicationDbContext>()
.AddDefaultTokenProviders();

// Configure application cookie paths
builder.Services.ConfigureApplicationCookie(options =>
{
    options.LoginPath = "/Account/Login";
    options.AccessDeniedPath = "/Account/AccessDenied";
});

// Configure SignalR
builder.Services.AddSignalR();



// Configure CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowLocalhost",
        builder => builder
            .WithOrigins("http://localhost:3000", "https://localhost:3000")
            .AllowAnyHeader()
            .AllowAnyMethod()
            .AllowCredentials());
});

// Add custom services
builder.Services.AddScoped<ILeilaoService, LeilaoService>();
builder.Services.AddScoped<ILanceService, LanceService>();
builder.Services.AddScoped<IUsuarioService, UsuarioService>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

CultureInfo.DefaultThreadCurrentCulture = new CultureInfo("pt-BR");
CultureInfo.DefaultThreadCurrentUICulture = new CultureInfo("pt-BR");
app.UseRequestLocalization();

// Ensure database exists for development run
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    db.Database.EnsureCreated();

    try
    {
        db.Database.ExecuteSqlRaw(@"ALTER TABLE ""Lotes"" DROP CONSTRAINT IF EXISTS ""FK_Lotes_Leiloes_LeilaoId""; ALTER TABLE ""Lotes"" ADD CONSTRAINT ""FK_Lotes_Leiloes_LeilaoId"" FOREIGN KEY (""LeilaoId"") REFERENCES ""Leiloes""(""Id"") ON DELETE CASCADE;");
        db.Database.ExecuteSqlRaw(@"ALTER TABLE ""Lotes"" ADD COLUMN IF NOT EXISTS ""Visivel"" boolean NOT NULL DEFAULT TRUE;");
    }
    catch { }

    var userManager = scope.ServiceProvider.GetRequiredService<UserManager<Usuario>>();

    var leiloeiroEmail = "leiloeiro@santacasa.local";
    var adminEmail = "admin@santacasa.local";
    var usuarioEmail = "usuario@santacasa.local";

    var leiloeiro = userManager.FindByEmailAsync(leiloeiroEmail).GetAwaiter().GetResult();
    if (leiloeiro == null)
    {
        leiloeiro = new Usuario
        {
            UserName = leiloeiroEmail,
            Email = leiloeiroEmail,
            EmailConfirmed = true,
            Nome = "Leiloeiro Teste",
            CPF = "00000000001",
            Telefone = "0000000000",
            PhoneNumber = "0000000000",
            TipoUsuario = 2,
            Aprovado = true,
            Ativo = true
        };
        userManager.CreateAsync(leiloeiro, "Leiloeiro123A").GetAwaiter().GetResult();
    }

    var admin = userManager.FindByEmailAsync(adminEmail).GetAwaiter().GetResult();
    if (admin == null)
    {
        admin = new Usuario
        {
            UserName = adminEmail,
            Email = adminEmail,
            EmailConfirmed = true,
            Nome = "Administrador Teste",
            CPF = "00000000002",
            Telefone = "0000000000",
            PhoneNumber = "0000000000",
            TipoUsuario = 3,
            Aprovado = true,
            Ativo = true
        };
        userManager.CreateAsync(admin, "Admin123A").GetAwaiter().GetResult();
    }

    var usuarioNormal = userManager.FindByEmailAsync(usuarioEmail).GetAwaiter().GetResult();
    if (usuarioNormal == null)
    {
        usuarioNormal = new Usuario
        {
            UserName = usuarioEmail,
            Email = usuarioEmail,
            EmailConfirmed = true,
            Nome = "Usuário Teste",
            CPF = "00000000003",
            Telefone = "0000000000",
            PhoneNumber = "0000000000",
            TipoUsuario = 1,
            Aprovado = true,
            Ativo = true
        };
        userManager.CreateAsync(usuarioNormal, "Usuario123A").GetAwaiter().GetResult();
    }
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

// Configure CORS
app.UseCors("AllowLocalhost");

app.UseAuthentication();
app.UseAuthorization();

// Configure SignalR Hub
app.MapHub<LeilaoHub>("/hubs/leilao");

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
