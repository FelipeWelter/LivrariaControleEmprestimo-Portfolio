using LivrariaControleEmprestimo.DATA.Models;
using LivrariaControleEmprestimo.DATA.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace LivrariaControleEmprestimo.WEB.Controllers
{
    
    [AutoValidateAntiforgeryToken]
    public class ClienteController : Controller
    {
        private ClienteService oClienteService = new ClienteService();
        public IActionResult Index()
        {
            List<Cliente> oListClient = oClienteService.oRepositoryCliente.SelecionarTodos();
            return View(oListClient);
        }

        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        public IActionResult Create(Cliente model)
        {
            if (!ModelState.IsValid)
            {
                return View();
            }
            
            oClienteService.oRepositoryCliente.Incluir(model);

            return RedirectToAction("Index");
        }

        public IActionResult Details(int id)
        {
            Cliente oCliente = oClienteService.oRepositoryCliente.SelecionarPK(id);
            return View(oCliente);
        }

        public IActionResult Edit(int id)
        {
            Cliente oCliente = oClienteService.oRepositoryCliente.SelecionarPK(id);
            return View(oCliente);
        }

        [HttpPost]
        public IActionResult Edit(Cliente model)
        {
            Cliente oCliente = oClienteService.oRepositoryCliente.Alterar(model);
            int id = oCliente.Id;
            return RedirectToAction("Details", new { id });
        }

        [HttpPost]
        public async Task<IActionResult> Delete(int id)
        {
            await using var db = new ControleEmprestimoLivroContext();
            var cliente = await db.Cliente.FindAsync(id);
            if (cliente == null) return NotFound();
            if (await db.LivroClienteEmprestimo.AnyAsync(e => e.IdCliente == id))
            {
                TempData["Aviso"] = "O leitor tem histórico de empréstimos e não pode ser excluído.";
                return RedirectToAction(nameof(Index));
            }

            db.Cliente.Remove(cliente);
            try
            {
                await db.SaveChangesAsync();
                TempData["Mensagem"] = "Leitor excluído.";
            }
            catch (DbUpdateException)
            {
                TempData["Aviso"] = "Não foi possível excluir o leitor. Confira se existem empréstimos vinculados.";
            }
            return RedirectToAction(nameof(Index));
        }
    }
}
