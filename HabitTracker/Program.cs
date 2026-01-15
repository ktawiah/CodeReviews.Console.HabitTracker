using Microsoft.Data.SqlClient;

class Program
{
  enum Option
  {
    INSERT_HABIT,
    DELETE_HABIT,
    UPDATE_HABIT,
    VIEW_ALL_HABITS,
    LOG_OCCURRENCE,
    VIEW_LOGS,
    UPDATE_LOG,
    DELETE_LOG,
    CLOSE
  }

  static readonly string INSERT_HABIT_INPUT = "I";
  static readonly string DELETE_HABIT_INPUT = "D";
  static readonly string UPDATE_HABIT_INPUT = "U";
  static readonly string VIEW_ALL_HABITS_INPUT = "V";
  static readonly string LOG_OCCURRENCE_INPUT = "L";
  static readonly string VIEW_LOGS_INPUT = "G";
  static readonly string UPDATE_LOG_INPUT = "E";
  static readonly string DELETE_LOG_INPUT = "X";
  static readonly string CLOSE_INPUT = "C";

  static string? DATABASE_NAME;
  static string? APP_CONNECTION;
  static string? MASTER_CONNECTION;

  static readonly Dictionary<string, (string Name, Option Operation)> operationsMenu = new()
    {
        { INSERT_HABIT_INPUT, ("Insert Habit", Option.INSERT_HABIT) },
        { DELETE_HABIT_INPUT, ("Delete Habit", Option.DELETE_HABIT) },
        { UPDATE_HABIT_INPUT, ("Update Habit", Option.UPDATE_HABIT) },
        { VIEW_ALL_HABITS_INPUT, ("View All Habits", Option.VIEW_ALL_HABITS) },
        { LOG_OCCURRENCE_INPUT, ("Log Occurrence", Option.LOG_OCCURRENCE) },
        { VIEW_LOGS_INPUT, ("View Logs", Option.VIEW_LOGS) },
        { UPDATE_LOG_INPUT, ("Update Log", Option.UPDATE_LOG) },
        { DELETE_LOG_INPUT, ("Delete Log", Option.DELETE_LOG) },
        { CLOSE_INPUT, ("Close", Option.CLOSE) }
    };

  static void Main(string[] args)
  {
    string dotenvPath = FindFileUpward(".env");
    DotNetEnv.Env.Load(dotenvPath);
    DATABASE_NAME = Environment.GetEnvironmentVariable("DATABASE_NAME") ?? "HabitTracker";
    string DATABASE_PASSWORD = Environment.GetEnvironmentVariable("DATABASE_PASSWORD") ?? "";
    string DATABASE_USER = Environment.GetEnvironmentVariable("DATABASE_USER") ?? "";
    string DATABASE_SERVER = Environment.GetEnvironmentVariable("DATABASE_SERVER") ?? "localhost,1433";
    MASTER_CONNECTION = $"Server={DATABASE_SERVER};Database=master;User Id={DATABASE_USER};Password={DATABASE_PASSWORD};TrustServerCertificate=True;";
    APP_CONNECTION = $"Server={DATABASE_SERVER};Database={DATABASE_NAME};User Id={DATABASE_USER};Password={DATABASE_PASSWORD};TrustServerCertificate=True;";

    using (var conn = new SqlConnection(MASTER_CONNECTION))
    {
      conn.Open();
      using var cmd = conn.CreateCommand();
      cmd.CommandText = $"IF DB_ID('{DATABASE_NAME}') IS NULL CREATE DATABASE {DATABASE_NAME};";
      cmd.ExecuteNonQuery();
    }

    InitializeDatabase();

    var habitRepo = new HabitRepository(APP_CONNECTION!);
    var habitLogRepo = new HabitLogRepository(APP_CONNECTION!);
    var habitService = new HabitService(habitRepo);
    var habitLogService = new HabitLogService(habitLogRepo);

    GetUserInput(habitService, habitLogService);
  }

  static void GetUserInput(HabitService habitService, HabitLogService habitLogService)
  {
    string? userInput;
    do
    {
      Console.WriteLine("What operation would you like to perform?");
      foreach (var kvp in operationsMenu)
      {
        Console.WriteLine($"Enter {kvp.Key} to {kvp.Value.Name}");
      }
      userInput = Console.ReadLine()?.ToUpper();
    } while (userInput == null || !operationsMenu.ContainsKey(userInput));

    if (userInput == CLOSE_INPUT)
      return;

    PerformOperation(userInput, habitService, habitLogService);
    GetUserInput(habitService, habitLogService);
  }

  static void PerformOperation(string userInput, HabitService habitService, HabitLogService habitLogService)
  {
    var op = operationsMenu[userInput].Operation;
    try
    {
      switch (op)
      {
        case Option.INSERT_HABIT:
          AddNewHabit(habitService);
          break;
        case Option.DELETE_HABIT:
          DeleteHabit(habitService);
          break;
        case Option.UPDATE_HABIT:
          UpdateHabit(habitService);
          break;
        case Option.VIEW_ALL_HABITS:
          ShowAllHabits(habitService);
          break;
        case Option.LOG_OCCURRENCE:
          LogHabitOccurrence(habitService, habitLogService);
          break;
        case Option.VIEW_LOGS:
          ShowAllLogs(habitLogService, habitService);
          break;
        case Option.UPDATE_LOG:
          UpdateHabitLog(habitLogService, habitService);
          break;
        case Option.DELETE_LOG:
          DeleteHabitLog(habitLogService);
          break;
        default:
          Console.WriteLine("Unknown operation.");
          break;
      }
    }
    catch (Exception ex)
    {
      Console.WriteLine($"Error: {ex.Message}\n");
    }
  }

  static void AddNewHabit(HabitService habitService)
  {
    Console.Write("Enter the name of the new habit: ");
    string? name = Console.ReadLine();
    Console.Write("Enter the unit for this habit (e.g., glasses, minutes): ");
    string? unit = Console.ReadLine();
    habitService.CreateHabit(name ?? "", unit ?? "");
    Console.WriteLine("Habit added successfully.\n");
  }

  static void ShowAllHabits(HabitService habitService)
  {
    var habits = habitService.GetAllHabits();
    if (habits.Count == 0)
    {
      Console.WriteLine("No habits found.\n");
      return;
    }
    Console.WriteLine("\nAll Habits:");
    foreach (var habit in habits)
    {
      Console.WriteLine($"Id: {habit.Id} | Name: {habit.Name} | Unit: {habit.Unit}");
    }
    Console.WriteLine();
  }

  static void UpdateHabit(HabitService habitService)
  {
    ShowAllHabits(habitService);
    Console.Write("Enter the Id of the habit to update: ");
    if (!int.TryParse(Console.ReadLine(), out int id))
    {
      Console.WriteLine("Invalid Id.\n");
      return;
    }
    Console.Write("Enter the new name: ");
    string? newName = Console.ReadLine();
    Console.Write("Enter the new unit: ");
    string? newUnit = Console.ReadLine();
    habitService.UpdateHabit(id, newName ?? "", newUnit ?? "");
    Console.WriteLine("Habit updated successfully.\n");
  }

  static void DeleteHabit(HabitService habitService)
  {
    ShowAllHabits(habitService);
    Console.Write("Enter the Id of the habit to delete: ");
    if (!int.TryParse(Console.ReadLine(), out int id))
    {
      Console.WriteLine("Invalid Id.\n");
      return;
    }
    habitService.DeleteHabit(id);
    Console.WriteLine("Habit deleted successfully.\n");
  }

  static void LogHabitOccurrence(HabitService habitService, HabitLogService habitLogService)
  {
    var habits = habitService.GetAllHabits();
    if (habits.Count == 0)
    {
      Console.WriteLine("No habits found. Add a habit first.\n");
      return;
    }
    ShowAllHabits(habitService);
    Console.Write("Enter the Id of the habit to log: ");
    if (!int.TryParse(Console.ReadLine(), out int habitId))
    {
      Console.WriteLine("Invalid Id.\n");
      return;
    }
    Console.Write("Enter the date (yyyy-MM-dd): ");
    if (!DateTime.TryParse(Console.ReadLine(), out DateTime date))
    {
      Console.WriteLine("Invalid date.\n");
      return;
    }
    Console.Write("Enter the quantity: ");
    if (!int.TryParse(Console.ReadLine(), out int quantity))
    {
      Console.WriteLine("Invalid quantity.\n");
      return;
    }
    Console.Write("Enter notes (optional): ");
    string? notes = Console.ReadLine();
    habitLogService.CreateHabitLog(habitId, date, quantity, notes ?? "", habits);
    Console.WriteLine("Habit occurrence logged successfully.\n");
  }

  static void ShowAllLogs(HabitLogService habitLogService, HabitService habitService)
  {
    var logs = habitLogService.GetAllLogs();
    var habits = habitService.GetAllHabits();
    if (logs.Count == 0)
    {
      Console.WriteLine("No logs found.\n");
      return;
    }
    Console.WriteLine("\nAll Habit Logs:");
    foreach (var log in logs)
    {
      var habit = habits.Find(h => h.Id == log.HabitId);
      string habitName = habit != null ? habit.Name : $"HabitId {log.HabitId}";
      Console.WriteLine($"LogId: {log.Id} | Habit: {habitName} | Date: {log.Date:yyyy-MM-dd} | Qty: {log.Quantity} | Notes: {log.Notes}");
    }
    Console.WriteLine();
  }

  static void UpdateHabitLog(HabitLogService habitLogService, HabitService habitService)
  {
    ShowAllLogs(habitLogService, habitService);
    Console.Write("Enter the Id of the log to update: ");
    if (!int.TryParse(Console.ReadLine(), out int id))
    {
      Console.WriteLine("Invalid Id.\n");
      return;
    }
    Console.Write("Enter the new date (yyyy-MM-dd): ");
    if (!DateTime.TryParse(Console.ReadLine(), out DateTime date))
    {
      Console.WriteLine("Invalid date.\n");
      return;
    }
    Console.Write("Enter the new quantity: ");
    if (!int.TryParse(Console.ReadLine(), out int quantity))
    {
      Console.WriteLine("Invalid quantity.\n");
      return;
    }
    Console.Write("Enter new notes (optional): ");
    string? notes = Console.ReadLine();
    var habits = habitService.GetAllHabits();
    habitLogService.UpdateHabitLog(id, date, quantity, notes ?? "", habits);
    Console.WriteLine("Habit log updated successfully.\n");
  }

  static void DeleteHabitLog(HabitLogService habitLogService)
  {
    Console.Write("Enter the Id of the log to delete: ");
    if (!int.TryParse(Console.ReadLine(), out int id))
    {
      Console.WriteLine("Invalid Id.\n");
      return;
    }
    habitLogService.DeleteHabitLog(id);
    Console.WriteLine("Habit log deleted successfully.\n");
  }
  private static string FindFileUpward(string fileName)
  {
    DirectoryInfo? currentDir = new(Directory.GetCurrentDirectory());

    while (currentDir != null)
    {
      string potentialPath = Path.Combine(currentDir.FullName, fileName);
      if (File.Exists(potentialPath))
      {
        return potentialPath;
      }

      currentDir = currentDir.Parent;
    }

    return fileName;
  }

  static void InitializeDatabase()
  {
    try
    {
      using (var conn = new SqlConnection(MASTER_CONNECTION))
      {
        conn.Open();
        using var cmd = conn.CreateCommand();
        cmd.CommandText = $"IF DB_ID('{DATABASE_NAME}') IS NULL CREATE DATABASE {DATABASE_NAME};";
        cmd.ExecuteNonQuery();
      }

      string schemaPath = FindFileUpward("schema.sql");
      if (!File.Exists(schemaPath))
      {
        throw new FileNotFoundException($"schema.sql not found at {schemaPath}");
      }

      string schemaSql = File.ReadAllText(schemaPath);

      using (var conn = new SqlConnection(APP_CONNECTION))
      {
        conn.Open();
        using var cmd = conn.CreateCommand();
        cmd.CommandText = schemaSql;
        cmd.ExecuteNonQuery();
      }

      Console.WriteLine("Database and tables initialized successfully.");
    }
    catch (Exception ex)
    {
      Console.WriteLine($"Error initializing database: {ex.Message}");
      Environment.Exit(1);
    }
  }
}

