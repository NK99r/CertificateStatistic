using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CertificateStatisticWPF.Models.DTOs
{
    class AccountDTO
    {
        /// <summary>
        /// 登录账号
        /// </summary>
        public string AccountID { get; set; }

        /// <summary>
        /// 密码
        /// </summary>
        public string Pwd { get; set; }

        /// <summary>
        /// 再次输入的密码
        /// </summary>
        public string ConfirmPwd { get; set; }

        /// <summary>
        /// 密码盐值，每个用户各自唯一
        /// </summary>
        public string Salt { get; set; }
    }
}
