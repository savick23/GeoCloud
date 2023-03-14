//--------------------------------------------------------------------------------------------------
// (С) 2020-2023 ООО «РМ Солюшн». Smart System Platform 3.1. Все права защищены.
// Описание: LeicaTotalStationDevice – Тахеометр Leica.
// Протокол: GEOCOM Leica
//--------------------------------------------------------------------------------------------------
namespace RmSolution.DataAccess
{
    #region Using
    using System;
    using RmSolution.Data;
    #endregion Using

    /// <summary> Тахеометр Leica.</summary>
    public class LeicaTotalStationDevice : IDevice
    {
        #region Properties

        public long Id { get; set; }
        public string Code { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }

        #endregion Properties

        public LeicaTotalStationDevice()
        {
        }
    }
}
