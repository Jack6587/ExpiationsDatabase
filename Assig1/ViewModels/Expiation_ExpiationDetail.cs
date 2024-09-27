using Assig1.Models;

namespace Assig1.ViewModels
{
    public class Expiation_ExpiationDetail
    {
        public Expiation Expiation { get; set; }
        public int DriverCount { get; set; }
        public int LsaCount { get; set; }
        public double SpeedPercentile { get; set; }
        public double BacPercentile { get; set; }
    }
}
