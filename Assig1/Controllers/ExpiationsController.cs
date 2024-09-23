using Assig1.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Assig1.Controllers
{
    public class ExpiationsController : Controller
    {
        private readonly ExpiationsContext _context;

        public ExpiationsController(ExpiationsContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            ViewBag.Title = "Expiations";
            ViewBag.Active = "Expiations";

            var expiations = await _context.Expiations
                .OrderBy(e => e.TotalFeeAmt)
                .Take(10)
                .ToListAsync();

            return View(expiations);
        }
    }
}
