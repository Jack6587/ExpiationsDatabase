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

        [Display(Name = "Sort Order")]
        public string SortOrder { get; set; }

        [Display(Name = "Total Expiations")]
        public int TotalExpiations { get; set; }

        [Display(Name = "Maximum Speed")]
        [DisplayFormat(DataFormatString = "{0:N0}")]
        public int? MaxSpeed { get; set; }

        [Display(Name = "Average Speed")]
        [DisplayFormat(DataFormatString = "{0:F2}")]
        public double? AverageSpeed { get; set; }

        [Display(Name = "Maximum BAC")]
        [DisplayFormat(DataFormatString = "{0:F3}")]
        public decimal? MaxBAC { get; set; }

        [Display(Name = "Maximum Fine")]
        [DisplayFormat(DataFormatString = "{0:C}")]
        public int? MaxFine { get; set; }

        [Display(Name = "Average Fine")]
        [DisplayFormat(DataFormatString = "{0:C}")]
        public double? AverageFine { get; set; }
        public int CurrentPage { get; set; }
        public int TotalPages { get; set; }
    }
}
