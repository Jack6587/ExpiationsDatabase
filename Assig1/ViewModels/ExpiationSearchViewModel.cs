using Assig1.Models;
using System.ComponentModel.DataAnnotations;

namespace Assig1.ViewModels
{
    public class ExpiationSearchViewModel
    {
        [Required(ErrorMessage = "You must provide a search term")]
        public string SearchText { get; set; }
        public int? SelectedCategoryId { get; set; }
        public IEnumerable<ExpiationCategory> Categories { get; set; }
        public IEnumerable<Expiation> Expiations { get; set; }
    }
}
