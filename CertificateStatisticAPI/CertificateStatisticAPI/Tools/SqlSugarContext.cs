using SqlSugar;

namespace CertificateStatisticAPI.Tools
{
    public class SqlSugarContext
    {
        /// <summary>
        /// SqlSugarClient 原生模式访问数据库
        /// SqlSugarScope 单例模式访问数据库
        /// 对线程隔离不熟悉时，官方推荐SqlSugarScope
        /// </summary>
        public static SqlSugarScope DB { get; } = new SqlSugarScope(
                new ConnectionConfig
                {
                    ConnectionString = "Server=115.29.227.224;Port=3307;Database=certificatestatisticdb;Uid=admin;Pwd=HSTCCS7112;SslMode=Required;",
                    DbType = DbType.MySql,
                    IsAutoCloseConnection = true    //是否自动关闭线程池链接
                }
            );
    }
}
