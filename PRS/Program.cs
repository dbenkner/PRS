using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using PRS.Data;
var builder = WebApplication.CreateBuilder(args);


// Add services to the container.

builder.Services.AddControllers();
var connStrKey = "ProdDb";
#if DEBUG
    connStrKey="DevDb";
#endif
builder.Services.AddDbContext<PRSContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString(connStrKey) ?? throw new InvalidOperationException("Connection string 'DevDb' not found.")));

builder.Services.AddCors();

var app = builder.Build();


// Configure the HTTP request pipeline.

app.UseCors(x => x.AllowAnyOrigin().AllowAnyHeader().AllowAnyMethod()); //Will be changed when running on the server

app.UseAuthorization();

app.MapControllers();

app.Run();
