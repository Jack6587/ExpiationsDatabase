using Assig1.Models;

namespace Assig1.ViewModels
{
    public class ExpiationsSearchViewModel
    {
        public List<Expiation> Expiations { get; set; } = new List<Expiation>();

        public int TotalExpiations { get; set; }
        public int? MaxSpeed { get; set; }
        public double? AverageSpeed { get; set; }
        public decimal MaxBAC { get; set; }
        public int? MaxFine { get; set; }
        public double? AverageFine { get; set; }
    }
}
