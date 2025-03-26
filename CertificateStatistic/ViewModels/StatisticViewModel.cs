using CertificateStatisticWPF.Models;
using DailyApp.WPF.HttpClients;
using ImTools;
using Newtonsoft.Json;
using Prism.Commands;
using Prism.Mvvm;
using Prism.Regions;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace CertificateStatisticWPF.ViewModels
{
    class StatisticViewModel:BindableBase
    {
        //webapi工具
        private readonly HttpRestClient Client;

        //prism区域管理
        private readonly IRegionManager RegionManager;

        public StatisticViewModel(HttpRestClient Client, IRegionManager RegionManager)
        {
            //构造器注入
            this.Client = Client;

            //构造器注入
            this.RegionManager = RegionManager;

            //.ConfigureAwait(false)避免回到原始线程，同时防止死锁
            LoadAvailableYear().ConfigureAwait(false);

            //异步lambda，async表示其为异步方法，接受一个year参数并传入方法异步执行
            SelectYearCommand = new DelegateCommand<string>(async (year) => await SelectYear(year));
        }

        /// <summary>
        /// 年份按钮集合
        /// </summary>
		private ObservableCollection<YearButton> _yearButtonList;
		public ObservableCollection<YearButton> YearButtonList
		{
			get { return _yearButtonList; }
			set
			{
				_yearButtonList = value;
				RaisePropertyChanged();
			}
		}

        /// <summary>
        /// 加载年份按钮，async表示异步方法，允许加载时程序做其他事情(不阻塞UI)
        /// </summary>
        /// <returns>Task用于没有返回值的异步操作，表示异步操作的执行过程</returns>
        private async Task LoadAvailableYear()
		{
			try
			{
                var request = new ApiRequest
                {
                    Route = "api/Statistic/GetAvailableYears",
                    Method = RestSharp.Method.GET,
                };

                //Client.Execute(request)放入线程池执行，避免阻塞UI
                //await表示等待Task.Run完成并获得返回值
                var response = await Task.Run(() => Client.Execute(request));
                if (response.Status == 1)
                {
                    List<string> yearList = JsonConvert.DeserializeObject<List<string>>(response.Data.ToString());
                    yearList.Reverse();
                    yearList.Insert(0, "全部");
                    //添加按钮
                    YearButtonList = new ObservableCollection<YearButton>();
                    foreach (var year in yearList)
                    {
                        YearButtonList.Add(new YearButton { Year = year });
                    }
                }
            }
			catch (Exception ex)
			{
                MessageBox.Show("获取数据异常:" + ex.Message);
			}
		}

        public DelegateCommand<string> SelectYearCommand { get; set; }

        private async Task SelectYear(string year)
        {
            try
            {
                if (year != "全部")
                {
                    year = null;
                }
                var request = new ApiRequest 
                {
                    Route = "api/Statistic/GetByYear",
                    Method = RestSharp.Method.POST,
                    Parameters = year
                };

                var response = await Task.Run(() => Client.Execute(request));

                if (response.Status == 1)
                {
                    string viewName = "";
                    if (year == "全部")
                    {
                        viewName = "TotalYearsUC";
                    }
                    else
                    {
                        viewName = "SingleYearUC";
                    }

                    List<Certificate> certificateList = JsonConvert.DeserializeObject<List<Certificate>>(response.Data.ToString());
                    //把这个集合放入键值对，并从这个StatisticUC传给StatisitcChartsUC
                    var parameters = new NavigationParameters 
                    {
                        { "CertificateList", certificateList },
                        { "Year", year }
                    };
                    RegionManager.RequestNavigate("StatisticChartsRegion", viewName, parameters);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("获取数据异常:" + ex.Message);
            }
        }

    }
}
