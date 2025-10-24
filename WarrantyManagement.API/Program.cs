using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;
using System.Reflection;
using WarrantyManagement.BLL.Services.Implements;
using WarrantyManagement.BLL.Services.Interfaces;
using WarrantyManagement.DAL.Data.Context;
using WarrantyManagement.DAL.Repositories.Implements;
using WarrantyManagement.DAL.Repositories.Interfaces;
using WarrantyManagement.DAL.Data.Mapper;

namespace WarrantyManagement.API
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // =============================
            // Services configuration
            // =============================
            builder.Services.AddControllers();
            builder.Services.AddEndpointsApiExplorer();

            // AutoMapper 
            builder.Services.AddAutoMapper(typeof(ClaimMappingProfile).Assembly);

            // DbContext
            builder.Services.AddDbContext<WarrantyDbContext>(options =>
                options.UseNpgsql(
                    builder.Configuration.GetConnectionString("DefaultConnection"),
                    b => b.MigrationsAssembly("WarrantyManagement.DAL")
                )
            );

            // Dependency Injection
            builder.Services.AddScoped<IUnitOfWork<WarrantyDbContext>, UnitOfWork<WarrantyDbContext>>();
            builder.Services.AddScoped(typeof(IGenericRepository<>), typeof(GenericRepository<>));
            builder.Services.AddScoped<IWorkOrderService, WorkOrderService>();
            builder.Services.AddScoped<IClaimService, ClaimService>();

            // CORS
            builder.Services.AddCors(options =>
            {
                options.AddPolicy("AllowAll", policy =>
                {
                    policy.AllowAnyOrigin()
                          .AllowAnyMethod()
                          .AllowAnyHeader();
                });
            });

            // Swagger
            builder.Services.AddSwaggerGen(options =>
            {
                options.SwaggerDoc("v1", new OpenApiInfo
                {
                    Title = "Warranty Management API",
                    Version = "v1",
                    Description = "API for managing electric vehicle warranty claims (No Authentication)"
                });

                var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
                var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
                if (File.Exists(xmlPath))
                    options.IncludeXmlComments(xmlPath);
            });

            var app = builder.Build();

            // =============================
            // Middleware pipeline
            // =============================

            // Swagger always enabled
            app.UseSwagger();
            app.UseSwaggerUI();

         
            var isDocker = Environment.GetEnvironmentVariable("DOTNET_RUNNING_IN_CONTAINER") == "true";
            if (!isDocker)
            {
                app.UseHttpsRedirection();
            }

            app.UseCors("AllowAll");
            app.UseAuthorization();
            app.MapControllers();
          
            app.Run();
        }
    }
}
