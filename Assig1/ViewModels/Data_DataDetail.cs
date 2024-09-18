using Assig1.Models;

namespace Assig1.ViewModels
{
    public class Data_DataDetail
    {
        public string OffenceCode { get; set; }
        public string Description { get; set; }
        public IEnumerable<Expiation> Expiations { get; set; }
    }
}
