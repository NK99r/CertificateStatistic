using SqlSugar;

namespace CertificateStatisticAPI.Tools
{
    public class SqlSugarContext
    {
        public static SqlSugarScope DB { get; } = new SqlSugarScope(
                new ConnectionConfig
                {
                    ConnectionString = "Server=localhost;Database=certificatestatisticdb;Uid=root;Pwd=1234;",
                    DbType = DbType.MySql,
                    IsAutoCloseConnection = true
                }
            );
    }
}
