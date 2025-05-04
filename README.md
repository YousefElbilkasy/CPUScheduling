# CPU Scheduling Simulator

A web-based application for simulating and visualizing CPU scheduling algorithms, built with ASP.NET Core and Tailwind CSS.

![CPU Scheduling Simulator Screenshot](screenshots/app-screenshot.png)

## Overview

This application simulates common CPU scheduling algorithms used in operating systems, providing visual representations through Gantt charts and detailed performance metrics. It's designed as an educational tool to help understand the behavior and efficiency of different scheduling algorithms.

## Features

- Simulation of multiple CPU scheduling algorithms:

  - First Come First Served (FCFS)
  - Shortest Job First (SJF)
  - Shortest Remaining Time (SRT)
  - Priority Scheduling
  - Round Robin

- Interactive interface to:

  - Add/remove processes
  - Set process parameters (arrival time, burst time, priority)
  - Configure time quantum for Round Robin

- Visualization through:

  - Dynamic Gantt charts
  - Detailed process metrics tables

- Performance statistics:
  - Average waiting time
  - Average turnaround time
  - Average response time
  - CPU utilization

## Getting Started

### Prerequisites

- [.NET 9.0 SDK](https://dotnet.microsoft.com/download) or later
- [Node.js](https://nodejs.org/) (for Tailwind CSS)

### Installation

1. Clone the repository:

   ```bash
   git clone https://github.com/YousefElbilkasy/CpuScheduling.git
   cd CpuScheduling
   ```

2. Install Node.js dependencies:

   ```bash
   npm install
   ```

3. Build the Tailwind CSS:

   ```bash
   npm run build:css
   ```

4. Run the application:

   ```bash
   dotnet run
   ```

5. Open your browser and navigate to:
   ```
   http://localhost:5246
   ```

## Usage

1. **Select a Scheduling Algorithm**:

   - Choose from the dropdown menu: FCFS, SJF, SRT, Priority, or Round Robin
   - For Round Robin, specify a time quantum

2. **Define Processes**:

   - Add processes using the "Add Process" button
   - For each process, specify:
     - Process ID
     - Arrival Time
     - Burst Time
     - Priority (only relevant for Priority Scheduling)

3. **Run Simulation**:
   - Click "Run Simulation" to execute the selected algorithm
   - View the Gantt chart and performance metrics

## Algorithm Descriptions

### First Come First Served (FCFS)

- Processes are executed in the order they arrive
- Non-preemptive scheduling algorithm

### Shortest Job First (SJF)

- Executes the process with the shortest burst time first
- Non-preemptive scheduling algorithm

### Shortest Remaining Time (SRT)

- Preemptive version of SJF
- Switches to a new process if it has a shorter remaining time

### Priority Scheduling

- Executes processes based on priority (lower number = higher priority)
- Non-preemptive implementation

### Round Robin

- Each process is assigned a fixed time slot (quantum)
- Preemptive algorithm that switches after time quantum expires

## Technical Details

### Built With

- [ASP.NET Core 9.0](https://docs.microsoft.com/aspnet/core) - Web framework
- [Tailwind CSS](https://tailwindcss.com/) - CSS framework
- [jQuery](https://jquery.com/) - JavaScript library

### Project Structure

- `Controllers/` - Contains MVC controllers
- `Models/` - Data models for processes and simulation results
- `Services/` - Business logic for scheduling algorithms
- `Views/` - Razor views for the UI
- `wwwroot/` - Static files (CSS, JS)

## License

This project is licensed under the MIT License - see the LICENSE file for details.

## Acknowledgments

- Developed as a project for Operating Systems course
- Inspired by CPU scheduling algorithms in modern operating systems
