using CartService.Service;
using Microsoft.OpenApi.Models;
using WishlistService.Service;

var builder = WebApplication.CreateBuilder(args);


//// Retrieve DB password from environment variable
//var dbPassword = Environment.GetEnvironmentVariable("DB_PASSWORD");
//Console.WriteLine($"Password: {dbPassword}");

//if (string.IsNullOrEmpty(dbPassword))
//{
//    throw new Exception("Environment variable DB_PASSWORD is not set.");
//}

//// Replace {DB_PASSWORD} in the connection string
//var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
//    .Replace("{DB_PASSWORD}", dbPassword);

//Console.WriteLine($"Final Connection String: {connectionString}"); // Debugging purpose


// Register Wishlist Service with HttpClient
builder.Services.AddHttpClient<WishlistServiceClass>();
builder.Services.AddScoped<WishlistServiceClass>();

// Register Cart Service
builder.Services.AddScoped<CartServiceClass>();

// Register Utility Service
builder.Services.AddScoped<UtilityService>();



// Add Controllers
builder.Services.AddControllers();

// Enable Swagger
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "Book Store APIs", Version = "v1" });
});

// CORS Configuration
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin()  // Allow any origin
              .AllowAnyMethod()  // Allow any HTTP method (GET, POST, etc.)
              .AllowAnyHeader(); // Allow any header
    });
});

var app = builder.Build();

var port = Environment.GetEnvironmentVariable("PORT") ?? "10000"; // Use Render’s PORT variable
app.Urls.Add($"http://0.0.0.0:{port}");

// Use Swagger in Development Environment
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// Enable CORS
app.UseCors("AllowAll");

//app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();

app.Run();
