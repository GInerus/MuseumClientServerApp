using MuseumServer.Data;
using MuseumServer.Services;
using Microsoft.EntityFrameworkCore;

namespace MuseumServer
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // DbContext
            builder.Services.AddDbContext<MuseumContext>(options =>
                options.UseSqlServer(builder.Configuration.GetConnectionString("MuseumDb")));

            // Add services
            builder.Services.AddSingleton<SessionService>();
            builder.Services.AddHostedService<SessionCleanupService>();

            builder.Services.AddControllers();
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen();

            var app = builder.Build();

            app.UseSwagger();
            app.UseSwaggerUI();

            app.MapControllers();

            app.Run();
        }
    }
}
