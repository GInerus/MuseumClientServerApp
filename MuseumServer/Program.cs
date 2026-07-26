using Microsoft.AspNetCore.Http.Features;
using Microsoft.EntityFrameworkCore;
using MuseumServer.Data;
using MuseumServer.Services;


namespace MuseumServer
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            builder.WebHost.ConfigureKestrel(options =>
            {
                options.Limits.MaxRequestBodySize = 500 * 1024 * 1024;
            });

            // DbContext
            builder.Services.AddDbContextFactory<MuseumContext>(options =>
                options.UseSqlServer(builder.Configuration.GetConnectionString("MuseumDb")));

            // Add services
            builder.Services.AddSingleton<SessionService>();
            builder.Services.AddHostedService<SessionCleanupService>();
            builder.Services.AddScoped<ExhibitService>();
            builder.Services.AddScoped<DocumentService>();
            builder.Services.AddScoped<MediaFileService>();
            builder.Services.AddScoped<DepartmentService>();
            builder.Services.AddScoped<MuseumInfoService>();
            builder.Services.AddScoped<IFileService, FileService>();
            builder.Services.AddScoped<ImageProcessor>();
            builder.Services.AddScoped<VideoProcessor>();
            builder.Services.AddSingleton<LoggingService>();

            builder.Services.Configure<FormOptions>(options =>
            {
                options.MultipartBodyLengthLimit = 500 * 1024 * 1024;
            });

            builder.Services.AddControllers();
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen();

            builder.Services.AddCors(options =>
            {
                options.AddPolicy("AllowAll", policy =>
                {
                    policy.AllowAnyOrigin()
                          .AllowAnyHeader()
                          .AllowAnyMethod();
                });
            });

            var app = builder.Build();

            // Ожидание доступности базы данных
            using (var scope = app.Services.CreateScope())
            {
                var dbFactory = scope.ServiceProvider
                    .GetRequiredService<IDbContextFactory<MuseumContext>>();

                Console.WriteLine("Ожидание доступности базы данных...");

                while (true)
                {
                    try
                    {
                        using var db = dbFactory.CreateDbContext();

                        if (db.Database.CanConnect())
                        {
                            Console.WriteLine("Подключение к базе данных успешно.");
                            break;
                        }
                    }
                    catch
                    {
                        // Игнорируем ошибки и ждём следующую попытку
                    }

                    Thread.Sleep(TimeSpan.FromSeconds(30));
                }
            }

            app.UseSwagger();
            app.UseSwaggerUI();

            app.UseCors("AllowAll");

            app.MapControllers();

            app.Run();
        }
    }
}
