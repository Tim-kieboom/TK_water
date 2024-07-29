using Npgsql;
using WebApplication1.data;
using WebApplication1.data.ORM;

try
{
    var database = new TK_ORM(new NpgsqlConnection("host=postgres;port=5432;Database=WaterUnitData;Username=tkWaterUser;Password=waterUnitPassowrd;SSL mode=prefer;Pooling=true;MinPoolSize=1;MaxPoolSize=100;"));
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

app.UseCors(webPolicy);

app.UseAuthorization();

app.MapControllers();

app.Run();
