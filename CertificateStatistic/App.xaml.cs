using CertificateStatistic.ViewModels;
using CertificateStatistic.Views;
using CertificateStatistics.Views;
using CertificateStatisticWPF.ViewModels;
using CertificateStatisticWPF.Views;
using CertificateStatisticWPF.Views.ChartsUC;
using CertificateStatisticWPF.Views.Dialogs;
using Prism.Ioc;
using Prism.Services.Dialogs;
using System.Windows;

namespace CertificateStatistic
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App
    {
        protected override Window CreateShell()
        {
            return Container.Resolve<MainWindow>();
        }

        /// <summary>
        /// 依赖注入容器
        /// </summary>
        /// <param name="containerRegistry"></param>
        protected override void RegisterTypes(IContainerRegistry containerRegistry)
        {
            containerRegistry.RegisterForNavigation<HomeUC, HomeUCViewModel>();
            containerRegistry.RegisterForNavigation<CertificateUC, CertificateViewModel>();
            containerRegistry.RegisterForNavigation<StatisticUC, StatisticViewModel>();

            //统计页导航
            containerRegistry.RegisterForNavigation<AllYearsUC, AllYearsUCViewModel>();
            containerRegistry.RegisterForNavigation<RecentFiveYearsUC, RecentFiveYearsUCViewModel>();
            containerRegistry.RegisterForNavigation<SingleYearUC, SingleYearUCViewModel>();

            containerRegistry.RegisterDialog<PreviewDialog, PreviewDialogViewModel>();
            containerRegistry.RegisterDialog<LoginDialog, LoginDialogViewModel>();
        }

        /*
        /// <summary>
        /// 在主窗口初始化之前执行的方法
        /// </summary>
        /// <param name="shell"></param>
        protected override void InitializeShell(Window shell)
        {
            //依赖注入容器获得IDialogService的实例
            var dialogService = Container.Resolve<IDialogService>();
            //打开登录对话框
            dialogService.ShowDialog("LoginDialog", new DialogParameters(), result =>
            {
                //result为回调函数，在对话框关闭时执行，其方法体表示对话框的返回结果
                if (result.Result != ButtonResult.OK)
                {
                    Application.Current.Shutdown();
                }
                else
                {
                    //MainWindow初始化
                    if (Current.MainWindow == null)
                    {
                        Current.MainWindow = shell;
                    }

                    if (Current.MainWindow.DataContext == null)
                    {
                        //依赖注入获得主页面数据上下文
                        Current.MainWindow.DataContext = Container.Resolve<MainWindowViewModel>();
                    }

                    var mainViewModel = Current.MainWindow.DataContext as MainWindowViewModel;
                    if (mainViewModel != null)
                    {
                        //默认导航到首页
                        mainViewModel.SetDefaultNavigation();
                    }
                    //显示主窗口
                    shell.Show();
                }
            });
        }
        */
    }
}
