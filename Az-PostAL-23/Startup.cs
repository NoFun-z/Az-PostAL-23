using Az_PostAL_23.Data;
using Az_PostAL_23.Services;
using Azure.Identity;
using Azure.Messaging.ServiceBus;
using Azure.Storage.Blobs;
using Microsoft.AspNetCore.Builder;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Azure;

//var builder = WebApplication.CreateBuilder(args);


//Add services to the container.
//Azure storage acount injection
//builder.Services.AddControllersWithViews();
//builder.Services.AddSingleton(u => new BlobServiceClient(
//	builder.Configuration.GetValue<string>("AzureConnections:BlobStorageConnection")
//));

//Azure SQL database injection
//var connectionString = builder.Configuration.GetValue<string>("AzureConnections:AzureSQLConnection");
//builder.Services.AddDbContext<ApplicationDBContext>(options => options.UseSqlServer(connectionString));

//Azure ServiceBus injection
//builder.Services.AddAzureClients(builder =>
//{
//	builder.AddClient<ServiceBusClient, ServiceBusClientOptions>((_, _, _) =>
//	{
//		return new ServiceBusClient("Endpoint=sb://az-postal-np.servicebus.windows.net/;SharedAccessKeyName=RootManageSharedAccessKey;SharedAccessKey=CWkx5LTHv1/EX0+zfwliNqbZhXBoffbvE+ASbFG+2Og=", new DefaultAzureCredential());
//	});
//	var connectionString = "Endpoint=sb://az-postal-np.servicebus.windows.net/;SharedAccessKeyName=RootManageSharedAccessKey;SharedAccessKey=CWkx5LTHv1/EX0+zfwliNqbZhXBoffbvE+ASbFG+2Og=";
//	builder.AddServiceBusClient(connectionString);
//});

//builder.Services.AddSingleton<IContainerService, ContainerService>();
//builder.Services.AddSingleton<IBlobService, BlobService>();

//SyncfusionKey Registration
//Syncfusion.Licensing.SyncfusionLicenseProvider.RegisterLicense("Mgo+DSMBPh8sVXJ3S0R+WFpFaV5BQmFJfFdmRGldfVRzd0UmHVdTRHRcQ19iTX9QckZmXXdfc3Y=");

//var app = builder.Build();


// Configure the HTTP request pipeline.
//if (!app.Environment.IsDevelopment())
//{
//	app.UseExceptionHandler("/Home/Error");
//	 The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
//	app.UseHsts();
//}

//app.UseHttpsRedirection();
//app.UseStaticFiles();

//app.UseRouting();

//app.UseAuthorization();

//app.MapControllerRoute(
//	name: "default",
//	pattern: "{controller=Dashboard}/{action=Index}/{id?}");

//app.Run();

public class Startup
{
    public Startup(IConfiguration configuration)
    {
        Configuration = configuration;
    }

    public IConfiguration Configuration { get; }

    // This method gets called by the runtime. Use this method to add services to the container.
    public void ConfigureServices(IServiceCollection services)
    {
        // Add services to the container.
        services.AddControllersWithViews();
        services.AddSingleton(u => new BlobServiceClient(
            Configuration.GetValue<string>("AzureConnections:BlobStorageConnection")
        ));

        var connectionString = Configuration.GetValue<string>("AzureConnections:AzureSQLConnection");
        services.AddDbContext<ApplicationDBContext>(options => options.UseSqlServer(connectionString));

        services.AddAzureClients(builder =>
        {
            var serviceBusConnectionString = "Endpoint=sb://az-postal-np.servicebus.windows.net/;SharedAccessKeyName=RootManageSharedAccessKey;SharedAccessKey=CWkx5LTHv1/EX0+zfwliNqbZhXBoffbvE+ASbFG+2Og=";
            builder.AddServiceBusClient(serviceBusConnectionString);
        });

        services.AddSingleton<IContainerService, ContainerService>();
        services.AddSingleton<IBlobService, BlobService>();

        Syncfusion.Licensing.SyncfusionLicenseProvider.RegisterLicense("Mgo+DSMBPh8sVXJ3S0R+WFpFaV5BQmFJfFdmRGldfVRzd0UmHVdTRHRcQ19iTX9QckZmXXdfc3Y=");
    }

    // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
    public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
    {
        if (!env.IsDevelopment())
        {
            app.UseExceptionHandler("/Home/Error");
            app.UseHsts();
        }

        app.UseHttpsRedirection();
        app.UseStaticFiles();

        app.UseRouting();

        app.UseAuthorization();

        app.UseEndpoints(endpoints =>
        {
            endpoints.MapControllerRoute(
                name: "default",
                pattern: "{controller=Dashboard}/{action=Index}/{id?}"
            );
        });
    }
}
