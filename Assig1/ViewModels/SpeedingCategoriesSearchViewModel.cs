using Assig1.Models;
using System.ComponentModel.DataAnnotations;
using X.PagedList;

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
        public IPagedList<Offence> Offences { get; set; }
        public int CurrentPage { get; set; }
        public int TotalPages { get; set; }
    }
}