using CertificateStatisticWPF.Models;
using DailyApp.WPF.HttpClients;
using Prism.Mvvm;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CertificateStatisticWPF.ViewModels
{
    class StatisticViewModel:BindableBase
    {
        private readonly HttpRestClient Client;

        public StatisticViewModel(HttpRestClient Client)
        {
            this.Client = Client;
            LoadAvailableYears();
        }

        #region 加载年份选项

        #region 年份选项集合
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
        #endregion

        private void LoadAvailableYears()
        {
            YearButtonList = new ObservableCollection<YearButton>();

            ApiRequest<string> request = new ApiRequest<string>
            {
                Route = "api/Statistic/GetAvailableYears",
                Method = RestSharp.Method.GET,
            };
            var response = Client.Execute(request);
            if (response.Status == 1)
            {
                var yearList = response.Data.ToList();
                //yearList.Insert(0, "全部");
                /*foreach (var year in yearList)
                {
                    YearButtonList.Add(new YearButton { Year = year });
                }*/
            }

        }
        #endregion
    }
}
