﻿using SqlSugar;

namespace CertificateStatisticAPI.DataModels
{
    [SugarTable("accounttable")]
    public class Account
    {
        [SugarColumn(IsPrimaryKey = true)]//long类型的主键会自动赋值
        public long ID { get; set; }

        public string AccountID { get; set; }

        public string Pwd {  get; set; }

        /// <summary>
        /// 密码盐值，每个用户各自唯一
        /// </summary>
        public string Salt { get; set; }

        public DateTime RegDate { get; set; }
    }
}
