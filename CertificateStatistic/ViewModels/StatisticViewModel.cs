using CertificateStatisticWPF.Models;
using DailyApp.WPF.HttpClients;
using ImTools;
using Newtonsoft.Json;
using Prism.Commands;
using Prism.Mvvm;
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
        private readonly HttpRestClient Client;

        public StatisticViewModel(HttpRestClient Client)
        {
            this.Client = Client;

            //.ConfigureAwait(false)避免回到原始线程，同时防止死锁
            LoadAvailableYear().ConfigureAwait(false);
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
                    var yearList = JsonConvert.DeserializeObject<List<string>>(response.Data.ToString());
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

    }
}
