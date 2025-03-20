using Prism.Events;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DailyApp.WPF.MsgEvents
{
    /// <summary>
    /// 发布订阅 信息model xaml页面实例化的时候就要开始订阅
    /// </summary>
    internal class MsgEvent :PubSubEvent<string>
    {
        //什么都不需要做，因为其继承了prism框架的PubSubEvent<string>，已具备发布订阅的功能
    }
}
