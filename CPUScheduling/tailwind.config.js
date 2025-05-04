/** @type {import('tailwindcss').Config} */
module.exports = {
  content: [
    "./Pages/**/*.cshtml",
    "./Views/**/*.cshtml",
    "./wwwroot/js/**/*.js",
  ],
  darkMode: "class",
  theme: {
    extend: {
      colors: {
        primary: "#3b82f6",
        secondary: "#64748b",
        success: "#22c55e",
        danger: "#ef4444",
        warning: "#f59e0b",
        info: "#3b82f6",
      },
    },
  },
  plugins: [],
};
