public class HabitLog
{
  public int Id { get; set; }
  public int HabitId { get; set; }
  public DateTime Date { get; set; }
  public int Quantity { get; set; }
  public string Notes { get; set; } = string.Empty;
}
