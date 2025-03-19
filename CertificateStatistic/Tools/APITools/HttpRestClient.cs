using CertificateStatisticAPI.Tools;
using CertificateStatisticWPF.Enum;
using Newtonsoft.Json;
using RestSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using System.Windows;

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
        public ApiResponse Execute(ApiRequest apiRequest)
        {
            RestRequest request = new RestRequest(apiRequest.Method);//请求方式 
            request.AddHeader("Content-Type", apiRequest.ContentType);//内容类型

            if (apiRequest.Parameters != null)//参数
            {
                //处理JSON序列化 对象->json字符串
                request.AddParameter("param", JsonConvert.SerializeObject(apiRequest.Parameters), ParameterType.RequestBody);
            }

            RestSharpClient.BaseUrl = new Uri(NetWorkConfig.BASEURL + apiRequest.Route);
            var res = RestSharpClient.Execute(request);//请求
            switch (res.StatusCode)
            {
                case System.Net.HttpStatusCode.OK:
                    //DeserializeObject 反序列化  json字符串->对象
                    return JsonConvert.DeserializeObject<ApiResponse>(res.Content);
                case System.Net.HttpStatusCode.BadRequest:
                    return new ApiResponse { Status = -1, Msg = "您的操作有误" };
                case System.Net.HttpStatusCode.Unauthorized:
                    return new ApiResponse { Status = -1, Msg = "身份校验失败" };
                case System.Net.HttpStatusCode.NotFound:
                    return new ApiResponse { Status = -1, Msg = "网络链接失败，请检查您的网络设置" };
                case System.Net.HttpStatusCode.BadGateway:
                    return new ApiResponse { Status = -1, Msg = "服务器被关停，请联系我" };
                case System.Net.HttpStatusCode.ServiceUnavailable:
                    return new ApiResponse { Status = -1, Msg = "服务器暂不可用" };
                case System.Net.HttpStatusCode.GatewayTimeout:
                    return new ApiResponse { Status = -1, Msg = "服务器超时，请重试" };
                default:
                    return new ApiResponse { Status = -1, Msg = "未知的错误" };
            }
        }
    }
}
