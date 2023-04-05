//--------------------------------------------------------------------------------------------------
// (С) 2020-2023 ООО «РМ Солюшн». RM System Platform 3.1. Все права защищены.
// Описание: AsyncHelper –
//--------------------------------------------------------------------------------------------------
namespace RmSolution.Web
{
    /// <summary> Выполнить синхронно асинхроный метод.</summary>
    internal static class AsyncHelper
    {
        private static readonly TaskFactory _task = new(CancellationToken.None, TaskCreationOptions.None, TaskContinuationOptions.None, TaskScheduler.Default);

        public static TResult RunSync<TResult>(Func<Task<TResult>> func) =>
            _task.StartNew(func).Unwrap().GetAwaiter().GetResult();

        public static void RunSync(Func<Task> func) =>
            _task.StartNew(func).Unwrap().GetAwaiter().GetResult();
    }
}
