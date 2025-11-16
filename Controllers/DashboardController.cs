using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using Leiloapp.Data;
using Leiloapp.Models.Entities;

namespace Leiloapp.Controllers
{
    public class DashboardController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<Usuario> _userManager;

        public DashboardController(ApplicationDbContext context, UserManager<Usuario> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        public async Task<IActionResult> Index()
        {
            var usuario = await _userManager.GetUserAsync(User);
            if (usuario == null)
                return RedirectToAction("Login", "Account", new { returnUrl = Url.Action("Index", "Dashboard") });

            if (usuario.TipoUsuario == 3)
                return RedirectToAction("Admin");
            if (usuario.TipoUsuario == 2)
                return RedirectToAction("Leiloeiro");

            return RedirectToAction("Index", "Home");
        }

        public class AdminDashboardViewModel
        {
            public int TotalUsuarios { get; set; }
            public int UsuariosPendentes { get; set; }
            public int LeiloesAtivos { get; set; }
            public int LeiloesFinalizados { get; set; }
            public int PagamentosPendentes { get; set; }
            public int PagamentosPagos { get; set; }
            public decimal TotalArrecadado { get; set; }
            public System.Collections.Generic.List<Leilao> TopLeiloes { get; set; }
            public System.Collections.Generic.Dictionary<int, decimal> ArrecadadoPorLeilao { get; set; }
        }

        public async Task<IActionResult> Admin()
        {
            var usuario = await _userManager.GetUserAsync(User);
            if (usuario == null || usuario.TipoUsuario != 3)
                return RedirectToAction("Login", "Account", new { returnUrl = Url.Action("Admin", "Dashboard") });

            var topLeiloes = await _context.Leiloes
                .Include(l => l.Leiloeiro)
                .OrderByDescending(l => l.ValorArrecadado)
                .Take(5)
                .ToListAsync();

            var arrecadadoPorLeilao = await _context.Lotes
                .Where(l => l.Status == 2)
                .GroupBy(l => l.LeilaoId)
                .Select(g => new { LeilaoId = g.Key, Total = g.Sum(x => x.LanceAtual) })
                .ToDictionaryAsync(x => x.LeilaoId, x => x.Total);

            var vm = new AdminDashboardViewModel
            {
                TotalUsuarios = await _context.Users.CountAsync(),
                UsuariosPendentes = await _context.Users.CountAsync(u => !u.Aprovado),
                LeiloesAtivos = await _context.Leiloes.CountAsync(l => l.Status == 0 || l.Status == 1),
                LeiloesFinalizados = await _context.Leiloes.CountAsync(l => l.Status == 2),
                PagamentosPendentes = await _context.Pagamentos.CountAsync(p => p.Status == 0),
                PagamentosPagos = await _context.Pagamentos.CountAsync(p => p.Status == 1),
                TotalArrecadado = await _context.Lotes.Where(l => l.Status == 2).SumAsync(l => l.LanceAtual),
                TopLeiloes = topLeiloes,
                ArrecadadoPorLeilao = arrecadadoPorLeilao
            };

            return View(vm);
        }

        public class LeiloeiroDashboardViewModel
        {
            public System.Collections.Generic.List<Leilao> MeusLeiloes { get; set; }
            public int EmAndamento { get; set; }
            public int Ativos { get; set; }
            public int Finalizados { get; set; }
            public decimal TotalArrecadado { get; set; }
        }

        public async Task<IActionResult> Leiloeiro()
        {
            var usuario = await _userManager.GetUserAsync(User);
            if (usuario == null || usuario.TipoUsuario != 2)
                return RedirectToAction("Login", "Account", new { returnUrl = Url.Action("Leiloeiro", "Dashboard") });

            var meusLeiloes = await _context.Leiloes
                .Include(l => l.Lotes)
                .Where(l => l.LeiloeiroId == usuario.Id)
                .OrderByDescending(l => l.DataInicio)
                .ToListAsync();

            foreach (var le in meusLeiloes)
            {
                le.ValorArrecadado = le.Lotes.Where(x => x.Status == 2).Sum(x => x.LanceAtual);
            }
            await _context.SaveChangesAsync();

            var vm = new LeiloeiroDashboardViewModel
            {
                MeusLeiloes = meusLeiloes,
                EmAndamento = meusLeiloes.Count(l => l.Status == 1),
                Ativos = meusLeiloes.Count(l => l.Status == 0 || l.Status == 1),
                Finalizados = meusLeiloes.Count(l => l.Status == 2),
                TotalArrecadado = meusLeiloes.Sum(l => l.Lotes.Where(x => x.Status == 2).Sum(x => x.LanceAtual))
            };

            return View(vm);
        }
    }
}