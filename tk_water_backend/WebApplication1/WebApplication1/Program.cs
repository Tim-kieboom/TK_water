using Microsoft.Extensions.FileProviders;
using WebApplication1.Controllers;
using WebApplication1.data.ORM;
using System.Data.Common;
using Npgsql;

try
{
    string connectionString = "host=postgres;port=5432;Database=WaterUnitData;Username=tkWaterUser;Password=waterUnitPassowrd;SSL mode=prefer;Pooling=true;MinPoolSize=1;MaxPoolSize=100;";
    DbConnection postgressConnection() { return new NpgsqlConnection(connectionString); }

    var database = new TK_ORM(postgressConnection);

    string sqlCreateTablesIfNotExist = File.ReadAllText(@"dataBaseTabels.txt");
    await database.ExecuteSqlQuery(sqlCreateTablesIfNotExist);
}
catch (Exception ex)
{
    Console.WriteLine(ex.Message);
}

var builder = WebApplication.CreateBuilder(args);
var webPolicy = "webPolicy";

builder.Services.AddCors(options =>
{
    options.AddPolicy(name: webPolicy,
            builder =>
            {
                builder.WithOrigins("http://localhost:4200", "https://localhost:4200")
                       .AllowAnyHeader()
                       .AllowAnyMethod()
                       .AllowCredentials();
            });
});


builder.Services.AddControllers();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddSingleton<MqttHandler>();


var app = builder.Build();

var mqtt = app.Services.GetRequiredService<MqttHandler>();

await Task.Run(async () => { await mqtt.Connect(); });

app.UseStaticFiles(new StaticFileOptions
{
    FileProvider = new PhysicalFileProvider(
           Path.Combine(builder.Environment.ContentRootPath, "WebContent/tk_water_ui/tk_water_ui/dist/tk_water_ui/browser")),
    RequestPath = ""
});

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseCors(webPolicy);
app.UseAuthorization();
app.MapControllers();

app.Run();