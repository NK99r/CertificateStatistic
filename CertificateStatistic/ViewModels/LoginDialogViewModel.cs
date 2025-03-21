using DailyApp.WPF.HttpClients;
using Prism.Commands;
using Prism.Events;
using Prism.Mvvm;
using Prism.Services.Dialogs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CertificateStatisticWPF.ViewModels
{
    internal class LoginDialogViewModel : BindableBase, IDialogAware
    {
        /// <summary>
        /// 事件管理器(消息队列用)
        /// </summary>
        private readonly IEventAggregator Aggregator;

        /// <summary>
        /// RestSharp客户端
        /// </summary>
        private readonly HttpRestClient Client;

        public LoginDialogViewModel(HttpRestClient Client, IEventAggregator Aggregator) 
        {
            this.Client = Client;

            this.Aggregator = Aggregator;

            #region 视图切换
            ShowRegisterContentCommand = new DelegateCommand(ShowRegisterContent);

            ShowLogin_PwdContentCommand = new DelegateCommand(ShowLogin_PwdContent);

            ShowLogin_CpaContentCommand = new DelegateCommand(ShowLogin_CpaContent);
            #endregion
        }




        #region 切换显示的内容
        /// <summary>
        /// 显示内容的索引
        /// </summary>
        private int _SelectedIndex;
        public int SelectedIndex
        {
            get { return _SelectedIndex; }
            set
            {
                _SelectedIndex = value;
                RaisePropertyChanged();
            }
        }

        public DelegateCommand ShowRegisterContentCommand { get; set; }

        public DelegateCommand ShowLogin_PwdContentCommand { get; set; }

        public DelegateCommand ShowLogin_CpaContentCommand { get; set; }

        /// <summary>
        /// 显示密码登录内容
        /// </summary>
        private void ShowLogin_PwdContent()
        {
            SelectedIndex = 0;
        }

        /// <summary>
        /// 显示短信验证码登录内容
        /// </summary>
        private void ShowLogin_CpaContent()
        {
            SelectedIndex = 1;
        }

        /// <summary>
        /// 显示注册内容
        /// </summary>
        private void ShowRegisterContent()
        {
            SelectedIndex = 2;
        }
        #endregion

        #region IDialogAware接口实现
        public string Title { get; set; } = "获奖证书管理系统登录";

        public event Action<IDialogResult> RequestClose;

        public bool CanCloseDialog()
        {
            return true;
        }

        public void OnDialogClosed()
        {
        }

        public void OnDialogOpened(IDialogParameters parameters)
        {
        }
        #endregion
    }
}
