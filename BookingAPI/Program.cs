using BookingAPI.Repositories;
using BookingAPI.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddSingleton<IHomesRepository, InMemoryHomeRepository>();

builder.Services.AddSingleton<IAvailableHomesService, AvailableHomesService>();
builder.Services.AddControllers();
builder.Services.AddSwaggerGen();  

// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.UseSwagger();              
    app.UseSwaggerUI(c =>          
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "Booking API V1");
        c.RoutePrefix = "/swagger"; 
    });
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();

public partial class Program { }