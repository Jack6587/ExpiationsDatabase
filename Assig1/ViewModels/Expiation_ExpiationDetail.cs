using Assig1.Models;
using System.ComponentModel.DataAnnotations;

namespace Assig1.ViewModels
{
    public class Expiation_ExpiationDetail
    {
        public Expiation Expiation { get; set; }

        [Display(Name = "Number of Expiations in Driver's State")]
        public int DriverStateCount { get; set; }

        [Display(Name = "Number of Expiations in Same LSA Code Area")]
        public int LsaCount { get; set; }

        [Display(Name = "Percentile of All Speed Expiations")]
        [DisplayFormat(DataFormatString = "{0:F2}")]
        public double SpeedPercentile { get; set; }

        [Display(Name = "Percentile of All BAC Expiations")]
        [DisplayFormat(DataFormatString = "{0:F2}")]
        public double BacPercentile { get; set; }
    }
}
