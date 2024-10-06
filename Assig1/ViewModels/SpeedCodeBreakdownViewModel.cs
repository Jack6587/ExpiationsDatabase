using System.ComponentModel.DataAnnotations;

namespace Assig1.ViewModels
{
    public class SpeedCodeBreakdownViewModel
    {
        [Display(Name = "Offence Code")]
        public string OffenceCode { get; set; }

        [Display(Name = "Description")]
        public string Description { get; set; }

        [Display(Name = "Speed Type")]
        public string SpeedDescription { get; set; }

        [Display(Name = "Average Fee Amount")]
        public double? AverageFeeAmt { get; set; }

        [Display(Name = "Offence Occurrence")]
        public int OffenceCount { get; set; }

        [Display(Name = "Speed Code")]
        public string SpeedCode { get; set; }
    }
}
