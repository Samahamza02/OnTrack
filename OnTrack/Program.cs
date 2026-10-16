using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using OnTrack.Data;
using OnTrack.Models;
using OnTrack.Services;
using OnTrack.Repositories;
using OnTrack.Interfaces;


namespace OnTrack
{
    public class Program
    {
        // NEW: changed from "void" to "async Task" so we can await the seeder
        public static async Task Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Add services
            builder.Services.AddControllersWithViews();

            // 1. Register ApplicationDbContext
            builder.Services.AddDbContext<ApplicationDbContext>(options =>
                options.UseSqlServer(
                    builder.Configuration.GetConnectionString("DefaultConnection")
                ));

            // 2. Register Identity
            builder.Services.AddIdentity<ApplicationUser, IdentityRole>(options =>
            {
                options.SignIn.RequireConfirmedAccount = true;

                // NEW: lockout settings (needed for lock/unlock users)
                options.Lockout.AllowedForNewUsers = true;
                options.Lockout.MaxFailedAccessAttempts = 5;
                options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(5);
            })
            .AddEntityFrameworkStores<ApplicationDbContext>()
            .AddDefaultTokenProviders();

            // NEW: where to send users who are not logged in / not allowed
            builder.Services.ConfigureApplicationCookie(options =>
            {
                options.LoginPath = "/Account/Login";
                options.AccessDeniedPath = "/Account/AccessDenied";
            });

            // NEW: a lock takes effect on already logged-in users within seconds
            builder.Services.Configure<SecurityStampValidatorOptions>(options =>
                options.ValidationInterval = TimeSpan.FromSeconds(10));

            // 3. Register Email Service
            builder.Services.AddTransient<IEmailSender, EmailSender>();

            // 4. Register generic repository
            builder.Services.AddScoped(typeof(IGenericRepository<>), typeof(GenericRepository<>));

            

            // Services
            builder.Services.AddScoped<ITicketService, TicketService>();
            builder.Services.AddScoped<IPaymentService, PaymentService>();
            builder.Services.AddScoped<IEvaluationService, EvaluationService>();
            builder.Services.AddScoped<INotificationService, NotificationService>();
            builder.Services.AddScoped<ITripStopService, TripStopService>();

            builder.Services.AddScoped<OnTrack.Services.IDashboardService,
                           OnTrack.Services.DashboardService>();
            var app = builder.Build();

            // Configure middleware
            if (!app.Environment.IsDevelopment())
            {
                app.UseExceptionHandler("/Home/Error");
                app.UseHsts();
            }

            app.UseHttpsRedirection();
            app.UseStaticFiles();

            app.UseRouting();

            // Authentication must come before Authorization
            app.UseAuthentication();
            app.UseAuthorization();

            app.MapStaticAssets();

            // NEW: area route, must come BEFORE the default route
            app.MapControllerRoute(
                name: "areas",
                pattern: "{area:exists}/{controller=Dashboard}/{action=Index}/{id?}")
                .WithStaticAssets();

            app.MapControllerRoute(
                name: "default",
                pattern: "{controller=Home}/{action=Index}/{id?}")
                .WithStaticAssets();

            app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

            // NEW: create roles and the first admin
            using (var scope = app.Services.CreateScope())
            {
                await DbSeeder.SeedAsync(scope.ServiceProvider);
            }

            app.Run();
        }
    }
}