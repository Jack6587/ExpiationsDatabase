using Assig1.Data;
using Assig1.Models;
using Assig1.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using X.PagedList;

namespace Assig1.Controllers
{
    public class SpeedingController : Controller
    {
        private readonly ExpiationsContext _context;

        public SpeedingController(ExpiationsContext context)
        {
            _context = context;
        }

        public IActionResult Index(SpeedingCategoriesSearchViewModel vm, int page = 1)
        {
            var categories = _context.SpeedingCategories
                .OrderBy(sc => sc.SpeedCode)
                .ToList();

            vm.SpeedingCategories = categories
                .GroupBy(sc => sc.SpeedDescription)
                .Select(group => group.First())
                .ToList();

            if(!string.IsNullOrWhiteSpace(vm.SearchText) || !string.IsNullOrWhiteSpace(vm.SpeedCode))
            {
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

                int pageSize = 20;
                var offences = offencesQuery
                    .OrderBy(o => o.Description)
                    .ToPagedList(page, pageSize);

                vm.Offences = offences;
                vm.CurrentPage = page;
                vm.TotalPages = offences.PageCount;
            }

            return View(vm);
        }

        [HttpGet]
        public async Task<IActionResult> SearchSuggestions(string inputString)
        {
            var suggestions = await _context.Offences
                .Where(o => o.Description.Contains(inputString))
                .Select(o => o.Description)
                .ToListAsync();

            return Json(suggestions);
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
        public async Task<IActionResult> DataBreakdown(string speedCode, string sortBy = "OffenceCode")
        {
            var query = await (
                        from o in _context.Offences
                        join e in _context.Expiations on o.OffenceCode equals e.OffenceCode
                        join sc in _context.SpeedingCategories on o.OffenceCode equals sc.OffenceCode
                        where sc.SpeedCode == speedCode
                        group new { o, e, sc } by new { o.OffenceCode, o.Description, sc.SpeedDescription } into g
                        select new DataBreakdownViewModel
                        {
                            OffenceCode = g.Key.OffenceCode,
                            Description = g.Key.Description,
                            SpeedDescription = g.Key.SpeedDescription,
                            AverageFeeAmt = g.Average(x => x.e.TotalFeeAmt),
                            OffenceCount = g.Count()
                        }).OrderByDescending(d => d.OffenceCount)
                        .ToListAsync();

            if(sortBy == "AverageFeeAmt")
            {
                query = query.OrderByDescending(d => d.AverageFeeAmt).ToList();
            } 
            else if (sortBy == "OffenceCount") 
            {
                query = query.OrderByDescending(d => d.OffenceCount).ToList();
            }

            if (query == null)
            {
                return NotFound();
            }

            return View(query);
        }

        [HttpGet]
        public async Task<IActionResult> DataDetails(string offenceCode)
        {
            var detailQuery = await (
                from o in _context.Offences
                join e in _context.Expiations on o.OffenceCode equals e.OffenceCode
                where o.OffenceCode == offenceCode
                select new
                {
                    o.OffenceCode,
                    o.Description,
                    e.ExpId,
                    e.TotalFeeAmt,
                    e.IncidentStartDate,
                    e.IncidentStartTime
                }).ToListAsync();

            if (detailQuery == null || !detailQuery.Any())
            {
                return NotFound();
            }

            var totalOffenceCount = await _context.Expiations.CountAsync();

            var vm = new Offence_OffenceDetail
            {
                OffenceCode = detailQuery.First().OffenceCode,
                Description = detailQuery.First().Description,
                Expiations = detailQuery.Select(d => new Expiation
                {
                   ExpId = d.ExpId,
                   TotalFeeAmt = d.TotalFeeAmt,
                   IncidentStartDate = d.IncidentStartDate,
                   IncidentStartTime = d.IncidentStartTime
                }).ToList()
            };

            var specificOffencesCount = vm.Expiations.Count();
            double offenceFrequency = ((double)specificOffencesCount / totalOffenceCount) * 100;

            vm.Frequency = offenceFrequency;
            vm.TotalExpiations = specificOffencesCount;
            vm.TotalFeePaid = vm.Expiations.Sum(e => e.TotalFeeAmt);

            return View(vm);
        }

    }
}
