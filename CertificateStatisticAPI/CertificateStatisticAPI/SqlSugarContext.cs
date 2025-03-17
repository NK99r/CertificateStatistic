using SqlSugar;

namespace CertificateStatisticAPI
{
    public class SqlSugarContext
    {
        public static SqlSugarScope DB { get; } = new SqlSugarScope(
                new ConnectionConfig
                {
                    ConnectionString = "Server=localhost;Database=certificatestatisticdb;Uid=root;Pwd=1234;",
                    DbType = DbType.MySql,
                    IsAutoCloseConnection = true
                },
                db =>
                {
                    db.Aop.OnLogExecuting = (sql, pars) =>
                    {
                        //打印 SQL 语句
                        Console.WriteLine(sql); 
                    };
                }
            );
    }
}
