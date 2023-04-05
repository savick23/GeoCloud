//--------------------------------------------------------------------------------------------------
// (С) 2020-2023 ООО «РМ Солюшн». RM System Platform 3.1. Все права защищены.
// Описание: SslCertificate – Настройки SSL-сертификата Веб-службы.
//--------------------------------------------------------------------------------------------------
namespace RmSolution.DataAccess
{
    /// <summary> Настройки SSL-сертификата Веб-службы.</summary>
    public class SslCertificate
    {
        public string? Path { get; set; }
        public string? Password { get; set; }
    }
}
