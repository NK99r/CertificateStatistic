using SqlSugar;

namespace CertificateStatisticAPI.DataModels
{
    [SugarTable("professiontable")]
    public class Profession
    {
        [SugarColumn(IsPrimaryKey = true)]
        public string ProID { get; set; }

        [SugarColumn(ColumnName = "Profession")]
        public string ProfessionName { get; set; }
    }
}
