//--------------------------------------------------------------------------------------------------
// (С) 2020-2023 ООО «РМ Солюшн». Smart System Platform 3.1. Все права защищены.
// Описание: ModuleCollection –
//--------------------------------------------------------------------------------------------------
namespace RmSolution.Server
{
    #region Using
    using RmSolution.Runtime;
    #endregion Using

    /// <summary> Модуль управления модулями (микросервисами).</summary>
    internal sealed class ModuleCollection : List<IModule>
    {
        /// <summary> Создание и запуск единственного экземпляра модуля.</summary>
        public IModule AddSingleton(Type serviceType, params object[] args)
        {
            return null;
        }
    }
}