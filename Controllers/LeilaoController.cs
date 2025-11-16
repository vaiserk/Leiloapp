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
    public class LeilaoController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<Usuario> _userManager;

        public LeilaoController(ApplicationDbContext context, UserManager<Usuario> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        // GET: Leilao
        public async Task<IActionResult> Index()
        {
            var leiloes = await _context.Leiloes
                .Include(l => l.Leiloeiro)
                .Include(l => l.Lotes)
                .ToListAsync();
            var usuario = await _userManager.GetUserAsync(User);
            ViewBag.UsuarioTipo = usuario?.TipoUsuario ?? 0;
            return View(leiloes);
        }

        // GET: Leilao/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var leilao = await _context.Leiloes
                .Include(l => l.Leiloeiro)
                .Include(l => l.Lotes)
                    .ThenInclude(lote => lote.Imagens)
                .Include(l => l.Lotes)
                    .ThenInclude(lote => lote.LanceVencedor)
                .FirstOrDefaultAsync(m => m.Id == id);
            
            if (leilao == null)
            {
                return NotFound();
            }

            var usuario = await _userManager.GetUserAsync(User);
            var tipo = usuario?.TipoUsuario ?? 0;
            ViewBag.UsuarioTipo = tipo;

            // Recalcular arrecadação sempre ao carregar detalhes
            var totalArrecadado = leilao.Lotes.Where(l => l.Status == 2).Sum(l => l.LanceAtual);
            leilao.ValorArrecadado = totalArrecadado;
            await _context.SaveChangesAsync();

            if (tipo != 2 && tipo != 3)
            {
                leilao.Lotes = leilao.Lotes.Where(l => l.Visivel).ToList();
            }

            return View(leilao);
        }

        // GET: Leilao/Create
        [Authorize]
        public IActionResult Create()
        {
            var usuario = _userManager.GetUserAsync(User).GetAwaiter().GetResult();
            if (usuario == null || (usuario.TipoUsuario != 2 && usuario.TipoUsuario != 3)) return Forbid();
            return View();
        }

        // POST: Leilao/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize]
        public async Task<IActionResult> Create([Bind("Titulo,Descricao,DataInicio,DataFim,Local,Status")] Leilao leilao)
        {
            ModelState.Remove("LeiloeiroId");
            ModelState.Remove("Leiloeiro");
            ModelState.Remove("ImagemCapa");
            if (leilao.DataInicio == default)
            {
                leilao.DataInicio = DateTime.UtcNow;
                ModelState.Remove(nameof(leilao.DataInicio));
            }

            if (leilao.DataFim == default || leilao.DataFim <= leilao.DataInicio)
            {
                leilao.DataFim = leilao.DataInicio.AddHours(2);
                ModelState.Remove(nameof(leilao.DataFim));
            }

            leilao.DataInicio = ToUtc(leilao.DataInicio);
            leilao.DataFim = ToUtc(leilao.DataFim);

            if (leilao.Status < 0 || leilao.Status > 3)
            {
                leilao.Status = 0;
                ModelState.Remove(nameof(leilao.Status));
            }

            var usuario = await _userManager.GetUserAsync(User);
            if (usuario == null)
            {
                return Unauthorized();
            }
            if (usuario.TipoUsuario != 2 && usuario.TipoUsuario != 3)
            {
                return Forbid();
            }
            leilao.LeiloeiroId = usuario.Id;
            leilao.CriadoEm = DateTime.UtcNow;
            leilao.AtualizadoEm = DateTime.UtcNow;

            leilao.ImagemCapa ??= string.Empty;

            TryValidateModel(leilao);

            if (!ModelState.IsValid)
            {
                return View(leilao);
            }

            _context.Add(leilao);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        [Authorize]
        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var usuario = await _userManager.GetUserAsync(User);
            if (usuario == null || (usuario.TipoUsuario != 2 && usuario.TipoUsuario != 3)) return Forbid();

            var leilao = await _context.Leiloes.FindAsync(id);
            if (leilao == null) return NotFound();

            return View(leilao);
        }

        [Authorize]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Titulo,Descricao,DataInicio,DataFim,Local,Status")] Leilao leilao)
        {
            var usuario = await _userManager.GetUserAsync(User);
            if (usuario == null || (usuario.TipoUsuario != 2 && usuario.TipoUsuario != 3)) return Forbid();
            if (id != leilao.Id) return BadRequest();

            leilao.DataInicio = ToUtc(leilao.DataInicio);
            leilao.DataFim = ToUtc(leilao.DataFim);
            leilao.AtualizadoEm = DateTime.UtcNow;

            if (!ModelState.IsValid)
            {
                return View(leilao);
            }

            var entity = await _context.Leiloes.FirstOrDefaultAsync(l => l.Id == id);
            if (entity == null) return NotFound();

            entity.Titulo = leilao.Titulo;
            entity.Descricao = leilao.Descricao;
            entity.DataInicio = leilao.DataInicio;
            entity.DataFim = leilao.DataFim;
            entity.Local = leilao.Local;
            entity.Status = leilao.Status;
            entity.AtualizadoEm = DateTime.UtcNow;

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Details), new { id });
        }

        [Authorize]
        [HttpGet]
        public async Task<IActionResult> Delete(int id)
        {
            var usuario = await _userManager.GetUserAsync(User);
            if (usuario == null || (usuario.TipoUsuario != 2 && usuario.TipoUsuario != 3)) return Forbid();

            var leilao = await _context.Leiloes.Include(l => l.Lotes).FirstOrDefaultAsync(l => l.Id == id);
            if (leilao == null) return NotFound();

            return View(leilao);
        }

        [Authorize]
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var usuario = await _userManager.GetUserAsync(User);
            if (usuario == null || (usuario.TipoUsuario != 2 && usuario.TipoUsuario != 3)) return Forbid();

            var leilao = await _context.Leiloes.Include(l => l.Lotes).FirstOrDefaultAsync(l => l.Id == id);
            if (leilao == null) return NotFound();

            _context.Leiloes.Remove(leilao);
            await _context.SaveChangesAsync();
            TempData["Success"] = "Leilão excluído com sucesso.";
            return RedirectToAction(nameof(Index));
        }
        // GET: Leilao/SalaLeilao/5
        public async Task<IActionResult> SalaLeilao(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var lote = await _context.Lotes
                .Include(l => l.Lances)
                    .ThenInclude(lance => lance.Usuario)
                .FirstOrDefaultAsync(m => m.Id == id);
            
            if (lote == null)
            {
                return NotFound();
            }

            return View(lote);
        }

        private static DateTime ToUtc(DateTime dt)
        {
            if (dt.Kind == DateTimeKind.Utc) return dt;
            if (dt.Kind == DateTimeKind.Unspecified) return DateTime.SpecifyKind(dt, DateTimeKind.Local).ToUniversalTime();
            return dt.ToUniversalTime();
        }
    }
}