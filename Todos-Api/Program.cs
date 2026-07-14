var builder = WebApplication.CreateBuilder(args);
const string AllowAllCorsPolicy = "AllowAll";
const string AllowLocalhostAndProdPolicy = "AllowLocalhostAndProd";

// Add services to the container.

builder.Services.AddControllers();
builder.Services.AddCors(options =>
{
    options.AddPolicy(AllowAllCorsPolicy, policy =>
    {
        policy
            .AllowAnyOrigin()
            .AllowAnyHeader()
            .AllowAnyMethod();
    });
    options.AddPolicy(AllowLocalhostAndProdPolicy, policy =>
    {
        policy.WithOrigins("https://kamil-caly.github.io", "http://localhost:3000")
            .AllowAnyHeader()
            .AllowAnyMethod();
    });
});
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// test
var app = builder.Build();

// Configure the HTTP request pipeline.
app.MapOpenApi();
app.UseSwagger();
app.UseSwaggerUI();

app.UseHttpsRedirection();

//app.UseCors(AllowAllCorsPolicy);
app.UseCors(AllowLocalhostAndProdPolicy);

app.UseAuthorization();

app.MapControllers();

app.Run();
