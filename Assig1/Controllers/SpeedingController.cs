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
            var categories = _context.SpeedingCategories
                .OrderBy(sc => sc.SpeedCode)
                .ToList();

            vm.SpeedingCategories = categories
                .GroupBy(sc => sc.SpeedDescription)
                .Select(group => group.First())
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

        [HttpGet]
        public async Task<IActionResult> Details(string offenceCode)
        {
            var offence = await _context.Offences
                .Where(o => o.OffenceCode == offenceCode)
                .Select(o => new Speeding_SpeedingDetail
                {
                    OffenceCode = o.OffenceCode,
                    Description = o.Description,
                    ExpiationFee = o.ExpiationFee,
                    DemeritPoints = o.DemeritPoints
                })
                .FirstOrDefaultAsync();

            return View(offence);
        }
    }
}
