﻿using CertificateStatistic.ViewModels;
using CertificateStatistic.Views;
using CertificateStatistics.Views;
using Prism.Ioc;
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

        protected override void RegisterTypes(IContainerRegistry containerRegistry)
        {
            containerRegistry.RegisterForNavigation<HomeUC, HomeUCViewModel>();
        }
    }
}
