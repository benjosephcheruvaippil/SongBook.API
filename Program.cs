using SongBook.API.Data;
using SongBook.API.Repositories;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddSingleton<DapperContext>();
builder.Services.AddScoped<ISongBookRepository, SongBookRepository>();
builder.Services.AddOpenApi();

// Add Swagger
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

app.UseSwagger();
app.UseSwaggerUI(options =>
{
    // This defines the endpoint for the JSON file
    options.SwaggerEndpoint("/swagger/v1/swagger.json", "SongBook API v1");

    // This ensures the UI is served at /swagger
    options.RoutePrefix = "swagger";
});

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    //app.UseSwagger();
    //app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
