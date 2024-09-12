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

        public IActionResult Index(string searchText, int? selectedCategory)
        {
            ViewBag.Categories = _context.ExpiationCategories
                .OrderBy(ec => ec.CategoryName).ToList();
            ViewBag.SelectedCategoryId = selectedCategory;

            var expiationCategoriesContext = _context.ExpiationCategories
                .AsQueryable();

            if(!string.IsNullOrWhiteSpace(searchText))
            {
                expiationCategoriesContext = expiationCategoriesContext
                    .Where(ec => ec.CategoryName.Contains(searchText));
            }

            if (selectedCategory.HasValue)
            {
                expiationCategoriesContext = expiationCategoriesContext
                    .Where(ec => ec.CategoryId == selectedCategory.Value);
            }

            var expiationCategoriesList = expiationCategoriesContext.ToList();
            ViewBag.ItemCount = expiationCategoriesList.Count;
            ViewBag.SearchText = searchText;

            return View(expiationCategoriesList);
        }
    }
}
