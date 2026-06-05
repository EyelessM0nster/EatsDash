namespace EatsDash.Models;

public class Courier
{
    public int Id { get; set; }
    public string Nickname { get; set; } = string.Empty;
    public bool IsActive { get; set; } = true;

    public ICollection<Review> Reviews { get; set; } = new List<Review>();
}
