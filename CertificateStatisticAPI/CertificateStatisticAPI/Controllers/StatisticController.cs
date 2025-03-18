using CertificateStatisticAPI.DataModels;
using CertificateStatisticAPI.Tools;
using CertificateStatisticAPI.Tools.Enum;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
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
        public ResponseResult<List<string>> GetAvailableYears()
        {
            //返回数据库中目前拥有的年份，如2022、2023、2024...
            //SqlFunc是SqlSugar框架调用SQL函数的方式
            List<string> list = DB.Queryable<Certificate>().Select(c => SqlFunc.Substring(c.Date, 0, 4)).Distinct().ToList();
            return new ResponseResult<List<string>> { Status = ResultStatus.Success, Msg = "获得数据成功", Data = list};
        }

        /// <summary>
        /// 查询对应年份的数据
        /// </summary>
        /// <param name="year">对应年份</param>
        /// <returns></returns>
        [HttpPost]
        public ResponseResult<List<Certificate>> GetByYear([FromBody] string year)
        {
            var query = DB.Queryable<Certificate>();
            if (year != "全部")
                //如果选择某一年，筛选
                query = query.Where(c => c.Date.StartsWith(year));
            //否则直接返回
            return new ResponseResult<List<Certificate>> { Data = query.ToList() };
        }


    }
}
