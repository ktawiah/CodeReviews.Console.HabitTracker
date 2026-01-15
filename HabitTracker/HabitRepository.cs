using Microsoft.Data.SqlClient;

public class HabitRepository(string connectionString)
{
  private readonly string _connectionString = connectionString;

  public List<Habit> GetAllHabits()
  {
    var habits = new List<Habit>();
    using (var conn = new SqlConnection(_connectionString))
    {
      conn.Open();
      using var cmd = conn.CreateCommand();
      cmd.CommandText = "SELECT Id, Name, Unit FROM Habits ORDER BY Id;";
      using var reader = cmd.ExecuteReader();
      while (reader.Read())
      {
        habits.Add(new Habit
        {
          Id = reader.GetInt32(0),
          Name = reader.GetString(1),
          Unit = reader.GetString(2)
        });
      }
    }
    return habits;
  }

  public void CreateHabit(string name, string unit)
  {
    using var conn = new SqlConnection(_connectionString);
    conn.Open();
    using var cmd = conn.CreateCommand();
    cmd.CommandText = "INSERT INTO Habits (Name, Unit) VALUES (@name, @unit);";
    cmd.Parameters.AddWithValue("@name", name);
    cmd.Parameters.AddWithValue("@unit", unit);
    cmd.ExecuteNonQuery();
  }

  public void UpdateHabit(int id, string newName, string newUnit)
  {
    using (var conn = new SqlConnection(_connectionString))
    {
      conn.Open();
      using var cmd = conn.CreateCommand();
      cmd.CommandText = "UPDATE Habits SET Name = @name, Unit = @unit WHERE Id = @id;";
      cmd.Parameters.AddWithValue("@id", id);
      cmd.Parameters.AddWithValue("@name", newName);
      cmd.Parameters.AddWithValue("@unit", newUnit);
      cmd.ExecuteNonQuery();
    }
  }

  public void DeleteHabit(int id)
  {
    using (var conn = new SqlConnection(_connectionString))
    {
      conn.Open();
      using var cmd = conn.CreateCommand();
      cmd.CommandText = "DELETE FROM Habits WHERE Id = @id;";
      cmd.Parameters.AddWithValue("@id", id);
      cmd.ExecuteNonQuery();
    }
  }
}
