﻿using CertificateStatisticAPI.DataModels;
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
                //SELECT DISTINCT SUBSTRING(Date, 1, 4) FROM CertificateTable;
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
                //SELECT * FROM CertificateTable
                if (query != null)
                {
                    if (year == "全部")
                    {
                        ResponseResult.Status = 1;
                        ResponseResult.Msg = "获取全部数据成功";
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
        /// 查询所有年份对应专业的证书数量
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public IActionResult GetTotalYearProfessionCount()
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
                /*
                    SELECT 
                        CASE
                            WHEN p.ProfessionName IS NULL OR p.ProfessionName = '' THEN '其他专业'
                            ELSE p.ProfessionName
                        END AS ProfessionName,
                        COUNT(c.StudentID) AS Count
                    FROM CertificateTable c LEFT JOIN ProfessionTable p ON c.ProID = p.ProID
                    GROUP BY p.ProfessionName;
                 */

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

        /// <summary>
        /// 查询单个年份对应专业的证书数量
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public IActionResult GetSingleYearProfessionCount(string year)
        {
            var ResponseResult = new ResponseResult();
            try
            {
                if (string.IsNullOrEmpty(year) || year.Length != 4)
                {
                    ResponseResult.Status = -1;
                    ResponseResult.Msg = "请输入有效的4位年份";
                    return Ok(ResponseResult);
                }

                var professionStat = DB.Queryable<Certificate>()
                                        //联表查询，条件为两者的ProID相同
                                        //详见果糖网：https://www.donet5.com/home/doc?masterId=1&typeId=1185
                                        .LeftJoin<Profession>((c, p) => c.ProID == p.ProID)
                                        // 筛选指定年份
                                        .Where((c, p) => c.Date.StartsWith(year))
                                        //按照专业名分组
                                        .GroupBy((c, p) => p.ProfessionName)
                                        //提取每组数据并化为新对象
                                        .Select((c, p) => new ProfessionCountDTO
                                        {
                                            //如果专业名称为空，返回 "其他专业"，否则返回原专业名称
                                            ProfessionName = SqlFunc.IsNullOrEmpty(p.ProfessionName) ? "其他专业" : p.ProfessionName,
                                            //统计每个专业下的证书数量
                                            Count = SqlFunc.AggregateCount(c.ID)
                                        })
                                        .ToList();
                /*
                    SELECT 
                        CASE 
                            WHEN ISNULL(p.ProfessionName, '') = '' THEN '其他专业' 
                            ELSE p.ProfessionName]
                        END AS ProfessionName,
                        COUNT(c.ID) AS Count
                    FROM CertificateTable c
                    LEFT JOIN ProfessionTable p ON c.ProID = p.ProID
                    WHERE c.Date LIKE '{year}%'
                    GROUP BY 
                        CASE 
                            WHEN ISNULL(p.ProfessionName, '') = '' THEN '其他专业' 
                            ELSE p.ProfessionName 
                        END
                 */

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
