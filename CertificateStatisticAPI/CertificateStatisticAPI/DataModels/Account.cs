using SqlSugar;

namespace CertificateStatisticAPI.DataModels
{
    [SugarTable("accounttable")]
    public class Account
    {
        [SugarColumn(IsPrimaryKey = true)]//long类型的主键会自动赋值
        public long ID { get; set; }

        public string PhoneNum { get; set; }

        public string Pwd {  get; set; }

        public DateTime RegDate { get; set; }
    }
}
