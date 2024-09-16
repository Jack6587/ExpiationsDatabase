namespace Assig1.ViewModels
{
    public class Speeding_SpeedingDetail
    {
        public string OffenceCode { get; set; }
        public string Description { get; set; }
        public int? ExpiationFee { get; set; }
        public int? DemeritPoints { get; set; }
        public int TotalOffences { get; set; }
        public double? AverageDemeritPoints { get; set; }
        public double? AverageFeePaid { get; set; }
    }
}
