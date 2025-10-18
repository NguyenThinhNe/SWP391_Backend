
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using WarrantyManagement.BLL.Services.Implements;
using WarrantyManagement.BLL.Services.Interfaces;
using WarrantyManagement.DAL.Data.Context;
using WarrantyManagement.DAL.Repositories.Implements;
using WarrantyManagement.DAL.Repositories.Interfaces;

namespace WarrantyManagement.API
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Add services to the container.

            builder.Services.AddAutoMapper(typeof(Program).Assembly); // Add appropriate assembly

            // Add UnitOfWork
            builder.Services.AddScoped<IUnitOfWork<WarrantyDbContext>, UnitOfWork<WarrantyDbContext>>();
            // Generic repository for common CRUD operations
            builder.Services.AddScoped(typeof(IGenericRepository<>), typeof(GenericRepository<>));

            builder.Services.AddScoped<IClaimService, ClaimService>();
            builder.Services.AddDbContext<WarrantyDbContext>(options =>
            options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));
            builder.Services.AddControllers();
            // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen();

            var app = builder.Build();

            // Configure the HTTP request pipeline.
            // if (app.Environment.IsDevelopment())
            // {
            //    app.UseSwagger();
            //    app.UseSwaggerUI();
            // }

            app.UseSwagger();
            app.UseSwaggerUI();

            app.UseHttpsRedirection();

            app.UseAuthorization();


            app.MapControllers();

            app.Run();
        }
    }
}
