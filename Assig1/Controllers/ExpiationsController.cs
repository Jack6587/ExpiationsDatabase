using Assig1.Data;
using Assig1.Models;
using Assig1.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Assig1.Helpers;
using X.PagedList;

namespace Assig1.Controllers
{
    public class ExpiationsController : Controller
    {
        private readonly ExpiationsContext _context;

        public ExpiationsController(ExpiationsContext context)
        {
            _context = context;
        }

        // search and filtering for expiations
        public async Task<IActionResult> Index(string searchLsaText, string offenceCode, int page = 1, string sortOrder = "default")
        {
            ViewBag.Active = "Expiations"; // sets the active nav bar link to Expiations

            var vm = new ExpiationsSearchViewModel();

            var expiationsQuery = _context.Expiations.AsQueryable();

            vm.TotalOffenceCountByState = await _context.Expiations // group by driver state to get no. of expiations per state
                .GroupBy(e => e.DriverState)
                .ToDictionaryAsync(g => g.Key, g => g.Count());

            if (!string.IsNullOrWhiteSpace(searchLsaText) || !string.IsNullOrWhiteSpace(offenceCode)) // search filtering
            {

                if (!string.IsNullOrWhiteSpace(searchLsaText))
                {
                    expiationsQuery = expiationsQuery
                        .Where(e => e.LsaCode.Contains(searchLsaText)); // using LSA code
                }

                if (!string.IsNullOrWhiteSpace(offenceCode))
                {
                    expiationsQuery = expiationsQuery
                        .Where(e => e.OffenceCode == offenceCode); // using Offence Code
                }

                switch (sortOrder) // sorting expiations based on user input from dropdown. Options include LSA ascending and descending
                {
                    case "lsa_asc":
                        expiationsQuery = expiationsQuery.OrderBy(e => e.LsaCode);
                        break;
                    case "lsa_desc":
                        expiationsQuery = expiationsQuery.OrderByDescending(e => e.LsaCode);
                        break;
                    case "time_asc":
                        expiationsQuery = expiationsQuery.OrderBy(e => e.IncidentStartTime);
                        break;
                    case "time_desc":
                        expiationsQuery = expiationsQuery.OrderByDescending(e => e.IncidentStartTime);
                        break;
                    case "bac_desc":
                        expiationsQuery = expiationsQuery.OrderByDescending(e => e.BacContentExp);
                        break;
                    default:
                        expiationsQuery = expiationsQuery.OrderBy(e => e.ExpId);
                        break;
                }

                // aggregate and summary calculations for display
                vm.TotalExpiations = expiationsQuery.Count();
                vm.MaxSpeed = expiationsQuery.Max(e => e.VehicleSpeed);
                vm.AverageSpeed = expiationsQuery.Average(e => e.VehicleSpeed);
                vm.MaxBAC = expiationsQuery.Max(e => e.BacContentExp);
                vm.MaxFine = expiationsQuery.Max(e => e.TotalFeeAmt);
                vm.AverageFine = expiationsQuery.Average(e => e.TotalFeeAmt);
                vm.SortOrder = sortOrder; // stores sort order in the VM

                // pagination
                int pageSize = 200;
                vm.Expiations = await PaginatedList<Expiation>.CreateAsync(expiationsQuery, page, pageSize);
                vm.CurrentPage = page;
                vm.TotalPages = vm.Expiations.TotalPages;
                vm.SearchLsaText = searchLsaText?.ToUpper();
                vm.OffenceCode = offenceCode?.ToUpper();
            }
            else
            {
                // if no search criteria
                vm.TotalExpiations = 0;
                vm.Expiations = new PaginatedList<Expiation>(new List<Expiation>(), page, 1, 0); // creates empty PaginatedList
            }

            return View(vm);
        }

        public IActionResult Detail(int id) // detail action for a specific expiation
        {
            var expiation = _context.Expiations // get expiations by its expiation ID
                .Where(e => e.ExpId == id)
                .FirstOrDefault();

            if(expiation == null)
            {
                return NotFound();
            }

            // aggregates 
            var driverStateCount = _context.Expiations // get number of expiations from the same state
                .Where(e => e.DriverState == expiation.DriverState)
                .Count();

            var lsaCount = _context.Expiations
                .Where(e => e.LsaCode == expiation.LsaCode)
                .Count();

            var speedCount = _context.Expiations
                .Count(e => e.VehicleSpeed.HasValue);

            var speedIndex = _context.Expiations
                .Count(e => e.VehicleSpeed.HasValue && e.VehicleSpeed <= expiation.VehicleSpeed);

            var speedPercentile = (double)speedIndex / speedCount * 100; // calculate percentile among all speeds from a speed index and total count

            var bacCount = _context.Expiations
                .Where(e => e.BacContentExp > 0)
                .Count();

            double bacPercentile = 0;

            if(bacCount > 0 && expiation.BacContentExp > 0)
            {
                var bacIndex = _context.Expiations
                    .Count(e => e.BacContentExp > 0 && e.BacContentExp <= expiation.BacContentExp);

                bacPercentile = (double)bacIndex / bacCount * 100; // bac percentile calculation works similarly
            } else
            {
                bacPercentile = 0; // otherwise, bac percentile defaults to 0
            }

            // instantiate the detail model with specific details
            var vm = new Expiation_ExpiationDetail
            {
                Expiation = expiation,
                DriverStateCount = driverStateCount,
                LsaCount = lsaCount,
                SpeedPercentile = speedPercentile,
                BacPercentile = bacPercentile
            };

            return View(vm);
        }

    }
}
