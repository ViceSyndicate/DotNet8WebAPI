using DotNet8WebAPI.Entity;
using DotNet8WebAPI.Helpers;
using DotNet8WebAPI.Services;
using Microsoft.EntityFrameworkCore;
// https://medium.com/@codewithankitsahu/authentication-and-authorization-in-net-8-web-api-94dda49516ee
var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddSingleton<IOurHeroService, OurHeroService>();
builder.Services.AddDbContext<OurHeroDbContext>(db => db.UseSqlServer(builder.Configuration.GetConnectionString("OurHeroConnectionString")), ServiceLifetime.Singleton);

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// I added
app.UseMiddleware<JwtMiddleware>();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
