using Microsoft.Data.SqlClient;

public class HabitLogRepository
{
  private readonly string _connectionString;

  public HabitLogRepository(string connectionString)
  {
    _connectionString = connectionString;
  }

  public List<HabitLog> GetAllHabitLogs()
  {
    var habitLogs = new List<HabitLog>();
    using (var conn = new SqlConnection(_connectionString))
    {
      conn.Open();
      using var cmd = conn.CreateCommand();
      cmd.CommandText = "SELECT Id, HabitId, [Date], Quantity, Notes FROM HabitLogs ORDER BY Id;";
      using var reader = cmd.ExecuteReader();
      while (reader.Read())
      {
        habitLogs.Add(new HabitLog
        {
          Id = reader.GetInt32(0),
          HabitId = reader.GetInt32(1),
          Date = reader.GetDateTime(2),
          Quantity = reader.GetInt32(3),
          Notes = reader.IsDBNull(4) ? string.Empty : reader.GetString(4)
        });
      }
    }
    return habitLogs;
  }

  public List<HabitLog> GetLogsByHabitId(int habitId)
  {
    var logs = new List<HabitLog>();
    using (var conn = new SqlConnection(_connectionString))
    {
      conn.Open();
      using var cmd = conn.CreateCommand();
      cmd.CommandText = "SELECT Id, HabitId, [Date], Quantity, Notes FROM HabitLogs WHERE HabitId = @habitId ORDER BY [Date];";
      cmd.Parameters.AddWithValue("@habitId", habitId);
      using var reader = cmd.ExecuteReader();
      while (reader.Read())
      {
        logs.Add(new HabitLog
        {
          Id = reader.GetInt32(0),
          HabitId = reader.GetInt32(1),
          Date = reader.GetDateTime(2),
          Quantity = reader.GetInt32(3),
          Notes = reader.IsDBNull(4) ? string.Empty : reader.GetString(4)
        });
      }
    }
    return logs;
  }

  public void CreateHabitLog(int habitId, DateTime date, int quantity, string notes)
  {
    using (var conn = new SqlConnection(_connectionString))
    {
      conn.Open();
      using var cmd = conn.CreateCommand();
      cmd.CommandText = "INSERT INTO HabitLogs (HabitId, [Date], Quantity, Notes) VALUES (@habitId, @date, @quantity, @notes);";
      cmd.Parameters.AddWithValue("@habitId", habitId);
      cmd.Parameters.AddWithValue("@date", date);
      cmd.Parameters.AddWithValue("@quantity", quantity);
      cmd.Parameters.AddWithValue("@notes", notes);
      cmd.ExecuteNonQuery();
    }
  }

  public void UpdateHabitLog(int id, DateTime date, int quantity, string notes)
  {
    using (var conn = new SqlConnection(_connectionString))
    {
      conn.Open();
      using var cmd = conn.CreateCommand();
      cmd.CommandText = "UPDATE HabitLogs SET [Date] = @date, Quantity = @quantity, Notes = @notes WHERE Id = @id;";
      cmd.Parameters.AddWithValue("@id", id);
      cmd.Parameters.AddWithValue("@date", date);
      cmd.Parameters.AddWithValue("@quantity", quantity);
      cmd.Parameters.AddWithValue("@notes", notes);
      cmd.ExecuteNonQuery();
    }
  }

  public void DeleteHabitLog(int id)
  {
    using (var conn = new SqlConnection(_connectionString))
    {
      conn.Open();
      using var cmd = conn.CreateCommand();
      cmd.CommandText = "DELETE FROM HabitLogs WHERE Id = @id;";
      cmd.Parameters.AddWithValue("@id", id);
      cmd.ExecuteNonQuery();
    }
  }
}
