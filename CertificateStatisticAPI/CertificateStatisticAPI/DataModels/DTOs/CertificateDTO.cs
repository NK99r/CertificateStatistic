using SqlSugar;

namespace CertificateStatisticAPI.DataModels
{
    public class CertificateDTO
    {
        public string StudentID {  get; set; }

        public string Name { get; set; }

        public string CertificateProject { get; set; }

        public string Category { get; set; }

        public string EventLevel { get; set; }

        public string Organizer { get; set; }

        public string Date { get; set; }
    }
}
