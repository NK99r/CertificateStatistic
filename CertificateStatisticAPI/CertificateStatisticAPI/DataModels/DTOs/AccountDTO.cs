namespace CertificateStatisticAPI.DataModels.DTOs
{
    public class AccountDTO
    {
        public string AccountID { get; set; }

        public string Pwd { get; set; }

        /// <summary>
        /// 密码盐值，每个用户各自唯一
        /// </summary>
        public string Salt { get; set; }
    }
}
