using Assig1.Data;
using Assig1.Models;
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

        public IActionResult Index(string searchLsaText, string offenceCode)
        {
            var vm = new ExpiationsSearchViewModel();

            if (!string.IsNullOrWhiteSpace(searchLsaText) || !string.IsNullOrWhiteSpace(offenceCode))
            {
                var expiationsQuery = _context.Expiations.AsQueryable();

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

                var expiations = expiationsQuery.ToList();

                vm.Expiations = expiations;
                vm.TotalExpiations = expiations.Count;
                vm.MaxSpeed = expiations.Max(e => e.VehicleSpeed);
                vm.AverageSpeed = expiations.Average(e => e.VehicleSpeed);
                vm.MaxBAC = expiations.Max(e => e.BacContentExp);
                vm.MaxFine = expiations.Max(e => e.TotalFeeAmt);
                vm.AverageFine = expiations.Average(e => e.TotalFeeAmt);
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

            var driverCount = _context.Expiations
                .Where(e => e.DriverState == expiation.DriverState)
                .Count();

            var lsaCount = _context.Expiations
                .Where(e => e.LsaCode == expiation.LsaCode)
                .Count();

            var speedCount = _context.Expiations
                .Count(e => e.VehicleSpeed.HasValue);

            var index = _context.Expiations
                .Count(e => e.VehicleSpeed.HasValue && e.VehicleSpeed <= expiation.VehicleSpeed);

            var vm = new Expiation_ExpiationDetail
            {
                Expiation = expiation,
                DriverCount = driverCount,
                LsaCount = lsaCount,
                SpeedPercentile = speedPercentile
            };

            return View(vm);
        }

    }
}
