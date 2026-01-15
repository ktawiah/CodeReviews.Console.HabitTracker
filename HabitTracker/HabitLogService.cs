public class HabitLogService
{
  private readonly HabitLogRepository _repo;

  public HabitLogService(HabitLogRepository repo)
  {
    _repo = repo;
  }

  public void CreateHabitLog(int habitId, DateTime date, int quantity, string notes, List<Habit> habits)
  {
    if (quantity <= 0)
      throw new ArgumentException("Quantity must be greater than zero.");
    if (!habits.Exists(h => h.Id == habitId))
      throw new ArgumentException($"No habit found with Id {habitId}.");
    _repo.CreateHabitLog(habitId, date, quantity, notes ?? "");
  }
  public void UpdateHabitLog(int id, DateTime date, int quantity, string notes, List<Habit> habits)
  {
    if (quantity <= 0)
      throw new ArgumentException("Quantity must be greater than zero.");
    _repo.UpdateHabitLog(id, date, quantity, notes ?? "");
  }

  public void DeleteHabitLog(int id)
  {
    _repo.DeleteHabitLog(id);
  }

  public List<HabitLog> GetAllLogs() => _repo.GetAllHabitLogs();

}
