using CPUScheduling.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();
builder.Services.AddScoped<SchedulingService>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
  app.UseExceptionHandler("/Home/Error");
}
app.UseRouting();

app.UseAuthorization();

// Ensure static files are properly served before MapStaticAssets
app.UseStaticFiles();
app.MapStaticAssets();

app.MapControllerRoute(
  name: "default",
  pattern: "{controller=Home}/{action=Index}/{id?}")
  .WithStaticAssets();

app.Run();
