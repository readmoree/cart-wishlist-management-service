using CartService.Service;
using Microsoft.OpenApi.Models;
using WishlistService.Service;

var builder = WebApplication.CreateBuilder(args);

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
