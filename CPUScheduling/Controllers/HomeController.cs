using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using CPUScheduling.Models;
using CPUScheduling.Services;

namespace CPUScheduling.Controllers;

public class HomeController : Controller
{
  private readonly ILogger<HomeController> _logger;
  private readonly SchedulingService _schedulingService;

  public HomeController(ILogger<HomeController> logger, SchedulingService schedulingService)
  {
    _logger = logger;
    _schedulingService = schedulingService;
  }

  public IActionResult Index()
  {
    var viewModel = new SchedulingViewModel
    {
      Processes = new List<Models.Process>
        {
          new Models.Process { Id = 1, ProcessId = 1, ArrivalTime = 0, BurstTime = 5, Priority = 1 },
          new Models.Process { Id = 2, ProcessId = 2, ArrivalTime = 1, BurstTime = 3, Priority = 1 },
          new Models.Process { Id = 3, ProcessId = 3, ArrivalTime = 2, BurstTime = 8, Priority = 1 }
        }
    };
    return View(viewModel);
  }

  [HttpPost]
  public IActionResult RunScheduler(SchedulingViewModel model)
  {
    if (model.Processes != null && model.Processes.Any())
    {
      // Ensure process IDs are assigned properly
      for (int i = 0; i < model.Processes.Count; i++)
      {
        if (model.Processes[i].ProcessId <= 0)
        {
          model.Processes[i].ProcessId = i + 1;
        }

        // Set default priority (1) for all processes when not using Priority algorithm
        if (model.SelectedAlgorithm != SchedulingAlgorithm.Priority)
        {
          model.Processes[i].Priority = 1;
        }
      }

      // Run the scheduling algorithm
      model.Result = _schedulingService.Schedule(
        model.Processes,
        model.SelectedAlgorithm,
        model.TimeQuantum
      );
    }

    return View("Index", model);
  }

  [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
  public IActionResult Error()
  {
    return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
  }
}
