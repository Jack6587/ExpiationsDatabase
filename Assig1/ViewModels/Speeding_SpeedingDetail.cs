using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace Assig1.ViewModels
{
    public class Speeding_SpeedingDetail
    {
        [Required]
        [Display(Name = "Offence Code")]
        public string OffenceCode { get; set; }

        [Display(Name = "Description")]
        public string Description { get; set; }

        [Display(Name = "Expiation Fee")]
        public int? ExpiationFee { get; set; }

        [Display(Name = "Demerit Points")]
        public int? DemeritPoints { get; set; }

        [Display(Name = "Total Offences")]
        public int TotalOffences { get; set; }

        [Display(Name = "Average Demerit Points In This Offence")]
        [DisplayFormat(DataFormatString = "{0:F2}")]
        public double? AverageDemeritPoints { get; set; }

        [Display(Name = "Average Fee Paid In This Offence")]
        [DisplayFormat(DataFormatString = "{0:C}")]
        public double? AverageFeePaid { get; set; }

        [Display(Name = "Highest Fee Paid In This Offence")]
        [DisplayFormat(DataFormatString = "{0:C}")]
        public int? HighestFeePaid { get; set; }
    }
}
