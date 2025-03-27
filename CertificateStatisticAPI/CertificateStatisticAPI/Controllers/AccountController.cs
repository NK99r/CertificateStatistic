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
        public IActionResult Register(AccountDTO loginDTO)
        {
            ResponseResult response = new ResponseResult();
            try
            {
                //检查账号是否存在
                bool exist = DB.Queryable<Account>().Any(a => a.PhoneNum == loginDTO.PhoneNum);
                if (exist)
                {
                    response.Status = -1;
                    response.Msg = "用户已存在";
                    return Ok(response);
                }

                //映射转换 AccountDTO->Account
                Account newAccount = AutoMapper.Map<Account>(loginDTO);
                newAccount.RegDate = DateTime.Now;

                //插入
                long snowFlakeID = DB.Insertable(newAccount).ExecuteReturnSnowflakeId();
                if (snowFlakeID != 0)
                {
                    response.Status = 1;
                    response.Msg = "注册成功，该用户雪花ID为：" + snowFlakeID.ToString();
                }
                else
                {
                    response.Status = -1;
                    response.Msg = "注册失败";
                }
            }
            catch (Exception ex)
            {
                response.Status = -1;
                response.Msg = $"{ex.Message}\n服务器忙,请稍等...";

            }
            return Ok(response);
        }

        /// <summary>
        /// 登录
        /// </summary>
        /// <param name="accountDTO">WPF端传来的账号信息</param>
        /// <returns></returns>
        [HttpPost]
        public IActionResult Login(AccountDTO accountDTO)
        {
            ResponseResult response = new ResponseResult();
            try
            {
                //检查账号是否存在
                var account = DB.Queryable<Account>().First(a => a.PhoneNum == accountDTO.PhoneNum);
                //SELECT TOP 1 * FROM AccountTable WHERE PhoneNum = {};
                if (account == null)
                {
                    response.Status = -1;
                    response.Msg = "用户不存在，请检查手机号或注册";
                    return Ok(response);
                }
                if(account.Pwd != accountDTO.Pwd)
                {
                    response.Status = -1;
                    response.Msg = "密码错误，请重试";
                    return Ok(response);
                }

                //映射转换 Account->AccountDTO
                AccountDTO loginAccountDTO = AutoMapper.Map<AccountDTO>(account);

                response.Status = 1;
                response.Msg = "登录成功";
                response.Data = loginAccountDTO;
                return Ok(response);
            }
            catch (Exception ex)
            {
                response.Status = -1;
                response.Msg = $"{ex.Message}\n服务器忙,请稍等...";
            }
            return Ok(response);
        }

        /// <summary>
        /// 获得用户盐值
        /// </summary>
        /// <param name="phoneNum">手机号</param>
        /// <returns></returns>
        [HttpGet]
        public IActionResult GetSalt(string phoneNum)
        {
            ResponseResult response = new ResponseResult();
            try
            {
                var account = DB.Queryable<Account>().First(a => a.PhoneNum == phoneNum);
                //SELECT TOP 1 * FROM AccountTable WHERE PhoneNum = {};
                if (account == null)
                {
                    response.Status = -1;
                    response.Msg = "用户不存在";
                    return Ok(response);
                }

                response.Status = 1;
                response.Msg = "获取盐值成功";
                response.Data = account.Salt;
                return Ok(response);
            }
            catch (Exception ex)
            {
                response.Status = -1;
                response.Msg = $"{ex.Message}\n服务器忙,请稍等...";
            }
            return Ok(response);
        }

    }
}
