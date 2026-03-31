using Microsoft.EntityFrameworkCore;
using Palantir.Application.Interfaces;
using Palantir.Infrastructure.Data;
using Palantir.Infrastructure.Repositories;

var builder = WebApplication.CreateBuilder(args);

// Add DB
var connectionString = 
    builder.Configuration.GetConnectionString(DatabaseSettings.ConnectionStringName)
    ?? throw new InvalidOperationException("Connection string not found");

builder.Services.AddDbContext<PalantirDbContext>(options => options.UseNpgsql(
    connectionString,
    o => o.UseNetTopologySuite())
);

// Add services to the container.
builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Add Repository
builder.Services.AddScoped<IArmyRepository, ArmyRepository>();
builder.Services.AddScoped<IOperationRepository, OperationRepository>();
builder.Services.AddScoped<IOperationSideRepository, OperationSideRepository>();
builder.Services.AddScoped<ISideRepository, SideRepository>();
builder.Services.AddScoped<ITheaterRepository, TheaterRepository>();
builder.Services.AddScoped<IWarRepository, WarRepository>();
builder.Services.AddScoped<IWarSideRepository, WarSideRepository>();

var app = builder.Build();

// Configure the HTTP request pipeline
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.MapGet("/", () => "Hello, World");

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
