public class HabitService
{
  private readonly HabitRepository _repo;

  public HabitService(HabitRepository repo)
  {
    _repo = repo;
  }

  public void CreateHabit(string name, string unit)
  {
    if (string.IsNullOrWhiteSpace(name))
      throw new ArgumentException("Habit name cannot be empty.");
    if (string.IsNullOrWhiteSpace(unit))
      throw new ArgumentException("Unit cannot be empty.");
    var existing = _repo.GetAllHabits();
    if (existing.Exists(h => h.Name.Equals(name, StringComparison.OrdinalIgnoreCase)))
      throw new ArgumentException($"A habit named '{name}' already exists.");
    _repo.CreateHabit(name, unit);
  }
  public void UpdateHabit(int id, string newName, string newUnit)
  {
    if (string.IsNullOrWhiteSpace(newName))
      throw new ArgumentException("Habit name cannot be empty.");
    if (string.IsNullOrWhiteSpace(newUnit))
      throw new ArgumentException("Unit cannot be empty.");
    var existing = _repo.GetAllHabits();
    if (!existing.Exists(h => h.Id == id))
      throw new ArgumentException($"No habit found with Id {id}.");
    if (existing.Exists(h => h.Name.Equals(newName, StringComparison.OrdinalIgnoreCase) && h.Id != id))
      throw new ArgumentException($"A habit named '{newName}' already exists.");
    _repo.UpdateHabit(id, newName, newUnit);
  }

  public void DeleteHabit(int id)
  {
    var existing = _repo.GetAllHabits();
    if (!existing.Exists(h => h.Id == id))
      throw new ArgumentException($"No habit found with Id {id}.");
    _repo.DeleteHabit(id);
  }

  public List<Habit> GetAllHabits() => _repo.GetAllHabits();

}
