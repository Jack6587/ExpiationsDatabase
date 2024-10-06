using Assig1.Models;
using Assig1.Helpers;
using System.ComponentModel.DataAnnotations;

namespace Assig1.ViewModels
{
    public class ExpiationsSearchViewModel
    {
        public PaginatedList<Expiation> Expiations { get; set; }
        public Dictionary<string?, int> TotalOffenceCountByState { get; set; } = new Dictionary<string?, int>();

        [Display(Name = "Search by LSA Code")]
        [StringLength(20, ErrorMessage = "LSA Code must not exceed 20 characters.")]
        public string SearchLsaText { get; set; }

        [Display(Name = "Search by Offence Code")]
        [StringLength(20, ErrorMessage = "Offence Code must not exceed 20 characters")]
        public string OffenceCode { get; set; }
        public string SortOrder { get; set; }
        public int TotalExpiations { get; set; }
        public int? MaxSpeed { get; set; }
        public double? AverageSpeed { get; set; }
        public decimal? MaxBAC { get; set; }
        public int? MaxFine { get; set; }
        public double? AverageFine { get; set; }
        public int CurrentPage { get; set; }
        public int TotalPages { get; set; }
    }
}
