using CertificateStatisticAPI.DataModels;
using CertificateStatisticAPI.DataModels.DTOs;
using CertificateStatisticAPI.Tools;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Mysqlx;
using SqlSugar;
using System.Collections.Generic;

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
                    if (year == "全部")
                    {
                        ResponseResult.Status = 1;
                        ResponseResult.Msg = "获取全部数据成功";
                        ResponseResult.Data = query.ToList();
                    }
                    else if(year == "近五年")
                    {
                        //今年
                        int currentYear = DateTime.Now.Year;

                        //五年前
                        int fiveYearsAgo = currentYear - 4;

                        var Data = new List<Certificate>();
                        foreach (var cer in query.ToList())
                        {
                            int cerYear = int.Parse(cer.Date.Substring(0, 4));

                            //在近五年范围内
                            if (cerYear >= fiveYearsAgo && cerYear <= currentYear)
                            {
                                Data.Add(cer);
                            }
                        }

                        ResponseResult.Status = 1;
                        ResponseResult.Msg = "获取近五年数据成功";
                        ResponseResult.Data = query.ToList();
                    }
                    else
                    {
                        //如果选择某一年，筛选该年
                        query = query.Where(c => c.Date.StartsWith(year));
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

        /// <summary>
        /// 查询对应专业的数量
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public IActionResult GetProfessionCount()
        {
            var ResponseResult = new ResponseResult();
            try
            {
                var professionStat = DB.Queryable<Certificate>()
                    //联表查询，条件为两者的ProID相同
                    //详见果糖网：https://www.donet5.com/home/doc?masterId=1&typeId=1185
                    .LeftJoin<Profession>((c, p) => c.ProID == p.ProID)     
                    //按照专业名分组
                    .GroupBy((c, p) => p.ProfessionName)
                    //提取每组数据并化为新对象
                    .Select((c, p) => new ProfessionCountDTO
                    {
                        //如果专业名称为空，返回 "其他专业"，否则返回原专业名称。
                        ProfessionName = SqlFunc.IsNullOrEmpty(p.ProfessionName) ? "其他专业" : p.ProfessionName,
                        //统计每个专业下StudentID数量
                        //详见果糖网：https://www.donet5.com/home/doc?masterId=1&typeId=2243
                        Count = SqlFunc.AggregateCount(c.StudentID)
                    })
                    .ToList();

                if (professionStat != null)
                {
                    ResponseResult.Status = 1;
                    ResponseResult.Msg = "获得数据成功";
                    ResponseResult.Data = professionStat;
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
    }
}
