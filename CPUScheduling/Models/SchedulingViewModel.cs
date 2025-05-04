namespace CPUScheduling.Models;

public class SchedulingViewModel
{
  public List<Process> Processes { get; set; } = new List<Process>();
  public SchedulingAlgorithm SelectedAlgorithm { get; set; } = SchedulingAlgorithm.FCFS;
  public int TimeQuantum { get; set; } = 2; // Default time quantum for Round Robin
  public SchedulingResult? Result { get; set; }
}
