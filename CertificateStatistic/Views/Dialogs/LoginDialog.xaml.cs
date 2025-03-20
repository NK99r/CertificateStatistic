using DailyApp.WPF.MsgEvents;
using NPOI.SS.Formula.Functions;
using Prism.Events;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace CertificateStatisticWPF.Views.Dialogs
{
    /// <summary>
    /// LoginDialog.xaml 的交互逻辑
    /// </summary>
    public partial class LoginDialog : UserControl
    {
        /// <summary>
        /// 事件管理器(消息队列用)
        /// </summary>
        private readonly IEventAggregator Aggregator;

        public LoginDialog(IEventAggregator Aggregator)
        {
            InitializeComponent();

            this.Aggregator = Aggregator;

            //订阅事件，当MsgEvent被发布时，自动调用Sub方法
            Aggregator.GetEvent<MsgEvent>().Subscribe(Sub);
        }

        /// <summary>
        /// 订阅事件执行的业务
        /// 某消息obj广播给LoginUC后，Sub方法被触发
        /// </summary>
        /// <param name="obj">接受订阅的信息</param>
        private void Sub(string obj)
        {
            //消息obj加入Snackbar的显示瑞列
            ErrorMsgBar.MessageQueue.Enqueue(obj);
        }

    }
}
