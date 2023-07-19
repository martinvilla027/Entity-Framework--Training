using Microsoft.AspNetCore.Builder;
using Microsoft.EntityFrameworkCore;
using SamuraiApp.Data;

namespace SamuraiAPI
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Add services to the container.
            
            builder.Services.AddControllers();
            builder.Services.AddSwaggerGen();
            // Database context configuration
            builder.Services.AddDbContext<SamuraiContext>(opt =>
            opt.UseSqlServer(builder.Configuration.GetConnectionString("SamuraiConnex"))
            .EnableSensitiveDataLogging()
            // Our controller has no reliance on tracking and this setting means the context won't bother 
            // This improve performance because the context won't waste any time needlessly tracking the objects that it queries
            .UseQueryTrackingBehavior(QueryTrackingBehavior.NoTracking));    

            var app = builder.Build();

            // Configure the HTTP request pipeline.
            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }

            app.UseAuthorization();

            app.MapControllers();

            app.Run();
        }
    }
}