using Assig1.Models;
using System.ComponentModel.DataAnnotations;

namespace Assig1.ViewModels
{
    public class SpeedingCategoriesSearchViewModel
    {
        [Required(ErrorMessage = "You must provide a search term")]
        public string SearchText { get; set; }
        public int? SelectedCategoryId { get; set; }
        public IEnumerable<SpeedingCategory> SpeedingCategories { get; set; }
        public IEnumerable<Offence> Offences {  get; set; }
    }
}
