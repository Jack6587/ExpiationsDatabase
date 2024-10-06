using Assig1.Helpers;
using Assig1.Models;
using System.ComponentModel.DataAnnotations;

namespace Assig1.ViewModels
{
    public class SpeedingCategoriesSearchViewModel
    {
        [Display(Name = "Search by Offence Description:")]
        [StringLength(100, ErrorMessage = "Search term cannot exceed 100 characters")]
        public string SearchText { get; set; }

        [Display(Name = "Filter by Speed Code")]
        public string? SpeedCode { get; set; }
        public List<SpeedingCategory> SpeedingCategories { get; set; }
        public PaginatedList<Offence> Offences { get; set; }
        public int CurrentPage { get; set; }
        public int TotalPages { get; set; }

        [Display(Name = "Search by Offence Code:")]
        public string OffenceCode { get; set; }
        public Dictionary<string, int> OffencesBySpeedCode { get; set; } = new Dictionary<string, int>();
        public int TotalResults { get; set; }
    }
}