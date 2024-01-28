using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.DependencyInjection;
using SendMailLib;
using FileUploadLib;
using DropboxMail;

var host = new HostBuilder()
    .ConfigureFunctionsWebApplication()
    .ConfigureServices(services => {
        services.AddApplicationInsightsTelemetryWorkerService();
        services.ConfigureFunctionsApplicationInsights();

        services.AddScoped<ISendMailService, SendMailSmtpService>();
        services.AddScoped<IFileUploadService, UploadDropboxService>();
        services.AddScoped<IDropboxMailService, DropboxMailService>();
    })
    .Build();

host.Run();
