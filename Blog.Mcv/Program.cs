using Blog.Data;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Serilog;
using Serilog.Sinks.MSSqlServer;
using System.Security.Claims;
using System.Text;
var builder = WebApplication.CreateBuilder(args);

// Serilog'u yapılandır
var connectionString = builder.Configuration.GetConnectionString("Default") ?? throw new InvalidOperationException("Connection string is not found.");

var columnOptions = new ColumnOptions
{
    Id = { ColumnName = "Id" },
    Level = { ColumnName = "Level", DataLength = 50 },
    Message = { ColumnName = "Message", DataLength = -1 },
    MessageTemplate = { ColumnName = "MessageTemplate", DataLength = -1 },
    Exception = { ColumnName = "Exception", DataLength = -1 },
    TimeStamp = { ColumnName = "TimeStamp" },
    Properties = { ColumnName = "Properties" } // Varsayılan olarak XML formatında saklanır
};

Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Information() // Minimum log seviyesi
    .Enrich.FromLogContext()
    .WriteTo.Console()
    .WriteTo.MSSqlServer(
        connectionString: connectionString,
        sinkOptions: new MSSqlServerSinkOptions { TableName = "Logs", AutoCreateSqlTable = true },
        columnOptions: columnOptions)
    .CreateLogger();


Log.Information("Uygulama başlatılıyor...");

    builder.Host.UseSerilog(); // Varsayılan logger yerine Serilog'u kullan

    // Add services to the container.
    builder.Services.AddControllersWithViews();

    builder.Services.AddBlogsSbData(connectionString);
    builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new Microsoft.IdentityModel.Tokens.TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidIssuer = "Blog",
            ValidateAudience = true,
            ValidAudience = "MVC",
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Secret"] ?? throw new Exception("Null"))),
            RoleClaimType = ClaimTypes.Role
        };
        options.Events = new JwtBearerEvents
        {
            OnMessageReceived = context =>
            {
                var accessToken = context.Request.Cookies["access_token"];

                if (!string.IsNullOrWhiteSpace(accessToken))
                {
                    context.Token = accessToken;
                }
                return Task.CompletedTask;
            },

            OnChallenge = async context =>
            {
                context.HandleResponse();
                context.Response.Redirect("/Auth/Login");
                await Task.CompletedTask;
            },

        };
        options.MapInboundClaims = false;

    });

    var app = builder.Build();

    // Configure the HTTP request pipeline.
    if (!app.Environment.IsDevelopment())
    {
        app.UseHsts();
    }

    app.UseHttpsRedirection();
    app.UseStaticFiles();
    app.UseAuthentication();
    app.UseRouting();

    app.UseAuthorization();

    app.MapControllerRoute(
        name: "default",
        pattern: "{controller=Home}/{action=Index}/{id?}");
    using (var scope = app.Services.CreateScope())
    {
        var services = scope.ServiceProvider;
        var context = services.GetRequiredService<DbContext>();

        await context.EnsureCreatedAndSeedAsync();
    }
    app.Run();
