using Assig1.Models;
using System.ComponentModel.DataAnnotations;

namespace Assig1.ViewModels
{
    public class SpeedingCategoriesSearchViewModel
    {
        [Required(ErrorMessage = "You must provide a search term")]
        public string SearchText { get; set; }
        public string? SpeedCode { get; set; }
        public List<SpeedingCategory> SpeedingCategories { get; set; }
        public List<Offence> Offences {  get; set; }
    }
}
