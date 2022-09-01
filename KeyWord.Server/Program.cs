using KeyWord.Server.Services;
using KeyWord.Server.Storage;
using Microsoft.AspNetCore.Authentication.Negotiate;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
builder.Services.AddEntityFrameworkSqlite()
    .AddDbContext<StorageContext>(options => options.UseSqlite("Filename=keyword.db3")); // TODO: Move to app appsettings.json
builder.Services.AddSingleton<RegisterService>();
builder.Services.AddScoped<IStorage, ServerStorage>();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.MapControllers();

using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetService<StorageContext>();
    SQLitePCL.Batteries_V2.Init();
    context!.Database.EnsureCreated();
}

app.Run();

public partial class Program
{
}