using EthanProject.DataAccess;
using EthanProject.Service;
using Microsoft.Extensions.Azure;
using Azure;
using Azure.Identity;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
var MyAllowSpecificOrigins = "_myAllowSpecificOrigins";

builder.Services.AddSingleton<ITableDataAccess, TableDataAccess>();
builder.Services.AddSingleton<ITableService, TableService>();

builder.Services.AddCors(options =>
{
    options.AddPolicy(name: MyAllowSpecificOrigins,
                      policy =>
                      {
                          //policy.WithOrigins("*", "http://127.0.0.1:5500", "http://localhost:5500"
                                             //);
                          policy.AllowAnyHeader();
                          policy.AllowAnyMethod();
                          policy.AllowAnyOrigin();
                          //policy.WithExposedHeaders("WWW-Authentication");
                      });
});

builder.Services.AddAzureClients(clientBuilder =>
{
    // Register clients in the container for each service 
    clientBuilder.AddBlobServiceClient(
        builder.Configuration.GetSection("Storage"));
    

    // Register client in the container for Azure tables
    clientBuilder.AddTableServiceClient(builder.Configuration.GetSection("AppSettings").GetSection("Storage").GetValue<string>("DatabaseConnectionString"));

    // The default credential works different ways whether you're running in Visual Studio or from within Azure.
    //  - When running from Visual Studio, it uses an Azure CLI credential.  To use this, you log into Azure in a
    //    Terminal window in Visual Studio (go to View -> Terminal from the menu if you don't see it).  In the Terminal window,
    //    type "az login" then login using the popup windows.  Once that's done, then you should be able to debug this from Visual Studio.
    //
    // - When accessing this API from Azure, it uses a Managed Identity credential.  You don't have to do anything special in code for that,
    //   it automatically uses the managed identity that should be set up on the Settings -> Identity page on your Api App in Azure.
    clientBuilder.UseCredential(new DefaultAzureCredential());

    // Set up any default settings
    clientBuilder.ConfigureDefaults(
        builder.Configuration.GetSection("AzureDefaults"));
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

app.UseCors(MyAllowSpecificOrigins);

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
