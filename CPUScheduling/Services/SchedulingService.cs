using CPUScheduling.Models;

namespace CPUScheduling.Services;

public class SchedulingService
{
  public SchedulingResult Schedule(List<Process> processes, SchedulingAlgorithm algorithm, int timeQuantum = 0)
  {
    // Create a copy of the processes to avoid modifying the original
    var processesToSchedule = processes.Select(p => new Process
    {
      Id = p.Id,
      ProcessId = p.ProcessId,
      ArrivalTime = p.ArrivalTime,
      BurstTime = p.BurstTime,
      RemainingTime = p.BurstTime,
      Priority = p.Priority
    }).ToList();

    SchedulingResult result = algorithm switch
    {
      SchedulingAlgorithm.FCFS => FCFS(processesToSchedule),
      SchedulingAlgorithm.SJF => SJF(processesToSchedule),
      SchedulingAlgorithm.SRT => SRT(processesToSchedule),
      SchedulingAlgorithm.Priority => PriorityScheduling(processesToSchedule),
      SchedulingAlgorithm.RoundRobin => RoundRobin(processesToSchedule, timeQuantum),
      _ => FCFS(processesToSchedule) // Default to FCFS
    };

    // Calculate statistics
    result.AverageWaitingTime = result.Processes.Average(p => p.WaitingTime);
    result.AverageTurnaroundTime = result.Processes.Average(p => p.TurnaroundTime);
    result.AverageResponseTime = result.Processes.Average(p => p.ResponseTime);

    if (result.GanttChart.Any())
    {
      result.TotalExecutionTime = result.GanttChart.Max(g => g.EndTime);

      // Calculate idle time
      int totalBurstTime = processes.Sum(p => p.BurstTime);
      int idleTime = result.TotalExecutionTime - totalBurstTime;

      // CPU Utilization = (Total Burst Time / Total Execution Time) * 100
      result.CpuUtilization = (double)totalBurstTime / result.TotalExecutionTime * 100;
    }

    return result;
  }

  private SchedulingResult FCFS(List<Process> processes)
  {
    var result = new SchedulingResult();
    var ganttChart = new List<GanttChartItem>();

    // Sort by arrival time
    processes = processes.OrderBy(p => p.ArrivalTime).ToList();

    int currentTime = 0;

    foreach (var process in processes)
    {
      // If there's a gap between the current time and arrival time
      if (currentTime < process.ArrivalTime)
      {
        // Add idle time in gantt chart
        ganttChart.Add(new GanttChartItem
        {
          ProcessId = -1, // -1 to indicate idle time
          StartTime = currentTime,
          EndTime = process.ArrivalTime
        });

        currentTime = process.ArrivalTime;
      }

      // Set response time when the process starts execution for the first time
      process.ResponseTime = currentTime - process.ArrivalTime;

      // Add process to gantt chart
      ganttChart.Add(new GanttChartItem
      {
        ProcessId = process.ProcessId,
        StartTime = currentTime,
        EndTime = currentTime + process.BurstTime
      });

      // Update the current time
      currentTime += process.BurstTime;

      // Set completion time
      process.CompletionTime = currentTime;

      // Calculate turnaround time and waiting time
      process.TurnaroundTime = process.CompletionTime - process.ArrivalTime;
      process.WaitingTime = process.TurnaroundTime - process.BurstTime;
    }

    result.Processes = processes;
    result.GanttChart = ganttChart;

    return result;
  }

  private SchedulingResult SJF(List<Process> processes)
  {
    var result = new SchedulingResult();
    var ganttChart = new List<GanttChartItem>();

    // Create a copy of processes to work with
    var remainingProcesses = processes.Select(p => new Process
    {
      Id = p.Id,
      ProcessId = p.ProcessId,
      ArrivalTime = p.ArrivalTime,
      BurstTime = p.BurstTime,
      RemainingTime = p.BurstTime,
      Priority = p.Priority
    }).ToList();

    int currentTime = 0;

    while (remainingProcesses.Any())
    {
      // Get the processes that have arrived by current time
      var availableProcesses = remainingProcesses.Where(p => p.ArrivalTime <= currentTime).ToList();

      if (!availableProcesses.Any())
      {
        // No processes available, advance time to the next arrival
        int nextArrival = remainingProcesses.Min(p => p.ArrivalTime);

        // Add idle time to gantt chart
        ganttChart.Add(new GanttChartItem
        {
          ProcessId = -1, // -1 indicates CPU idle
          StartTime = currentTime,
          EndTime = nextArrival
        });

        currentTime = nextArrival;
        continue;
      }

      // Select process with shortest burst time
      var selectedProcess = availableProcesses.OrderBy(p => p.BurstTime).First();

      // Set response time when the process starts execution for the first time
      selectedProcess.ResponseTime = currentTime - selectedProcess.ArrivalTime;

      // Add to gantt chart
      ganttChart.Add(new GanttChartItem
      {
        ProcessId = selectedProcess.ProcessId,
        StartTime = currentTime,
        EndTime = currentTime + selectedProcess.BurstTime
      });

      // Update current time
      currentTime += selectedProcess.BurstTime;

      // Set completion time
      selectedProcess.CompletionTime = currentTime;

      // Calculate turnaround time and waiting time
      selectedProcess.TurnaroundTime = selectedProcess.CompletionTime - selectedProcess.ArrivalTime;
      selectedProcess.WaitingTime = selectedProcess.TurnaroundTime - selectedProcess.BurstTime;

      // Add the completed process to the result and remove from remaining
      result.Processes.Add(selectedProcess);
      remainingProcesses.Remove(selectedProcess);
    }

    result.GanttChart = ganttChart;

    return result;
  }

  private SchedulingResult SRT(List<Process> processes)
  {
    var result = new SchedulingResult();
    var ganttChart = new List<GanttChartItem>();

    // Create a copy of processes to work with
    var remainingProcesses = processes.Select(p => new Process
    {
      Id = p.Id,
      ProcessId = p.ProcessId,
      ArrivalTime = p.ArrivalTime,
      BurstTime = p.BurstTime,
      RemainingTime = p.BurstTime,
      Priority = p.Priority,
      ResponseTime = -1 // -1 indicates not started yet
    }).ToList();

    int currentTime = 0;
    Process? currentProcess = null;
    int executionStart = 0;

    // Continue until all processes are completed
    while (remainingProcesses.Any(p => p.RemainingTime > 0))
    {
      // Find processes that have arrived by current time
      var availableProcesses = remainingProcesses.Where(p => p.ArrivalTime <= currentTime && p.RemainingTime > 0).ToList();

      if (!availableProcesses.Any())
      {
        // No processes available, find the next arrival time
        var nextArrival = remainingProcesses.Where(p => p.RemainingTime > 0)
            .OrderBy(p => p.ArrivalTime)
            .First();

        // Add idle time to gantt chart if there was a previous process
        if (currentProcess != null)
        {
          ganttChart.Add(new GanttChartItem
          {
            ProcessId = currentProcess.ProcessId,
            StartTime = executionStart,
            EndTime = currentTime
          });
          currentProcess = null;
        }

        // Add idle time
        ganttChart.Add(new GanttChartItem
        {
          ProcessId = -1, // -1 indicates CPU idle
          StartTime = currentTime,
          EndTime = nextArrival.ArrivalTime
        });

        currentTime = nextArrival.ArrivalTime;
        continue;
      }

      // Find process with shortest remaining time
      var shortestProcess = availableProcesses.OrderBy(p => p.RemainingTime).First();

      // If there's a context switch
      if (currentProcess != shortestProcess)
      {
        // Add completed execution to gantt chart if there was a previous process
        if (currentProcess != null)
        {
          ganttChart.Add(new GanttChartItem
          {
            ProcessId = currentProcess.ProcessId,
            StartTime = executionStart,
            EndTime = currentTime
          });
        }

        // Set the new current process and execution start time
        currentProcess = shortestProcess;
        executionStart = currentTime;

        // Set response time if this is the first time the process starts
        if (shortestProcess.ResponseTime == -1)
        {
          shortestProcess.ResponseTime = currentTime - shortestProcess.ArrivalTime;
        }
      }

      // Find the next event time (either new process arrival or current process completion)
      int timeToProcessCompletion = shortestProcess.RemainingTime;

      // Find if any new process arrives before current process completes
      var nextArrivingProcess = remainingProcesses
          .Where(p => p.ArrivalTime > currentTime && p.RemainingTime > 0)
          .OrderBy(p => p.ArrivalTime)
          .FirstOrDefault();

      int nextEventTime;
      if (nextArrivingProcess != null && nextArrivingProcess.ArrivalTime - currentTime < timeToProcessCompletion)
      {
        nextEventTime = nextArrivingProcess.ArrivalTime - currentTime;
      }
      else
      {
        nextEventTime = timeToProcessCompletion;
      }

      // Execute the process for the calculated time
      currentTime += nextEventTime;
      shortestProcess.RemainingTime -= nextEventTime;

      // If the process is completed
      if (shortestProcess.RemainingTime == 0)
      {
        // Add to gantt chart
        ganttChart.Add(new GanttChartItem
        {
          ProcessId = shortestProcess.ProcessId,
          StartTime = executionStart,
          EndTime = currentTime
        });

        // Set completion time and calculate metrics
        shortestProcess.CompletionTime = currentTime;
        shortestProcess.TurnaroundTime = shortestProcess.CompletionTime - shortestProcess.ArrivalTime;
        shortestProcess.WaitingTime = shortestProcess.TurnaroundTime - shortestProcess.BurstTime;

        // Reset current process
        currentProcess = null;
      }
    }

    result.Processes = remainingProcesses;

    // Merge adjacent gantt chart items for the same process
    var mergedGanttChart = new List<GanttChartItem>();
    for (int i = 0; i < ganttChart.Count; i++)
    {
      var current = ganttChart[i];
      // Skip zero-duration items
      if (current.StartTime == current.EndTime)
        continue;

      if (mergedGanttChart.Count > 0 && mergedGanttChart.Last().ProcessId == current.ProcessId)
      {
        // Merge with the last item
        mergedGanttChart.Last().EndTime = current.EndTime;
      }
      else
      {
        mergedGanttChart.Add(current);
      }
    }

    result.GanttChart = mergedGanttChart;
    return result;
  }

  private SchedulingResult PriorityScheduling(List<Process> processes)
  {
    var result = new SchedulingResult();
    var ganttChart = new List<GanttChartItem>();

    // Create a copy of processes to work with
    var remainingProcesses = processes.Select(p => new Process
    {
      Id = p.Id,
      ProcessId = p.ProcessId,
      ArrivalTime = p.ArrivalTime,
      BurstTime = p.BurstTime,
      RemainingTime = p.BurstTime,
      Priority = p.Priority
    }).ToList();

    int currentTime = 0;

    while (remainingProcesses.Any())
    {
      // Get processes that have arrived by the current time
      var availableProcesses = remainingProcesses.Where(p => p.ArrivalTime <= currentTime).ToList();

      if (!availableProcesses.Any())
      {
        // No process available, advance time to the next arrival
        int nextArrival = remainingProcesses.Min(p => p.ArrivalTime);

        // Add idle time to gantt chart
        ganttChart.Add(new GanttChartItem
        {
          ProcessId = -1, // -1 indicates CPU idle
          StartTime = currentTime,
          EndTime = nextArrival
        });

        currentTime = nextArrival;
        continue;
      }

      // Higher priority means lower number
      var selectedProcess = availableProcesses.OrderBy(p => p.Priority).First();

      // Set response time when the process starts execution for the first time
      selectedProcess.ResponseTime = currentTime - selectedProcess.ArrivalTime;

      // Add to gantt chart
      ganttChart.Add(new GanttChartItem
      {
        ProcessId = selectedProcess.ProcessId,
        StartTime = currentTime,
        EndTime = currentTime + selectedProcess.BurstTime
      });

      // Update current time
      currentTime += selectedProcess.BurstTime;

      // Set completion time
      selectedProcess.CompletionTime = currentTime;

      // Calculate turnaround time and waiting time
      selectedProcess.TurnaroundTime = selectedProcess.CompletionTime - selectedProcess.ArrivalTime;
      selectedProcess.WaitingTime = selectedProcess.TurnaroundTime - selectedProcess.BurstTime;

      // Add the completed process to the result and remove from remaining
      result.Processes.Add(selectedProcess);
      remainingProcesses.Remove(selectedProcess);
    }

    result.GanttChart = ganttChart;

    return result;
  }

  private SchedulingResult RoundRobin(List<Process> processes, int timeQuantum)
  {
    if (timeQuantum <= 0)
      throw new ArgumentException("Time quantum must be greater than zero", nameof(timeQuantum));

    var result = new SchedulingResult();
    var ganttChart = new List<GanttChartItem>();

    // Create a copy of processes to work with
    var remainingProcesses = processes.Select(p => new Process
    {
      Id = p.Id,
      ProcessId = p.ProcessId,
      ArrivalTime = p.ArrivalTime,
      BurstTime = p.BurstTime,
      RemainingTime = p.BurstTime,
      Priority = p.Priority,
      ResponseTime = -1 // -1 indicates not started yet
    }).ToList();

    // Sort by arrival time initially
    var processQueue = new Queue<Process>();
    int currentTime = 0;

    // Continue until all processes are completed
    while (remainingProcesses.Any(p => p.RemainingTime > 0) || processQueue.Any())
    {
      // Add newly arrived processes to the queue
      var newArrivals = remainingProcesses
          .Where(p => p.ArrivalTime <= currentTime && p.RemainingTime > 0 && !processQueue.Contains(p))
          .OrderBy(p => p.ArrivalTime)
          .ToList();

      foreach (var process in newArrivals)
      {
        processQueue.Enqueue(process);
      }

      // If there are no processes in the queue, advance time to the next arrival
      if (!processQueue.Any())
      {
        var nextArrival = remainingProcesses
            .Where(p => p.RemainingTime > 0)
            .OrderBy(p => p.ArrivalTime)
            .FirstOrDefault();

        if (nextArrival != null)
        {
          // Add idle time to gantt chart
          ganttChart.Add(new GanttChartItem
          {
            ProcessId = -1, // -1 indicates CPU idle
            StartTime = currentTime,
            EndTime = nextArrival.ArrivalTime
          });

          currentTime = nextArrival.ArrivalTime;
        }
        else
        {
          break; // No more processes to execute
        }

        continue;
      }

      // Get the next process from the queue
      var currentProcess = processQueue.Dequeue();

      // Set response time if this is the first time the process starts
      if (currentProcess.ResponseTime == -1)
      {
        currentProcess.ResponseTime = currentTime - currentProcess.ArrivalTime;
      }

      // Calculate execution time for this quantum
      int executionTime = Math.Min(timeQuantum, currentProcess.RemainingTime);

      // Add to gantt chart
      ganttChart.Add(new GanttChartItem
      {
        ProcessId = currentProcess.ProcessId,
        StartTime = currentTime,
        EndTime = currentTime + executionTime
      });

      // Update current time and remaining time
      currentTime += executionTime;
      currentProcess.RemainingTime -= executionTime;

      // Check if the process is completed
      if (currentProcess.RemainingTime > 0)
      {
        // Add newly arrived processes before re-adding the current process
        var arrivedDuringExecution = remainingProcesses
            .Where(p => p.ArrivalTime > currentTime - executionTime && p.ArrivalTime <= currentTime
                   && p.RemainingTime > 0 && !processQueue.Contains(p) && p != currentProcess)
            .OrderBy(p => p.ArrivalTime)
            .ToList();

        foreach (var process in arrivedDuringExecution)
        {
          processQueue.Enqueue(process);
        }

        // Re-add the current process to the queue
        processQueue.Enqueue(currentProcess);
      }
      else
      {
        // Process is completed
        currentProcess.CompletionTime = currentTime;
        currentProcess.TurnaroundTime = currentProcess.CompletionTime - currentProcess.ArrivalTime;
        currentProcess.WaitingTime = currentProcess.TurnaroundTime - currentProcess.BurstTime;
      }
    }

    result.Processes = remainingProcesses;

    // Merge adjacent gantt chart items for the same process
    var mergedGanttChart = new List<GanttChartItem>();
    for (int i = 0; i < ganttChart.Count; i++)
    {
      var current = ganttChart[i];
      if (mergedGanttChart.Count > 0 && mergedGanttChart.Last().ProcessId == current.ProcessId)
      {
        // Merge with the last item
        mergedGanttChart.Last().EndTime = current.EndTime;
      }
      else
      {
        mergedGanttChart.Add(current);
      }
    }

    result.GanttChart = mergedGanttChart;
    return result;
  }
}
