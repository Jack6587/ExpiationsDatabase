using Assig1.Models;
using System.ComponentModel.DataAnnotations;

namespace Assig1.ViewModels
{
    public class Offence_OffenceDetail
    {
        [Display(Name = "Offence Code")]
        public string OffenceCode { get; set; }

        [Display(Name = "Description")]
        public string Description { get; set; }
        public IEnumerable<Expiation> Expiations { get; set; }

        [Display(Name = "Total Expiations")]
        [DisplayFormat(DataFormatString = "{0:N0}")]
        public int TotalExpiations { get; set; }

        [Display(Name = "Total Fees Paid")]
        [DisplayFormat(DataFormatString = "${0:N2}")]
        public int? TotalFeePaid { get; set; }

        [Display(Name = "Frequency (%)")]
        [DisplayFormat(DataFormatString = "{0:F2}")]
        public double Frequency { get; set; }
    }
}
