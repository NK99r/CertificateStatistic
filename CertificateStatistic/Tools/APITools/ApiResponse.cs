

namespace CertificateStatisticAPI.Tools
{
    /// <summary>
    /// 统一封装返回结果
    /// </summary>
    public class ApiResponse<T>
    {
        /// <summary>
        /// 状态结果
        /// </summary>
        public int Status { get; set; }

        /// <summary>
        /// 消息描述
        /// </summary>
        public string Msg { get; set; }

        /// <summary>
        /// 返回结果
        /// </summary>
        public T Data { get; set; }
    }
}
