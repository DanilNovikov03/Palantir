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
// Add Service
builder.Services.AddScoped<IArmyService, ArmyService>();
builder.Services.AddScoped<IOperationService, OperationService>();
builder.Services.AddScoped<IOperationSideService, OperationSideService>();
builder.Services.AddScoped<ISideService, SideService>();
builder.Services.AddScoped<ITheaterService, TheaterService>();
builder.Services.AddScoped<IWarService, WarService>();
builder.Services.AddScoped<IWarSideService, WarSideService>();

var app = builder.Build();

// Configure the HTTP request pipeline
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.UseDefaultFiles();
app.UseStaticFiles();

app.MapControllers();

app.Run();
