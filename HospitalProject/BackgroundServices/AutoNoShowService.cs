//using HospitalProject.Services;

//namespace HospitalProject.BackgroundServices
//{
//    public class AutoNoShowService : BackgroundService
//    {
//        private readonly IServiceScopeFactory _scopeFactory;

//        public AutoNoShowService(IServiceScopeFactory scopeFactory)
//        {
//            _scopeFactory = scopeFactory;
//        }

//        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
//        {
//            while (!stoppingToken.IsCancellationRequested)
//            {
//                using var scope = _scopeFactory.CreateScope();
//                var service = scope.ServiceProvider
//                    .GetRequiredService<HospitalService>();

//                await service.MarkAutoNoShowAppointments();

//                // 24 hours format
//                await Task.Delay(TimeSpan.FromHours(24), stoppingToken);
//            }
//        }
//    }

//}
