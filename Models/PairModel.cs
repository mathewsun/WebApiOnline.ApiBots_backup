namespace WebApiOnline.ApiBots.Models;

public class PairModel
{
    public string PairName { get; set; }
    public decimal AmountFrom { get; set; }
    public decimal AmountTo { get; set; }
    public bool IsBuy { get; set; }
}