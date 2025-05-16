namespace BookIt.BLL.Models.Requests;

public class EstablishmentRequest
{
    public string Name { get; set; } = null!;
    public string Address { get; set; } = null!;
    public string Description { get; set; } = null!;
    public double Rating { get; set; }
    public int OwnerId { get; set; }
}