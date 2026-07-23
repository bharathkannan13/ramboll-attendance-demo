using RambollAttendanceAPI.Data;
using RambollAttendanceAPI.Repositories;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();

// Register Database Connection Factory
builder.Services.AddSingleton<IDbConnectionFactory, SqlConnectionFactory>();

// Register Repositories
builder.Services.AddScoped<IAttendanceRepository, AttendanceRepository>();

// Configure CORS to support local HTML file client execution
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowLocalClient", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyHeader()
              .AllowAnyMethod();
    });
});

var app = builder.Build();

// Configure the HTTP request pipeline.
app.UseCors("AllowLocalClient");

app.UseAuthorization();

app.MapControllers();

app.Run();
