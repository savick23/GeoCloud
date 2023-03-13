//--------------------------------------------------------------------------------------------------
// (�) 2020-2023 ��� ��� ������. Smart System Platform 3.1. ��� ����� ��������.
// ��������: ModuleCollection �
//--------------------------------------------------------------------------------------------------
namespace RmSolution.Server
{
    #region Using
    using RmSolution.Runtime;
    #endregion Using

    /// <summary> ������ ���������� �������� (��������������).</summary>
    internal sealed class ModuleCollection : List<IModule>
    {
        /// <summary> �������� � ������ ������������� ���������� ������.</summary>
        public IModule AddSingleton(Type serviceType, params object[] args)
        {
            return null;
        }
    }
}