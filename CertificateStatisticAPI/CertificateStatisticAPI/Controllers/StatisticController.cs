using CertificateStatisticAPI.DataModels;
using CertificateStatisticAPI.Tools;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Mysqlx;
using SqlSugar;

namespace CertificateStatisticAPI.Controllers
{
    [Route("api/[controller]/[action]")]
    [ApiController]
    public class StatisticController : ControllerBase
    {
        private readonly SqlSugarScope DB;

        public StatisticController(SqlSugarScope db)
        {
            this.DB = db;
        }

        /// <summary>
        /// 查询所有年份
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public IActionResult GetAvailableYears()
        {
            var ResponseResult = new ResponseResult();
            try
            {
                //返回数据库中目前拥有的年份，如2022、2023、2024...
                //SqlFunc是SqlSugar框架调用SQL函数的方式
                List<string> list = DB.Queryable<Certificate>().Select(c => SqlFunc.Substring(c.Date, 0, 4)).Distinct().ToList();
                if (list != null)
                {
                    ResponseResult.Status = 1;
                    ResponseResult.Msg = "获得数据成功";
                    ResponseResult.Data = list;
                }
                else
                {
                    ResponseResult.Status = -1;
                    ResponseResult.Msg = "获得数据失败，请稍后重试";
                }
            }
            catch (Exception ex)
            {
                ResponseResult.Status = -1;
                ResponseResult.Msg = $"获取数据失败：{ex.Message}";
            }
            return Ok(ResponseResult);
        }

        /// <summary>
        /// 查询对应年份的数据
        /// </summary>
        /// <param name="year">对应年份</param>
        /// <returns></returns>
        [HttpPost]
        public IActionResult GetByYear([FromBody] string year)
        {
            var ResponseResult = new ResponseResult();
            try
            {
                var query = DB.Queryable<Certificate>();
                if (query != null)
                {
                    if (year != "全部")
                    {
                        //如果选择某一年，筛选该年
                        query = query.Where(c => c.Date.StartsWith(year));
                        ResponseResult.Status = 1;
                        ResponseResult.Msg = "获取一年数据成功";
                        ResponseResult.Data = query.ToList();
                    }
                    else
                    {
                        ResponseResult.Status = 1;
                        ResponseResult.Msg = "获取全部数据成功";
                        ResponseResult.Data = query.ToList();
                    }
                }
                else
                {
                    ResponseResult.Status = -1;
                    ResponseResult.Msg = "获取数据失败";
                }
                return Ok(ResponseResult);
            }
            catch (Exception ex)
            {
                ResponseResult.Status = -1;
                ResponseResult.Msg = $"获取数据失败:{ex.Message}";
            }
            return Ok(ResponseResult);

        }


    }
}
