using Assig1.Data;
using Assig1.Models;
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

        public IActionResult Index(string searchText, string offenceCode)
        {

            var expiationsQuery = _context.Expiations.AsQueryable();

            if (!string.IsNullOrWhiteSpace(searchText) || !string.IsNullOrWhiteSpace(offenceCode))
            {
                if (!string.IsNullOrWhiteSpace(searchText))
                {
                    expiationsQuery = expiationsQuery
                        .Where(e => e.LsaCode.Contains(searchText));
                }

                if (!string.IsNullOrWhiteSpace(offenceCode))
                {
                    expiationsQuery = expiationsQuery
                        .Where(e => e.OffenceCode == offenceCode);
                }
            }

            var expiations = expiationsQuery.ToList();

            return View(expiations);
        }
    }
}
