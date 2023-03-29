//--------------------------------------------------------------------------------------------------
// (С) 2020-2023 ООО «РМ Солюшн». RM System Platform 3.1. Все права защищены.
// Описание: RmInformationService –
//--------------------------------------------------------------------------------------------------
namespace RmSolution.Web
{
    #region Using
    using System.Threading;
    using System.Threading.Tasks;
    #endregion Using

    public class RmInformationService : BackgroundService
    {
        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                await Task.Delay(1000, stoppingToken);
            }
        }
    }
}