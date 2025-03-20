using CertificateStatistic.ViewModels;
using CertificateStatistic.Views;
using CertificateStatistics.Views;
using CertificateStatisticWPF.ViewModels;
using CertificateStatisticWPF.Views;
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
            containerRegistry.RegisterForNavigation<StatisticChartsUC, StatisticChartsViewModel>();

            containerRegistry.RegisterDialog<PreviewDialog, PreviewDialogViewModel>();
            containerRegistry.RegisterDialog<LoginDialog, LoginDialogViewModel>();
        }

        protected override void InitializeShell(Window shell)
        {
            var dialogService = Container.Resolve<IDialogService>();
            dialogService.ShowDialog("LoginDialog", new DialogParameters(), result =>
            {
                if (result.Result != ButtonResult.OK)
                    Application.Current.Shutdown();
                else
                    shell.Show();
            });
        }
    }
}
