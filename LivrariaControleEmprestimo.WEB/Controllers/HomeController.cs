using System.Diagnostics;
using LivrariaControleEmprestimo.DATA.Models;
using LivrariaControleEmprestimo.WEB.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace LivrariaControleEmprestimo.WEB.Controllers;

public class HomeController : Controller
{
    public async Task<IActionResult> Index()
    {
        await using var db = new ControleEmprestimoLivroContext();
        ViewBag.Livros = await db.Livros.CountAsync();
        ViewBag.Clientes = await db.Cliente.CountAsync();
        ViewBag.Ativos = await db.LivroClienteEmprestimo.CountAsync(e => e.DataDevolucao == null);
        return View();
    }

    public IActionResult Privacy() => View();

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error() => View(new ErrorViewModel
    {
        RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier
    });
}
