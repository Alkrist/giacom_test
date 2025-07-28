namespace Order.Model
{
    public class ProfitSummary
    {
        public int Year { get; set; }
        public int Month { get; set; }
        //public string MonthName => CultureInfo.CurrentCulture.DateTimeFormat.GetMonthName(Month);
        public decimal TotalCost { get; set; }
        public decimal TotalRevenue { get; set; }
        public decimal Profit => TotalRevenue - TotalCost;
        public int OrderCount { get; set; }
    }
}
