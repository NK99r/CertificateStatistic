using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace CertificateStatisticWPF.Tools
{
    internal static class EncryptionTool
    {
        /// <summary>
        /// 从 chars 字符串中随机选择16个字符，生成一个新的随机字符串
        /// </summary>
        /// <returns>16位随机字符盐值</returns>
        public static string GenerateSalt()
        {
            var random = new Random();
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789";
            return new string(Enumerable.Repeat(chars, 16)
                .Select(s => s[random.Next(s.Length)]).ToArray());
        }

        /// <summary>
        /// 加密
        /// </summary>
        /// <param name="password">明文密码</param>
        /// <param name="salt">盐值</param>
        /// <returns></returns>
        public static string HashPassword(string password, string salt)
        {
            //生成256位哈希值
            using (var sha256 = SHA256.Create())
            {
                //拼接明文密码和盐值
                var saltedPassword = password + salt;
                //Encoding.UTF8.GetBytes(saltedPassword)将拼接字符串转为字节数组
                //sha256.ComputeHash(...)使用SHA256算法对字节数组进行哈希计算
                var hashedBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(saltedPassword));
                //去掉字符串中的"-"并转为小写后返回，
                return BitConverter.ToString(hashedBytes).Replace("-", "").ToLower();
            }
        }
    }
}
