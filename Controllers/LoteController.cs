using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Leiloapp.Data;
using Leiloapp.Models.Entities;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;

namespace Leiloapp.Controllers
{
    public class LoteController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<Usuario> _userManager;

        public LoteController(ApplicationDbContext context, UserManager<Usuario> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        public async Task<IActionResult> Index(int leilaoId)
        {
            var usuario = await _userManager.GetUserAsync(User);
            ViewBag.UsuarioTipo = usuario?.TipoUsuario ?? 0;

            var lotesQuery = _context.Lotes
                .Where(l => l.LeilaoId == leilaoId);

            if (usuario == null || (usuario.TipoUsuario != 2 && usuario.TipoUsuario != 3))
            {
                lotesQuery = lotesQuery.Where(l => l.Visivel);
            }

            var lotes = await lotesQuery
                .OrderBy(l => l.Numero)
                .ToListAsync();

            ViewBag.LeilaoId = leilaoId;
            return View(lotes);
        }

        [Authorize]
        [HttpGet]
        public IActionResult Create(int leilaoId)
        {
            var usuario = _userManager.GetUserAsync(User).GetAwaiter().GetResult();
            if (usuario == null || (usuario.TipoUsuario != 2 && usuario.TipoUsuario != 3)) return Forbid();
            ViewBag.LeilaoId = leilaoId;
            return View(new Lote { LeilaoId = leilaoId });
        }

        [Authorize]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("LeilaoId,Titulo,NomeDoador,Descricao,ValorInicial,ValorMinimo,Visivel")] Lote lote)
        {
            var usuario = await _userManager.GetUserAsync(User);
            if (usuario == null || (usuario.TipoUsuario != 2 && usuario.TipoUsuario != 3)) return Forbid();
            if (!ModelState.IsValid)
            {
                ViewBag.LeilaoId = lote.LeilaoId;
                return View(lote);
            }

            var proximoNumero = await _context.Lotes
                .Where(l => l.LeilaoId == lote.LeilaoId)
                .Select(l => (int?)l.Numero)
                .MaxAsync() ?? 0;

            lote.Numero = proximoNumero + 1;
            lote.Status = 0;

            _context.Lotes.Add(lote);
            await _context.SaveChangesAsync();

            return RedirectToAction("Details", "Leilao", new { id = lote.LeilaoId });
        }

        [Authorize]
        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var usuario = _userManager.GetUserAsync(User).GetAwaiter().GetResult();
            if (usuario == null || (usuario.TipoUsuario != 2 && usuario.TipoUsuario != 3)) return Forbid();
            var lote = await _context.Lotes.FindAsync(id);
            if (lote == null) return NotFound();
            
            ViewBag.LeilaoId = lote.LeilaoId;
            return View(lote);
        }

        [Authorize]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,LeilaoId,Titulo,NomeDoador,Descricao,ValorInicial,ValorMinimo,Visivel")] Lote lote)
        {
            var usuario = await _userManager.GetUserAsync(User);
            if (usuario == null || (usuario.TipoUsuario != 2 && usuario.TipoUsuario != 3)) return Forbid();
            if (id != lote.Id) return BadRequest();

            var original = await _context.Lotes.AsNoTracking().FirstOrDefaultAsync(l => l.Id == id);
            if (original == null) return NotFound();
            

            if (!ModelState.IsValid)
            {
                ViewBag.LeilaoId = lote.LeilaoId;
                return View(lote);
            }

            var entity = await _context.Lotes.FirstOrDefaultAsync(l => l.Id == id);
            if (entity == null) return NotFound();

            entity.Titulo = lote.Titulo;
            entity.NomeDoador = lote.NomeDoador;
            entity.Descricao = lote.Descricao;
            entity.ValorInicial = lote.ValorInicial;
            entity.ValorMinimo = lote.ValorMinimo;
            entity.Visivel = lote.Visivel;
            entity.AtualizadoEm = DateTime.UtcNow;

            await _context.SaveChangesAsync();
            return RedirectToAction("Index", new { leilaoId = entity.LeilaoId });
        }

        [Authorize]
        [HttpGet]
        public async Task<IActionResult> Delete(int id)
        {
            var usuario = _userManager.GetUserAsync(User).GetAwaiter().GetResult();
            if (usuario == null || (usuario.TipoUsuario != 2 && usuario.TipoUsuario != 3)) return Forbid();
            var lote = await _context.Lotes.FindAsync(id);
            if (lote == null) return NotFound();
            
            return View(lote);
        }

        [Authorize]
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var usuario = await _userManager.GetUserAsync(User);
            if (usuario == null || (usuario.TipoUsuario != 2 && usuario.TipoUsuario != 3)) return Forbid();
            var lote = await _context.Lotes.FindAsync(id);
            if (lote == null) return NotFound();
            var leilaoId = lote.LeilaoId;

            _context.Lotes.Remove(lote);
            await _context.SaveChangesAsync();

            var totalArrecadado = await _context.Lotes
                .Where(l => l.LeilaoId == leilaoId && l.Status == 2)
                .SumAsync(l => l.LanceAtual);

            var leilao = await _context.Leiloes.FirstOrDefaultAsync(l => l.Id == leilaoId);
            if (leilao != null)
            {
                leilao.ValorArrecadado = totalArrecadado;
                leilao.AtualizadoEm = DateTime.UtcNow;
                await _context.SaveChangesAsync();
            }

            return RedirectToAction("Index", new { leilaoId });
        }
    }
}