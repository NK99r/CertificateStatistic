using CertificateStatisticAPI.Tools;
using CertificateStatisticAPI.Tools.Enum;
using CertificateStatisticWPF.Enum;
using Newtonsoft.Json;
using RestSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace DailyApp.WPF.HttpClients
{
    /// <summary>
    /// 调用api 工具类
    /// </summary>
    internal class HttpRestClient
    {
        private readonly RestClient RestSharpClient;//客户端

        /// <summary>
        /// 构造函数
        /// </summary>
        public HttpRestClient()
        {
            RestSharpClient = new RestClient();
        }

        /// <summary>
        /// 请求
        /// </summary>
        /// <param name="apiRequest">请求数据</param>
        /// <returns>接收的数据</returns>
        public ApiResponse<T> Execute<T>(ApiRequest<T> apiRequest)
        {
            RestRequest request = new RestRequest(apiRequest.Method);//请求方式 
            request.AddHeader("Content-Type", apiRequest.ContentType);//内容类型

            if (apiRequest.Parameters != null)//参数
            {
                //自动处理JSON序列化 对象->json字符串
                request.AddJsonBody(apiRequest.Parameters);
            }

            RestSharpClient.BaseUrl = new Uri(NetWorkConfig.BASEURL + apiRequest.Route);
            var res = RestSharpClient.Execute(request);//请求
            if (res.StatusCode == System.Net.HttpStatusCode.OK)//请求成功
            {
                //DeserializeObject 反序列化  json字符串->对象
                return JsonConvert.DeserializeObject<ApiResponse<T>>(res.Content);
            }
            else
            {
                return new ApiResponse<T> { Status = ResultStatus.Error, Msg = "服务器忙，请稍后" };
            }
        }
    }
}
