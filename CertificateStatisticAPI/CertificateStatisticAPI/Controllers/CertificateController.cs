
using CertificateStatisticAPI.Tools;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using SqlSugar;
using CertificateStatisticAPI.DataModels;

namespace CertificateStatisticAPI.Controllers
{
    [Route("api/[controller]/[action]")]
    [ApiController]
    public class CertificateController : ControllerBase
    {
        private readonly SqlSugarScope DB;

        public CertificateController(SqlSugarScope DB)
        {
            this.DB = DB;
        }

        /// <summary>
        /// 插入证书项目
        /// </summary>
        /// <param name="certificate">[FromBody]特性从请求的 body 中获取一个 JSON 格式的数据，并将其反序列化成 Certificate 类型的对象</param>
        /// <returns>统一封装结果</returns>
        [HttpPost]
        public IActionResult AddCertificate([FromBody] List<Certificate> certificates)
        {
            var ResponseResult = new ResponseResult();

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

                /*
                    SELECT StudentID,CertificateProject,LEFT(Date, 4) AS Year FROM CertificateTable C
                    WHERE EXISTS (
                            SELECT 1 
                            FROM (
                                内存中的checkKeys集合，需要参数化传递
                                SELECT 
                                    @StudentID1 AS StudentID, 
                                    @CertificateProject1 AS CertificateProject, 
                                    @Year1 AS Year
                                UNION ALL
                                SELECT 
                                    @StudentID2 AS StudentID, 
                                    @CertificateProject2 AS CertificateProject, 
                                    @Year2 AS Year
                            ) AS K
                            WHERE 
                                K.StudentID = C.StudentID 
                                AND K.CertificateProject = C.CertificateProject 
                                AND C.Date LIKE K.Year + '%'
                        )
                 */

                //如果存在重复数据
                if (duplicates.Any())
                {
                    var duplicateList = string.Join("\n", duplicates.Select(d => $"{d.StudentID}-{d.CertificateProject}-{d.Year}"));
                    //对于存在的重复数据的每一项，转换为字符串格式：学号-获奖项目-年份

                    ResponseResult.Status = -1;
                    ResponseResult.Msg = $"发现重复数据：{duplicateList}";

                    return Ok(ResponseResult);
                }

                //否则正常插入
                DB.Insertable(certificates).ExecuteReturnSnowflakeIdList();
                //INSERT INTO Certificate (StudentID, CertificateProject, Date) VALUES ...
                ResponseResult.Status = 1;
                ResponseResult.Msg = "数据导入成功";
            }
            catch (Exception ex)
            {
                ResponseResult.Status = -1;
                ResponseResult.Msg = $"数据导入失败：{ex.Message}";
            }
            return Ok(ResponseResult);
        }

        /// <summary>
        /// 获得全部专业
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public IActionResult GetAllProfession()
        {
            var ResponseResult = new ResponseResult();
            try
            {
                List<Profession> professionList = DB.Queryable<Profession>().ToList();
                //SELECT * FROM ProfessionTable
                if(professionList == null)
                {
                    ResponseResult.Status = -1;
                    ResponseResult.Msg = "数据获取失败";
                    return Ok(ResponseResult);
                }
                ResponseResult.Status = 1;
                ResponseResult.Msg = "数据获取成功";
                ResponseResult.Data = professionList;
            }
            catch (Exception ex)
            {
                ResponseResult.Status = -1;
                ResponseResult.Msg = $"数据获取失败：{ex.Message}";
            }
            return Ok(ResponseResult);
        }
    }
}
