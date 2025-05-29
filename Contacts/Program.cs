using BL.Services;
using BL.Services.Interfaces;
using Contacts;
using Contacts.Transformer;
using Contacts.Transformer.Interface;
using DAL.Repository;
using DAL.Repository.Interface;
using DAL.Transformer;
using DAL.Transformer.Interfaces;
using Serilog;



var builder = WebApplication.CreateBuilder(args);

//Setup Logger
Log.Logger = new LoggerConfiguration()
    .WriteTo.Console()
    .WriteTo.File("Logs/log.txt", rollingInterval: RollingInterval.Day)
    .Enrich.FromLogContext()
    .MinimumLevel.Debug()
    .CreateLogger();

builder.Host.UseSerilog(); // Replace default .NET logger

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddTransient<IContactsRepository, ContactRepository>();
builder.Services.AddSingleton<IContactTransformer, ContactTransformer>();
builder.Services.AddSingleton<IContactsService, ContactsService>();
builder.Services.AddSingleton<IContactDtoTransformer, ContactDtoTransformer>();
builder.Services.AddSingleton<IMetricsService, MetricsService>();

//Register PhoneBook services
builder.Services.RegisterPhoneBookServices(builder.Configuration);
builder.Services.AddHealthChecks();

var app = builder.Build();

app.MapHealthChecks("/health");
// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.UseMiddleware<MetricsMiddleware>();

app.MapControllers();

app.Run();