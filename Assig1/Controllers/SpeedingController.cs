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
            ViewBag.Active = "Speeding"; // sets the active tab in the nav bar to be "Speeding"

            var categories = _context.SpeedingCategories // fetch speed categories and order by speed code
                .OrderBy(sc => sc.SpeedCode)
                .ToList();

            vm.SpeedingCategories = categories // groups categories by speed description, ensuring they are unique
                .GroupBy(sc => sc.SpeedDescription)
                .Select(group => group.First())
                .ToList();

            vm.OffencesBySpeedCode = categories // count the total number of offences for each speed code
                .GroupBy(sc => sc.SpeedCode)
                .ToDictionary(group => group.Key, group => group.Count(sc => _context.Offences.Any(o => o.OffenceCode == sc.OffenceCode)));

            // if search filters (user input) is entered, filters the offences
            if (!string.IsNullOrWhiteSpace(vm.SearchText) || !string.IsNullOrWhiteSpace(vm.SpeedCode) || !string.IsNullOrWhiteSpace(vm.OffenceCode))
            {
                var offencesQuery = _context.Offences.AsQueryable();

                if (!string.IsNullOrWhiteSpace(vm.SearchText))
                {
                    offencesQuery = offencesQuery // filter by description if search text (which represents the description) is provided
                        .Where(o => o.Description.Contains(vm.SearchText));
                }

                if (!string.IsNullOrWhiteSpace(vm.SpeedCode))
                {
                    var offenceCodes = _context.SpeedingCategories // filters by speed code (which is a drop down bar in the view)
                        .Where(sc => sc.SpeedCode == vm.SpeedCode)
                        .Select(sc => sc.OffenceCode)
                        .ToList();
                
                    offencesQuery = offencesQuery
                        .Where(o => offenceCodes.Contains(o.OffenceCode));
                }

                if (!string.IsNullOrWhiteSpace(vm.OffenceCode))
                {
                    offencesQuery = offencesQuery // filter for offence code input - ensures that any value can be entered to match, rather than just a full offence code
                        .Where(o => o.OffenceCode.Contains(vm.OffenceCode));
                }

                vm.TotalResults = offencesQuery.Count(); // count all results

                // pagination -> here, only 10 results are displayed per page, hence pageSize = 10
                int pageSize = 10;
                vm.Offences = await PaginatedList<Offence>.CreateAsync(offencesQuery.OrderBy(o => o.Description), page, pageSize);
                
                // update the current page and total pages for the VM
                vm.CurrentPage = vm.Offences.PageIndex;
                vm.TotalPages = vm.Offences.TotalPages;
            }

            return View(vm);
        }

        [HttpGet]
        public async Task<IActionResult> SearchSuggestions(string inputString) // autocorrect
        {
            var suggestions = await _context.Offences // gets offences where the description contains the input string
                .Where(o => o.Description.Contains(inputString))
                .Select(o => o.Description)
                .ToListAsync();

            return Json(suggestions); // displays suggestions as JSON for autocorrect
        }

        [HttpGet]
        public async Task<IActionResult> SpeedingDetails(string offenceCode) // get details for a specific offence
        {
            var offence = await _context.Offences // get the offence details based on the input offence code
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

            if(offence == null) // 404 error if offence not found
            {
                return NotFound();
            }

            // aggregate data below
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

            //create the view model with the queried aggregate data and specific offence details
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

        // a breakdown of all offences belonging to a specific speed code
        [HttpGet]
        public async Task<IActionResult> SpeedCodeBreakdown(string speedCode, string sortOrder = "default") 
        {
            var query = await ( // query that connects offences to expiations, calculating counts and averages from the expiation data set
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

            switch (sortOrder) // sorts query results based on user input from a drop down
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
        public async Task<IActionResult> OffenceDetails(string offenceCode, DateOnly? startDate, DateOnly? endDate) // get details of a specific offence - specific to expiations, which contains details such as date range (which can also be used to filter results)
        {
            var detailQuery = await ( // queries details about an offence, considering a date range
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

            var totalOffenceCount = await _context.Expiations.CountAsync(); // total count of expiations

            var monthlyExpiations = await _context.Expiations // month expiation query for the graph, dividing them by month and counting
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

            // create the view model
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
            vm.TotalFeePaid = vm.Expiations.Sum(e => e.TotalFeeAmt ?? 0); // getting the total fees for this offence (of all fees paid, not just one)
            vm.Frequency = ((double)vm.TotalExpiations / totalOffenceCount) * 100; // calculates the frequency of this offence against all offences

            vm.MostCommonLsaCode = detailQuery // gets the most common LSA code
                .GroupBy(x => x.LsaCode)
                .OrderByDescending(g => g.Count())
                .Select(x => x.Key)
                .FirstOrDefault();

            vm.MostCommonState = detailQuery // gets the most common (driver) state
                .GroupBy(x => x.DriverState)
                .OrderByDescending(g => g.Count())
                .Select(x => x.Key)
                .FirstOrDefault();


            return View(vm);
        }

    }
}
