using Assig1.Data;
using Assig1.Models;
using Assig1.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
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

        public async Task<IActionResult> Index(string searchLsaText, string offenceCode, int page = 1, string sortOrder = "default")
        {
            ViewBag.Active = "Expiations";

            var vm = new ExpiationsSearchViewModel();

            var expiationsQuery = _context.Expiations.AsQueryable();

            if (!string.IsNullOrWhiteSpace(searchLsaText) || !string.IsNullOrWhiteSpace(offenceCode))
            {

                if (!string.IsNullOrWhiteSpace(searchLsaText))
                {
                    expiationsQuery = expiationsQuery
                        .Where(e => e.LsaCode.Contains(searchLsaText));
                }

                if (!string.IsNullOrWhiteSpace(offenceCode))
                {
                    expiationsQuery = expiationsQuery
                        .Where(e => e.OffenceCode == offenceCode);
                }

                switch (sortOrder)
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

                vm.TotalExpiations = expiationsQuery.Count();
                vm.MaxSpeed = expiationsQuery.Max(e => e.VehicleSpeed);
                vm.AverageSpeed = expiationsQuery.Average(e => e.VehicleSpeed);
                vm.MaxBAC = expiationsQuery.Max(e => e.BacContentExp);
                vm.MaxFine = expiationsQuery.Max(e => e.TotalFeeAmt);
                vm.AverageFine = expiationsQuery.Average(e => e.TotalFeeAmt);
                vm.SortOrder = sortOrder;

                int pageSize = 200;
                var expiations = expiationsQuery
                    .ToPagedList(page, pageSize);

                vm.CurrentPage = page;
                vm.TotalPages = expiations.PageCount;
                vm.Expiations = expiations;
                vm.SearchLsaText = searchLsaText;
                vm.OffenceCode = offenceCode;

                vm.TotalOffenceCountByState = await _context.Expiations
                    .GroupBy(e => e.DriverState)
                    .ToDictionaryAsync(g => g.Key, g => g.Count()); ;
            }
            else
            {
                vm.TotalExpiations = 0;
                vm.Expiations = new PagedList<Expiation>(new List<Expiation>(), page, 1);
            }

            return View(vm);
        }

        public IActionResult Detail(int id)
        {
            var expiation = _context.Expiations
                .Where(e => e.ExpId == id)
                .FirstOrDefault();

            if(expiation == null)
            {
                return NotFound();
            }

            var driverStateCount = _context.Expiations
                .Where(e => e.DriverState == expiation.DriverState)
                .Count();

            var lsaCount = _context.Expiations
                .Where(e => e.LsaCode == expiation.LsaCode)
                .Count();

            var speedCount = _context.Expiations
                .Count(e => e.VehicleSpeed.HasValue);

            var speedIndex = _context.Expiations
                .Count(e => e.VehicleSpeed.HasValue && e.VehicleSpeed <= expiation.VehicleSpeed);

            var speedPercentile = (double)speedIndex / speedCount * 100;

            var bacCount = _context.Expiations
                .Where(e => e.BacContentExp > 0)
                .Count();

            double bacPercentile = 0;

            if(bacCount > 0 && expiation.BacContentExp > 0)
            {
                var bacIndex = _context.Expiations
                    .Count(e => e.BacContentExp > 0 && e.BacContentExp <= expiation.BacContentExp);

                bacPercentile = (double)bacIndex / bacCount * 100;
            } else
            {
                bacPercentile = 0;
            }

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
