//--------------------------------------------------------------------------------------------------
// (С) 2020-2023 ООО «РМ Солюшн». RM System Platform 3.1. Все права защищены.
// Описание: TUserRow – Пользователи Системы.
//--------------------------------------------------------------------------------------------------
namespace RmSolution.Data
{
    [Table("config.users")]
    public class TUserRow
    {
        [Column("id bigint PRIMARY KEY")]
        public long Id { get; set; }
        [Column("code nvarchar(8) NOT NULL")]
        public string Code { get; set; }
        [Column("name nvarchar(64) NOT NULL")]
        public string Name { get; set; }
        [Column("descript nvarchar(1024) NULL")]
        public string Descript { get; set; }
    }
}
