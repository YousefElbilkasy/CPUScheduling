// Please see documentation at https://learn.microsoft.com/aspnet/core/client-side/bundling-and-minification
// for details on configuring this project to bundle and minify static web assets.

// Write your JavaScript code.

// Force dark mode only without toggle
document.addEventListener("DOMContentLoaded", function () {
  // Always enable dark mode
  document.documentElement.classList.add("dark");

  // Remove any theme preferences from local storage
  localStorage.removeItem("theme");

  // Hide any theme toggle controls if they exist
  const toggleElements = document.querySelectorAll(
    '[id*="darkMode"], [class*="theme-switch"]'
  );
  toggleElements.forEach((el) => (el.style.display = "none"));
});
