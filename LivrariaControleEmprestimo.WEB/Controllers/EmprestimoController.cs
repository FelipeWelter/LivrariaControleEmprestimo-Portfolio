using LivrariaControleEmprestimo.DATA.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;

namespace LivrariaControleEmprestimo.WEB.Controllers;

[AutoValidateAntiforgeryToken]
public class EmprestimoController : Controller
{
    public async Task<IActionResult> Index(bool ativos = false)
    {
        await using var db = new ControleEmprestimoLivroContext();
        var registros = db.LivroClienteEmprestimo.AsNoTracking()
            .Include(e => e.IdLivroNavigation).Include(e => e.IdClienteNavigation).AsQueryable();
        if (ativos) registros = registros.Where(e => e.DataDevolucao == null);
        ViewBag.Ativos = ativos;
        ViewBag.Livros = await db.Livros.AsNoTracking().Where(l =>
            !l.LivroClienteEmprestimo.Any(e => e.DataDevolucao == null))
            .OrderBy(l => l.Nome).ToListAsync();
        ViewBag.Clientes = await db.Cliente.AsNoTracking().OrderBy(c => c.Nome).ToListAsync();
        return View(await registros.OrderByDescending(e => e.Id).ToListAsync());
    }

    [HttpPost]
    public async Task<IActionResult> Registrar(int idLivro, int idCliente)
    {
        await using var db = new ControleEmprestimoLivroContext();
        await using var transacao = await db.Database.BeginTransactionAsync();
        var recurso = $"Livraria:livro:{idLivro}";
        try
        {
            await db.Database.ExecuteSqlInterpolatedAsync($@"
                DECLARE @resultado int;
                EXEC @resultado = sp_getapplock @Resource = {recurso},
                    @LockMode = 'Exclusive', @LockOwner = 'Transaction', @LockTimeout = 5000;
                IF @resultado < 0 THROW 50001, 'Falha ao reservar o livro.', 1;");
        }
        catch (SqlException ex) when (ex.Number == 50001)
        {
            TempData["Aviso"] = "O livro está sendo atualizado. Tente novamente.";
            return RedirectToAction(nameof(Index));
        }

        if (!await db.Livros.AnyAsync(l => l.Id == idLivro)
            || !await db.Cliente.AnyAsync(c => c.Id == idCliente)
            || await db.LivroClienteEmprestimo.AnyAsync(e => e.IdLivro == idLivro && e.DataDevolucao == null))
        {
            TempData["Aviso"] = "Selecione um leitor cadastrado e um livro disponível.";
            return RedirectToAction(nameof(Index));
        }

        db.LivroClienteEmprestimo.Add(new LivroClienteEmprestimo
        {
            IdLivro = idLivro, IdCliente = idCliente, DataEmprestimo = DateTime.UtcNow
        });
        try
        {
            await db.SaveChangesAsync();
            await transacao.CommitAsync();
            TempData["Mensagem"] = "Empréstimo registrado.";
        }
        catch (DbUpdateException)
        {
            TempData["Aviso"] = "Não foi possível registrar o empréstimo. Confira o leitor e o livro e tente novamente.";
        }
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    public async Task<IActionResult> Devolver(int id, bool ativos)
    {
        await using var db = new ControleEmprestimoLivroContext();
        var agora = DateTime.UtcNow;
        var alterados = await db.LivroClienteEmprestimo
            .Where(e => e.Id == id && e.DataDevolucao == null)
            .ExecuteUpdateAsync(e => e.SetProperty(x => x.DataDevolucao, (DateTime?)agora));
        if (alterados == 0) TempData["Aviso"] = "Empréstimo indisponível ou já devolvido.";
        else TempData["Mensagem"] = "Devolução registrada. O livro voltou ao catálogo disponível.";
        return RedirectToAction(nameof(Index), new { ativos });
    }
}
