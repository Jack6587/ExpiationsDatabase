using Assig1.Data;
using Assig1.ViewModels;
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

        public IActionResult Index(ExpiationSearchViewModel vm)
        {
            vm.Categories = _context.ExpiationCategories
                .OrderBy(c => c.CategoryName)
                .ToList();

            var categoriesQuery = _context.ExpiationCategories
                .AsQueryable();

            if(!string.IsNullOrWhiteSpace(vm.SearchText))
            {
                categoriesQuery = categoriesQuery
                    .Where(ec => ec.CategoryName.Contains(vm.SearchText));
            }

            vm.Categories = categoriesQuery.ToList();

            return View(vm);
        }
    }
}
