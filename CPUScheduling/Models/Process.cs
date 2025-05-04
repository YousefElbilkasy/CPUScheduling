namespace CPUScheduling.Models;

public class Process
{
  public int Id { get; set; }
  public int ProcessId { get; set; } // Process ID entered by user
  public int ArrivalTime { get; set; }
  public int BurstTime { get; set; }
  public int RemainingTime { get; set; }
  public int Priority { get; set; } = 0;
  public int CompletionTime { get; set; } = 0;
  public int TurnaroundTime { get; set; } = 0;
  public int WaitingTime { get; set; } = 0;
  public int ResponseTime { get; set; } = -1; // -1 means not started yet
}
