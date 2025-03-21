using CertificateStatisticAPI.Tools;
using CertificateStatisticWPF.Models.DTOs;
using CertificateStatisticWPF.Tools;
using DailyApp.WPF.HttpClients;
using DailyApp.WPF.MsgEvents;
using Newtonsoft.Json;
using Prism.Commands;
using Prism.Events;
using Prism.Mvvm;
using Prism.Services.Dialogs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
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

            #region 登陆注册操作
            AccountDTO = new AccountDTO();

            RegisterCommand = new DelegateCommand(Register);

            LoginCommand = new DelegateCommand(Login);
            #endregion
        }

        /// <summary>
        /// 账号信息
        /// </summary>
        private AccountDTO _accountDTO;
        public AccountDTO AccountDTO
        {
            get { return _accountDTO; }
            set
            {
                _accountDTO = value;
                RaisePropertyChanged();
            }
        }

        #region 密码
        /// <summary>
        /// 密码
        /// </summary>
        private string _pwd;
        public string Pwd
        {
            get { return _pwd; }
            set
            {
                _pwd = value;
                AccountDTO.Pwd = value;  //自动更新AccountDTO.Pwd
                RaisePropertyChanged();
            }
        }

        /// <summary>
        /// 确认密码
        /// </summary>
        private string _confirmPwd;
        public string ConfirmPwd
        {
            get { return _confirmPwd; }
            set
            {
                _confirmPwd = value;
                AccountDTO.ConfirmPwd = value;  //自动更新AccountDTO.ConfirmPwd
                RaisePropertyChanged();
            }
        }
        #endregion

        #region 注册
        public DelegateCommand RegisterCommand { get; set; }

        private void Register()
        {
            //基本数据验证
            //验证手机号长度
            if (AccountDTO.PhoneNum == null || AccountDTO.PhoneNum.Length != 11)
            {
                Aggregator.GetEvent<MsgEvent>().Publish("手机号不足11位");
                return;
            }

            if (string.IsNullOrEmpty(AccountDTO.PhoneNum) || string.IsNullOrEmpty(AccountDTO.Pwd) || string.IsNullOrEmpty(AccountDTO.ConfirmPwd))
            {
                //将"注册信息不全"广播给订阅者(LoginDialog)
                Aggregator.GetEvent<MsgEvent>().Publish("注册信息不全");
                return;
            }

            if (AccountDTO.Pwd != AccountDTO.ConfirmPwd)
            {
                Aggregator.GetEvent<MsgEvent>().Publish("两次密码输入不一致");
                return;
            }

            //生成盐值
            string salt = EncryptionTool.GenerateSalt();
            AccountDTO.Salt = salt;

            string hashedPassword = EncryptionTool.HashPassword(AccountDTO.Pwd, salt);
            AccountDTO.Pwd = hashedPassword;

            ApiRequest apiRequest = new ApiRequest();
            apiRequest.Method = RestSharp.Method.POST;
            apiRequest.Route = "api/Account/Register";
            apiRequest.Parameters = AccountDTO;

            ApiResponse response = Client.Execute(apiRequest);  //请求Api
            if (response.Status == 1)
            {
                Aggregator.GetEvent<MsgEvent>().Publish(response.Msg);
                SelectedIndex = 0;  //注册成功，切换到登录模块
            }
            else
            {
                Aggregator.GetEvent<MsgEvent>().Publish(response.Msg);
            }
        }
        #endregion

        #region 登录
        public DelegateCommand LoginCommand { get; set; }

        private void Login()
        {
            // 数据基本验证
            if (AccountDTO.PhoneNum == null || AccountDTO.PhoneNum.Length != 11)
            {
                Aggregator.GetEvent<MsgEvent>().Publish("手机号不足11位");
                return;
            }

            if (string.IsNullOrEmpty(Pwd))
            {
                Aggregator.GetEvent<MsgEvent>().Publish("登录信息不全");
                return;
            }

            //获取用户的盐值
            ApiRequest saltRequest = new ApiRequest();
            saltRequest.Method = RestSharp.Method.GET;
            saltRequest.Route = $"api/Account/GetSalt?phoneNum={AccountDTO.PhoneNum}";

            var saltResponse = Client.Execute(saltRequest);
            if (saltResponse.Status != 1)
            {
                Aggregator.GetEvent<MsgEvent>().Publish("获取用户信息失败，确保手机号输入正确");
                return;
            }

            AccountDTO.Salt = saltResponse.Data.ToString();

            //对用户输入的密码进行加密
            string hashedPassword = EncryptionTool.HashPassword(Pwd, AccountDTO.Salt);
            AccountDTO.Pwd = hashedPassword;

            //发送登录请求
            ApiRequest loginRequest = new ApiRequest();
            loginRequest.Method = RestSharp.Method.POST;
            loginRequest.Route = "api/Account/Login";
            loginRequest.Parameters = AccountDTO;

            var loginResponse = Client.Execute(loginRequest);
            if (loginResponse.Status == 1)
            {
                if (RequestClose != null)
                {
                    RequestClose(new DialogResult(ButtonResult.OK));
                }
            }
            else
            {
                Aggregator.GetEvent<MsgEvent>().Publish(loginResponse.Msg);
            }
        }
        #endregion

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
