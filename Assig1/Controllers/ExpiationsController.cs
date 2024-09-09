using Assig1.Data;
using Microsoft.AspNetCore.Mvc;

namespace Assig1.Controllers
{
    public class ExpiationsController : Controller
    {
        private readonly ExpiationsContext _context;

        public ExpiationsController(ExpiationsContext context)
        {
            _context = context;
        }

        public IActionResult Index()
        {
            var expiationCategores = _context.ExpiationCategories.ToList();

            return View(expiationCategores);
        }
    }
}
