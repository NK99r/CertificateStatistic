﻿using CertificateStatistics.Models;
using Prism.Commands;
using Prism.Mvvm;
using Prism.Regions;
using System.Collections.ObjectModel;

namespace CertificateStatistic.ViewModels
{
    public class MainWindowViewModel : BindableBase
    {
        /// <summary>
        /// prism区域管理
        /// </summary>
        private readonly IRegionManager RegionManager;

        public MainWindowViewModel(IRegionManager regionManager)
        {
            //构造器注入
            this.RegionManager = regionManager;

            #region 导航初始化
            //添加导航栏选项
            NavBarList = new ObservableCollection<NavBtn>();

            //给NavBarList添加导航对象
            CreateNavBar();

            NavigateCommand = new DelegateCommand<string>(Navigate);
            #endregion
        }

        #region 左侧导航栏功能

        /// <summary>
        /// 左侧导航集合
        /// ObservableCollection表示一个动态数据收集，该集合在添加或删除项时或刷新整个列表时提供通知。
        /// </summary>
        private ObservableCollection<NavBtn> _NavBarList;
        public ObservableCollection<NavBtn> NavBarList
        {
            get { return _NavBarList; }
            set
            {
                _NavBarList = value;
                RaisePropertyChanged();
            }
        }

        /// <summary>
        /// 左侧导航被选中项
        /// </summary>
        private NavBtn _selectedNavItem;
        public NavBtn SelectedNavItem
        {
            get { return _selectedNavItem; }
            set
            {
                _selectedNavItem = value;
                RaisePropertyChanged();
                //当前端的SelectedNavItem更新时，后台被通知，执行以下方法
                NavigateCommand.Execute(_selectedNavItem?.ViewName);
                //如果传来的参数为空，不会报异常，也什么都不做
            }
        }

        public DelegateCommand<string> NavigateCommand { get; set; }

        private void Navigate(string viewName)
        {
            if (!string.IsNullOrEmpty(viewName))
                RegionManager.RequestNavigate("MainRegion", viewName);
        }

        /// <summary>
        /// 添加导航项
        /// </summary>
        private void CreateNavBar()
        {
            NavBarList.Clear();
            NavBarList.Add(new NavBtn() { Icon = "/Asset/pic/navbar/home.png", Title = "首页", ViewName = "HomeUC" });
            NavBarList.Add(new NavBtn() { Icon = "/Asset/pic/navbar/cer.png", Title = "证书", ViewName = "CertificateUC" });
            NavBarList.Add(new NavBtn() { Icon = "/Asset/pic/navbar/stat.png", Title = "统计", ViewName = "StatisticUC" });
            NavBarList.Add(new NavBtn() { Icon = "/Asset/pic/navbar/about.png", Title = "关于", ViewName = "OtherUC" });
        }
        #endregion

        /// <summary>
        /// 默认首页
        /// </summary>
        public void SetDefaultNavigation()
        {
            RegionManager.Regions["MainRegion"].RequestNavigate("HomeUC");
        }

    }
}
