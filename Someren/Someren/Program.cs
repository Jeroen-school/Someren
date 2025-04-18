using Someren.Repositories;

namespace Someren
{
    public class Program
    {
        public static void Main(string[] args)
        {

            var builder = WebApplication.CreateBuilder(args);
            builder.Services.AddControllersWithViews();
            builder.Services.AddSingleton<IDbActivityRepository, DbActivityRepository>();
            builder.Services.AddSingleton<IActivitySupervisersRepository, DbActivitySupervisersRepository>();
            builder.Services.AddSingleton<IStudentsRepository, StudentsRepository>(); // Dependency injection for student repo
            builder.Services.AddSingleton<IRoomRepository, RoomRepository>();
            builder.Services.AddSingleton<ILecturersRepository, DbLecturersRepository>();
            builder.Services.AddSingleton<IActivityParticipantRepository, ActivityParticipantRepository>();
            builder.Services.AddSingleton<IDrinkRepository, DrinkRepository>();
            builder.Services.AddSingleton<IDrinkOrderRepository, DrinkOrderRepository>();


            var app = builder.Build();

            // Configure the HTTP request pipeline.
            if (!app.Environment.IsDevelopment())
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

            app.Run();
        }
    }
}
