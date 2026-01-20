//using HospitalProject.Services;
//using Microsoft.Extensions.DependencyInjection;
//using Microsoft.Extensions.Hosting;

//public class NoShowBackgroundService : BackgroundService
//{
//    private readonly IServiceScopeFactory _scopeFactory;

//    public NoShowBackgroundService(IServiceScopeFactory scopeFactory)
//    {
//        _scopeFactory = scopeFactory;
//    }

//    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
//    {
//        while (!stoppingToken.IsCancellationRequested)
//        {
//            using var scope = _scopeFactory.CreateScope();

//            var service = scope.ServiceProvider
//                .GetRequiredService<HospitalService>();

//            await service.MarkNoShowAppointments();

//            // Every 5 minutes check pannum
//            await Task.Delay(TimeSpan.FromMinutes(5), stoppingToken);
//        }
//    }
//}
