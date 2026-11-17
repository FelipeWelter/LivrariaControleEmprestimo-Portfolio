using LivrariaControleEmprestimo.DATA.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace LivrariaControleEmprestimo.WEB.Controllers;

[AutoValidateAntiforgeryToken]
public class LivroController : Controller
{
    public async Task<IActionResult> Index(string? busca)
    {
        await using var db = new ControleEmprestimoLivroContext();
        var livros = db.Livros.AsNoTracking().AsQueryable();
        if (!string.IsNullOrWhiteSpace(busca))
        {
            var termo = busca.Trim();
            livros = livros.Where(l => l.Nome.Contains(termo) || l.Autor.Contains(termo));
        }

        ViewBag.Busca = busca;
        ViewBag.Emprestados = (await db.LivroClienteEmprestimo
            .Where(e => e.DataDevolucao == null && e.IdLivro.HasValue)
            .Select(e => e.IdLivro!.Value).Distinct().ToListAsync()).ToHashSet();
        return View(await livros.OrderBy(l => l.Nome).ToListAsync());
    }

    public async Task<IActionResult> Edit(int? id)
    {
        if (id == null) return View(new Livros());
        await using var db = new ControleEmprestimoLivroContext();
        var livro = await db.Livros.FindAsync(id);
        if (livro == null) return NotFound();
        return View(livro);
    }

    [HttpPost]
    public async Task<IActionResult> Edit(int? id, [Bind("Nome,Autor,Editora")] Livros dados)
    {
        if (string.IsNullOrWhiteSpace(dados.Nome))
            ModelState.AddModelError(nameof(dados.Nome), "Informe o título.");
        if (string.IsNullOrWhiteSpace(dados.Autor))
            ModelState.AddModelError(nameof(dados.Autor), "Informe o autor.");
        dados.Id = id ?? 0;
        if (!ModelState.IsValid) return View(dados);

        await using var db = new ControleEmprestimoLivroContext();
        var livro = id == null ? new Livros() : await db.Livros.FindAsync(id);
        if (livro == null) return NotFound();
        livro.Nome = dados.Nome.Trim();
        livro.Autor = dados.Autor.Trim();
        livro.Editora = dados.Editora?.Trim();
        if (id == null) db.Livros.Add(livro);
        await db.SaveChangesAsync();
        TempData["Mensagem"] = "Livro salvo no catálogo.";
        return RedirectToAction(nameof(Index));
    }
}
