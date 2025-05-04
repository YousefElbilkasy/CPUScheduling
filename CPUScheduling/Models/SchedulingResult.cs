namespace CPUScheduling.Models;

public class SchedulingResult
{
  public List<Process> Processes { get; set; } = new List<Process>();
  public List<GanttChartItem> GanttChart { get; set; } = new List<GanttChartItem>();
  public double AverageWaitingTime { get; set; }
  public double AverageTurnaroundTime { get; set; }
  public double AverageResponseTime { get; set; }
  public double CpuUtilization { get; set; }
  public int TotalExecutionTime { get; set; }
}
