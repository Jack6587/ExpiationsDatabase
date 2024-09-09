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

        public IActionResult Index(string searchText)
        {
            var expiationCategoriesContext = _context.ExpiationCategories
                .OrderBy(ec => ec.CategoryName)
                .AsQueryable();

            if(!string.IsNullOrWhiteSpace(searchText))
            {
                expiationCategoriesContext = expiationCategoriesContext
                    .Where(ec => ec.CategoryName.Contains(searchText));
            }

            var expiationCategoriesList = expiationCategoriesContext.ToList();
            ViewBag.ItemCount = expiationCategoriesList.Count;
            ViewBag.SearchText = searchText;

            return View(expiationCategoriesContext);
        }
    }
}
