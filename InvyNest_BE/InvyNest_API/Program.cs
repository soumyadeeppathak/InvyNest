
using InvyNest_API.Data;
using Microsoft.EntityFrameworkCore;

namespace InvyNest_API
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Add services to the container.

            builder.Services.AddControllers();
            // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen();
            // Add DbContext (Postgres)
            builder.Services.AddDbContext<AppDbContext>(opt =>
            {
                var cs = builder.Configuration.GetConnectionString("Postgres");
                opt.UseNpgsql(cs);
            });

            var app = builder.Build();

            // Configure the HTTP request pipeline.
            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }

            app.UseHttpsRedirection();

            app.UseAuthorization();


            app.MapControllers();
            app.MapGet("/healthz", () => Results.Ok(new { ok = true, time = DateTime.UtcNow }));

            app.Run();
        }
    }
}
