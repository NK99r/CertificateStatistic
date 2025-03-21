using AutoMapper;
using CertificateStatisticAPI.DataModels;
using CertificateStatisticAPI.DataModels.DTOs;

namespace DailyApp.API.AutoMappers
{
    /// <summary>
    /// model之间转换设置
    /// </summary>
    public class AutoMapperSettings:Profile
    {
        public AutoMapperSettings()
        {
            //用户账号映射
            CreateMap<AccountDTO, Account>().ReverseMap();
        }
    }
}
