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
                .Select(o => new
                {
                    o.OffenceCode,
                    o.Description,
                    o.ExpiationFee,
                    o.DemeritPoints,
                    SpeedCodeCategory = _context.SpeedingCategories
                        .Where(sc => sc.OffenceCode == o.OffenceCode)
                        .Select(sc => sc.SpeedCode)
                        .FirstOrDefault()
                })
                .FirstOrDefaultAsync();

            if(offence == null)
            {
                return NotFound();
            }

            var totalOffences = await _context.Offences
                .Join(_context.SpeedingCategories,
                    o => o.OffenceCode,
                    sc => sc.OffenceCode,
                    (o, sc) => new { o, sc })
                .Where(join => join.sc.SpeedCode == offence.SpeedCodeCategory)
                .CountAsync();

            var averageDemerit = await _context.Offences
                .Join(_context.SpeedingCategories,
                    o => o.OffenceCode,
                    sc => sc.OffenceCode,
                    (o, sc) => new { o, sc })
                .Where(join => join.sc.SpeedCode == offence.SpeedCodeCategory)
                .AverageAsync(join => join.o.DemeritPoints);

            var averageFee = await _context.Offences
                .Join(_context.SpeedingCategories,
                    o => o.OffenceCode,
                    sc => sc.OffenceCode,
                    (o, sc) => new { o, sc })
                .Where(join => join.sc.SpeedCode == offence.SpeedCodeCategory)
                .AverageAsync(join => join.o.TotalFee);

            var highestFee = await _context.Offences
                .Join(_context.SpeedingCategories,
                    o => o.OffenceCode,
                    sc => sc.OffenceCode,
                    (o, sc) => new { o, sc })
                .Where(join => join.sc.SpeedCode == offence.SpeedCodeCategory)
                .MaxAsync(join => join.o.TotalFee);

            var vm = new Speeding_SpeedingDetail
            {
                OffenceCode = offence.OffenceCode,
                Description = offence.Description,
                ExpiationFee = offence.ExpiationFee,
                DemeritPoints = offence.DemeritPoints,
                AverageFeePaid = averageFee,
                TotalOffences = totalOffences,
                AverageDemeritPoints = averageDemerit,
                HighestFeePaid = highestFee
            };

            return View(vm);
        }

        [HttpGet]
        public async Task<IActionResult> DataBreakdown (string offenceCode)
        {
            var query = (
                        from o in _context.Offences
                        join e in _context.Expiations
                        on o.OffenceCode equals e.OffenceCode
                        where o.OffenceCode == offenceCode
                        select new DataBreakdownViewModel
                        {
                            OffenceCode = o.OffenceCode,
                            Description = o.Description,
                            IncidentStartDate = e.IncidentStartDate,
                            TotalFeeAmt = e.TotalFeeAmt
                        }).FirstOrDefaultAsync();

            return View(await query);
        }
    }
}
