using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using PRS;
using PRS.Data;
using PRS.Services;
using System.Text;
var builder = WebApplication.CreateBuilder(args);


// Add services to the container.

builder.Services.AddControllers();
var connStrKey = "ProdDb";
var orgins = builder.Configuration["AllowedOrgins:ProdOrgin"];
#if DEBUG
    connStrKey="DevDb";
    orgins = builder.Configuration["AllowedOrgins:DevOrgin"];
#endif

builder.Services.AddDbContext<PRSContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString(connStrKey) ?? throw new InvalidOperationException("Connection string 'DevDb' not found.")));
builder.Services.AddTransient<DbInitialiser>();

var key = builder.Configuration["Jwt:Key"];
builder.Services.AddSingleton<AuthService>(new AuthService(key));


builder.Services.AddAuthentication(x =>
{
    x.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    x.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
}).AddJwtBearer(x =>
{
    x.TokenValidationParameters = new TokenValidationParameters
    {
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(key)),
        ValidateIssuer = false,
        ValidateAudience = false
    };
    x.Events = new JwtBearerEvents
    {
        OnMessageReceived = context =>
        {
            context.Token = context.Request.Cookies["Token"];
            return Task.CompletedTask;
        }
    };
});

builder.Services.AddAuthorization(x =>
{
    x.AddPolicy("admin", r => r.RequireRole("admin"));
    x.AddPolicy("user", r => r.RequireRole("user"));
    x.AddPolicy("reviewer", r => r.RequireRole("reviewer"));
});


builder.Services.AddCors();

var app = builder.Build();
using var scope = app.Services.CreateScope();


var services = scope.ServiceProvider;

var intialiser = services.GetRequiredService<DbInitialiser>();

intialiser.Run();

// Configure the HTTP request pipeline.

int i =0;

app.UseCors(x => x.WithOrigins(orgins).AllowAnyHeader().AllowAnyMethod().AllowCredentials()); //Will be changed when running on the server
app.UseAuthentication();

app.UseAuthorization();

app.MapControllers();

app.Run();
