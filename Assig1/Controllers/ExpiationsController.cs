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
    }
}
