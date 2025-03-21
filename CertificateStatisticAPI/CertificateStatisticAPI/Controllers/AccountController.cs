using AutoMapper;
using CertificateStatisticAPI.DataModels;
using CertificateStatisticAPI.DataModels.DTOs;
using CertificateStatisticAPI.Tools;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using SqlSugar;

namespace CertificateStatisticAPI.Controllers
{
    [Route("api/[controller]/[action]")]
    [ApiController]
    public class AccountController : ControllerBase
    {
        /// <summary>
        /// 数据库上下文
        /// </summary>
        private readonly SqlSugarScope DB;

        /// <summary>
        /// AutoMapper类与DTO映射工具
        /// </summary>
        private readonly IMapper AutoMapper;

        public AccountController(SqlSugarScope DB, IMapper AutoMapper)
        {
            this.DB = DB;

            this.AutoMapper = AutoMapper;
        }

        /// <summary>
        /// 注册账号
        /// </summary>
        /// <param name="accountDTO">WPF端传来的账号信息</param>
        /// <returns></returns>
        [HttpPost]
        public IActionResult Register(AccountDTO accountDTO)
        {
            ResponseResult response = new ResponseResult();
            try
            {
                //检查账号是否存在
                bool exist = DB.Queryable<Account>().Any(a => a.PhoneNum == accountDTO.PhoneNum);
                if (exist)
                {
                    response.Status = -1;
                    response.Msg = "账号已存在";
                    return Ok(response);
                }

                //映射转换 AccountDTO->Account
                Account newAccount = AutoMapper.Map<Account>(accountDTO);
                newAccount.RegDate = DateTime.Now;

                //插入
                long snowFlakeID = DB.Insertable(newAccount).ExecuteReturnSnowflakeId();
                if (snowFlakeID != 0)
                {
                    response.Status = 1;
                    response.Msg = "注册成功，该账户雪花ID为：" + snowFlakeID.ToString();
                }
                else
                {
                    response.Status = -1;
                    response.Msg = "注册失败";
                }
            }
            catch (Exception ex)
            {
                response.Status = -1;//失败
                response.Msg = $"{ex.Message}\n服务器忙,请稍等...";

            }

            return Ok(response);

        }


    }
}
