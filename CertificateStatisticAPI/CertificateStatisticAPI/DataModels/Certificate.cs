using SqlSugar;

namespace CertificateStatisticAPI.DataModels
{
    [SugarTable("certificatetable")]
    public class Certificate
    {
        [SugarColumn(IsPrimaryKey = true)]//long类型的主键会自动赋值
        public long ID { get; set; }

        public string StudentID {  get; set; }

        public string Name { get; set; }

        public string CertificateProject { get; set; }

        public string Category { get; set; }

        public string EventLevel { get; set; }

        public string Organizer { get; set; }

        public string Date { get; set; }
    }
}
