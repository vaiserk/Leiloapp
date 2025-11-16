using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Leiloapp.Models;
using Leiloapp.Data;
using Microsoft.EntityFrameworkCore;

namespace Leiloapp.Controllers;

public class HomeController : Controller
{
    private readonly ILogger<HomeController> _logger;
    private readonly ApplicationDbContext _context;

    public HomeController(ILogger<HomeController> logger, ApplicationDbContext context)
    {
        _logger = logger;
        _context = context;
    }

    public async Task<IActionResult> Index()
    {
        var leiloes = await _context.Leiloes
            .Include(l => l.Lotes)
            .Where(l => l.Status == 0 || l.Status == 1)
            .OrderBy(l => l.DataInicio)
            .Take(6)
            .ToListAsync();
        
        return View(leiloes);
    }

    public IActionResult Privacy()
    {
        return View();
    }

    public IActionResult Contato()
    {
        return View();
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}
