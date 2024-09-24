using Assig1.Models;

namespace Assig1.ViewModels
{
    public class Offence_OffenceDetail
    {
        public string OffenceCode { get; set; }
        public string Description { get; set; }
        public IEnumerable<Expiation> Expiations { get; set; }
        public int TotalExpiations { get; set; }
        public int? TotalFeePaid { get; set; }
        public double Frequency { get; set; }
    }
}
