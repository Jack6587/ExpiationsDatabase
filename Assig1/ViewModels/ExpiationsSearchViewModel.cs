using Assig1.Models;
using X.PagedList;

namespace Assig1.ViewModels
{
    public class ExpiationsSearchViewModel
    {
        public IPagedList<Expiation> Expiations { get; set; }
        public Dictionary<string?, int> TotalOffenceCountByState { get; set; } = new Dictionary<string?, int>();
        public string SearchLsaText { get; set; }
        public string OffenceCode { get; set; }
        public string SortOrder { get; set; }
        public int TotalExpiations { get; set; }
        public int? MaxSpeed { get; set; }
        public double? AverageSpeed { get; set; }
        public decimal? MaxBAC { get; set; }
        public int? MaxFine { get; set; }
        public double? AverageFine { get; set; }
        public int CurrentPage { get; set; }
        public int TotalPages { get; set; }
    }
}
