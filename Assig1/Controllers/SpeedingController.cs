using Assig1.Data;
using Assig1.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Assig1.Controllers
{
    public class SpeedingController : Controller
    {
        private readonly ExpiationsContext _context;

        public SpeedingController(ExpiationsContext context)
        {
            _context = context;
        }

        public IActionResult Index(SpeedingCategoriesSearchViewModel vm)
        {
            vm.SpeedingCategories = _context.SpeedingCategories
                .OrderBy(sc => sc.SpeedDescription)
                .ToList();

            return View(vm);
        }
    }
}
