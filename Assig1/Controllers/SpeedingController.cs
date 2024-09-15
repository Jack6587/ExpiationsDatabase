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
                .GroupBy(sc => sc.SpeedDescription)
                .Select(group => group.FirstOrDefault())
                .OrderBy(sc => sc.SpeedDescription)
                .ToList();

            var offencesQuery = _context.Offences.AsQueryable();

            if (!string.IsNullOrWhiteSpace(vm.SearchText))
            {
                offencesQuery = offencesQuery
                    .Where(o => o.Description.Contains(vm.SearchText));
            }

            if (!string.IsNullOrWhiteSpace(vm.SpeedCode))
            {
                var offenceCodes = _context.SpeedingCategories
                    .Where(sc => sc.SpeedCode == vm.SpeedCode)
                    .Select(sc => sc.OffenceCode)
                    .ToList();
                
                offencesQuery = offencesQuery
                    .Where(o => offenceCodes.Contains(o.OffenceCode));
            }

            vm.Offences = offencesQuery.ToList();

            return View(vm);
        }
    }
}
