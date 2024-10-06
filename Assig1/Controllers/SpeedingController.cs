using Assig1.Data;
using Assig1.Models;
using Assig1.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Assig1.Helpers;

namespace Assig1.Controllers
{
    public class SpeedingController : Controller
    {
        private readonly ExpiationsContext _context;

        public SpeedingController(ExpiationsContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index(SpeedingCategoriesSearchViewModel vm, int page = 1)
        {
            ViewBag.Active = "Speeding";

            var categories = _context.SpeedingCategories
                .OrderBy(sc => sc.SpeedCode)
                .ToList();

            vm.SpeedingCategories = categories
                .GroupBy(sc => sc.SpeedDescription)
                .Select(group => group.First())
                .ToList();

            vm.OffencesBySpeedCode = categories
                .GroupBy(sc => sc.SpeedCode)
                .ToDictionary(group => group.Key, group => group.Count(sc => _context.Offences.Any(o => o.OffenceCode == sc.OffenceCode)));

            if (!string.IsNullOrWhiteSpace(vm.SearchText) || !string.IsNullOrWhiteSpace(vm.SpeedCode) || !string.IsNullOrWhiteSpace(vm.OffenceCode))
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

                if (!string.IsNullOrWhiteSpace(vm.OffenceCode))
                {
                    offencesQuery = offencesQuery
                        .Where(o => o.OffenceCode.Contains(vm.OffenceCode));
                }

                vm.TotalResults = offencesQuery.Count();

                int pageSize = 10;
                vm.Offences = await PaginatedList<Offence>.CreateAsync(offencesQuery.OrderBy(o => o.Description), page, pageSize);
                
                vm.CurrentPage = vm.Offences.PageIndex;
                vm.TotalPages = vm.Offences.TotalPages;
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
        public async Task<IActionResult> SpeedingDetails(string offenceCode)
        {
            var offence = await _context.Offences
                .Where(o => o.OffenceCode == offenceCode)
                .Select(o => new
                {
                    o.OffenceCode,
                    o.Description,
                    o.ExpiationFee,
                    o.DemeritPoints,
                    o.SectionId,
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

            var totalOffencesInSpeedCode = await _context.Offences
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
                SectionID = offence.SectionId,
                AverageFeePaid = averageFee,
                TotalOffencesInSpeedCode = totalOffencesInSpeedCode,
                AverageDemeritPoints = averageDemerit,
                HighestFeePaid = highestFee
            };

            return View(vm);
        }

        [HttpGet]
        public async Task<IActionResult> SpeedCodeBreakdown(string speedCode, string sortOrder = "default")
        {
            var query = await (
                        from o in _context.Offences
                        join e in _context.Expiations on o.OffenceCode equals e.OffenceCode
                        join sc in _context.SpeedingCategories on o.OffenceCode equals sc.OffenceCode
                        where sc.SpeedCode == speedCode
                        group new { o, e, sc } by new { o.OffenceCode, o.Description, sc.SpeedDescription } into g
                        select new SpeedCodeBreakdownViewModel
                        {
                            OffenceCode = g.Key.OffenceCode,
                            Description = g.Key.Description,
                            SpeedDescription = g.Key.SpeedDescription,
                            AverageFeeAmt = g.Average(x => x.e.TotalFeeAmt),
                            OffenceCount = g.Count(),
                            SpeedCode = speedCode
                        }).OrderByDescending(d => d.OffenceCount)
                        .ToListAsync();

            switch (sortOrder)
            {
                case "offence_count_asc":
                    query = query.OrderBy(d => d.OffenceCount).ToList();
                    break;
                case "fee_desc":
                    query = query.OrderByDescending(d => d.AverageFeeAmt).ToList(); ;
                    break;
                case "fee_asc":
                    query = query.OrderBy(d => d.AverageFeeAmt).ToList(); ;
                    break;
                default:
                    query = query.OrderByDescending(d => d.OffenceCount).ToList();
                    break;
            }

            if (query == null)
            {
                return NotFound();
            }

            return View(query);
        }

        [HttpGet]
        public async Task<IActionResult> OffenceDetails(string offenceCode, DateOnly? startDate, DateOnly? endDate)
        {
            var detailQuery = await (
                from o in _context.Offences
                join e in _context.Expiations on o.OffenceCode equals e.OffenceCode
                where o.OffenceCode == offenceCode && (!startDate.HasValue || e.IncidentStartDate >= startDate) && (!endDate.HasValue || e.IncidentStartDate <= endDate)
                select new
                {
                    o.OffenceCode,
                    o.Description,
                    e.ExpId,
                    e.TotalFeeAmt,
                    e.IncidentStartDate,
                    e.IncidentStartTime,
                    e.LsaCode,
                    e.DriverState,
                    e.LocationSpeedLimit
                }).ToListAsync();

            if (detailQuery == null || !detailQuery.Any())
            {
                return NotFound();
            }

            var totalOffenceCount = await _context.Expiations.CountAsync();

            var monthlyExpiations = await _context.Expiations
                .Where(e => e.OffenceCode == offenceCode)
                .GroupBy(e => new { e.IncidentStartDate.Year, e.IncidentStartDate.Month })
                .Select(g => new
                {
                    Year = g.Key.Year,
                    Month = g.Key.Month,
                    Count = g.Count()
                })
                .OrderBy(g => g.Month)
                .ToListAsync();

            var vm = new Offence_OffenceDetail
            {
                OffenceCode = detailQuery.First().OffenceCode,
                Description = detailQuery.First().Description,
                Expiations = detailQuery.Select(d => new Expiation
                {
                   ExpId = d.ExpId,
                   TotalFeeAmt = d.TotalFeeAmt,
                   IncidentStartDate = d.IncidentStartDate,
                   IncidentStartTime = d.IncidentStartTime,
                   LsaCode = d.LsaCode,
                   DriverState = d.DriverState,
                   LocationSpeedLimit = d.LocationSpeedLimit
                }).ToList(),
                MonthlyExpiations = monthlyExpiations
            };

            vm.TotalExpiations = vm.Expiations.Count();
            vm.TotalFeePaid = vm.Expiations.Sum(e => e.TotalFeeAmt ?? 0);
            vm.Frequency = ((double)vm.TotalExpiations / totalOffenceCount) * 100;

            vm.MostCommonLsaCode = detailQuery
                .GroupBy(x => x.LsaCode)
                .OrderByDescending(g => g.Count())
                .Select(x => x.Key)
                .FirstOrDefault();

            vm.MostCommonState = detailQuery
                .GroupBy(x => x.DriverState)
                .OrderByDescending(g => g.Count())
                .Select(x => x.Key)
                .FirstOrDefault();


            return View(vm);
        }

    }
}
