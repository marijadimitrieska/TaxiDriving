using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using OfficeOpenXml;
using PaginationApi.Data;
using PaginationApi.Models;

var builder = WebApplication.CreateBuilder(args);



// Add services to the container.
var connectionString = builder.Configuration.GetConnectionString("TaxiDB") ?? throw new InvalidOperationException("Connection string 'TaxiDB' not found.");

builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(connectionString));
builder.Services.AddDatabaseDeveloperPageExceptionFilter();

builder.Services.AddDefaultIdentity<IdentityUser>(options => options.SignIn.RequireConfirmedAccount = true)
    .AddEntityFrameworkStores<ApplicationDbContext>();
builder.Services.AddControllersWithViews();

builder.Services.Configure<PaginationSettings>(
    builder.Configuration.GetSection("Pagination"));

builder.WebHost.ConfigureKestrel((context, options) =>
{
    options.Configure(context.Configuration.GetSection("Kestrel"));
});

var app = builder.Build();

using var scope = app.Services.CreateScope();

using (scope)
{
    var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    context.Database.Migrate();

    var nullCount = context.TaxiDrivings.Count(r => r.createdDate == null
                                                || r.updatedDate == null);

    if (nullCount > 0)
    {
        Console.WriteLine($"Updating {nullCount} records with null dates...");

        context.Database.ExecuteSqlRaw(@"
        UPDATE data_small
        SET createdDate = COALESCE(createdDate, GETDATE()),
            updatedDate = COALESCE(updatedDate, GETDATE())
        WHERE createdDate IS NULL OR updatedDate IS NULL");

        Console.WriteLine("Update complete!");
    }

    context.SaveChangesAsync();
}

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseMigrationsEndPoint();
}
else
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");
app.MapRazorPages();

app.Run();
