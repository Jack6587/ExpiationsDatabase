namespace Assig1.ViewModels
{
    public class DataBreakdownViewModel
    {
        public string OffenceCode { get; set; }
        public string Description { get; set; }
        public DateOnly IncidentStartDate { get; set; }
        public int? TotalFeeAmt { get; set; }
    }
}
