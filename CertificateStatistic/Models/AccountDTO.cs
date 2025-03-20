using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CertificateStatisticWPF.Models
{
    class AccountDTO
    {
        /// <summary>
        /// 登录账号
        /// </summary>
        public string PhoneNum { get; set; }

        /// <summary>
        /// 密码
        /// </summary>
        public string Pwd { get; set; }

        /// <summary>
        /// 再次输入的密码
        /// </summary>
        public string ConfirmPwd { get; set; }
    }
}
