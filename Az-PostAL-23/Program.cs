using Az_PostAL_23.Data;
using Az_PostAL_23.Services;
using Azure.Identity;
using Azure.Messaging.ServiceBus;
using Azure.Storage.Blobs;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Azure;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
//Azure storage acount injection
builder.Services.AddControllersWithViews();
builder.Services.AddSingleton(u => new BlobServiceClient(
	builder.Configuration.GetValue<string>("AzureConnections:BlobStorageConnection")
));

//Azure SQL database injection
var connectionString = builder.Configuration.GetValue<string>("AzureConnections:AzureSQLConnection");
builder.Services.AddDbContext<ApplicationDBContext>(options => options.UseSqlServer(connectionString));

//Azure ServiceBus injection
builder.Services.AddAzureClients(builder => {
	//builder.AddClient<ServiceBusClient, ServiceBusClientOptions>((_, _, _) =>
	//{
	//	return new ServiceBusClient("Endpoint=sb://az-postal-np.servicebus.windows.net/;SharedAccessKeyName=RootManageSharedAccessKey;SharedAccessKey=CWkx5LTHv1/EX0+zfwliNqbZhXBoffbvE+ASbFG+2Og=", new DefaultAzureCredential());
	//});
	var connectionString = "Endpoint=sb://az-postal-np.servicebus.windows.net/;SharedAccessKeyName=RootManageSharedAccessKey;SharedAccessKey=CWkx5LTHv1/EX0+zfwliNqbZhXBoffbvE+ASbFG+2Og=";
	builder.AddServiceBusClient(connectionString);
});

builder.Services.AddSingleton<IContainerService, ContainerService>();
builder.Services.AddSingleton<IBlobService, BlobService>();

//SyncfusionKey Registration
Syncfusion.Licensing.SyncfusionLicenseProvider.RegisterLicense("Mgo+DSMBPh8sVXJ3S0R+WFpFaV5BQmFJfFdmRGldfVRzd0UmHVdTRHRcQ19iTX9QckZmXXdfc3Y=");

var app = builder.Build();


// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
	app.UseExceptionHandler("/Home/Error");
	// The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
	app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthorization();

app.MapControllerRoute(
	name: "default",
	pattern: "{controller=Dashboard}/{action=Index}/{id?}");

app.Run();
