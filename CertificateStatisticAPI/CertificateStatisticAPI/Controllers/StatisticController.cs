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
        /// 插入证书项目
        /// </summary>
        /// <param name="certificate">[FromBody]特性从请求的 body 中获取一个 JSON 格式的数据，并将其反序列化成 Certificate 类型的对象</param>
        /// <returns>统一封装结果</returns>
        [HttpPost]
        public ResponseResult<string> AddCertificate([FromBody]List<Certificate> certificates)
        {
            try
            {
                //获得传递过来的表的三个字段的组合集合(学号+获奖项目+年份组合的集合)
                var checkKeys = certificates.Select(c => new
                                            {
                                                c.StudentID,
                                                c.CertificateProject,
                                                Year = c.Date.Substring(0, 4)
                                            }).ToList();

                //获得的三字段组合集合与数据库表中的三个字段做比对，有相同的则提取，表示有重复项
                var duplicates = DB.Queryable<Certificate>()    //从数据库查询
                                           .Where(c => checkKeys.Any(k =>   //Any查询，用于判断checkKeys中是否存在某些项与以下条件符合
                                                k.StudentID == c.StudentID &&
                                                k.CertificateProject == c.CertificateProject &&
                                                c.Date.StartsWith(k.Year)))
                                           .Select(c => new     //获得符合上述条件的数据集合，即学号相同、获奖项目相同、年份相同三条件成立的数据
                                           {
                                               c.StudentID,
                                               c.CertificateProject,
                                               Year = c.Date.Substring(0, 4)
                                           })
                                           .ToList();

                //如果存在重复数据
                if (duplicates.Any())
                {
                    var duplicateList = string.Join("\n",duplicates.Select(d => $"{d.StudentID}-{d.CertificateProject}-{d.Year}"));
                    //对于存在的重复数据的每一项，转换为字符串格式：学号-获奖项目-年份
                    return new ResponseResult<string>
                    {
                        Status = ResultStatus.Error,
                        Msg = $"发现重复数据：{duplicateList}"
                    };
                }

                //否则正常插入
                DB.Insertable(certificates).ExecuteReturnSnowflakeIdList();
                return new ResponseResult<string> { Status = ResultStatus.Success ,Msg = "数据导入成功"};
            }
            catch (Exception ex)
            {
                return new ResponseResult<string> { Status = ResultStatus.Error, Msg = $"数据导入失败：{ex.Message}"};
            }
        }
    }
}
