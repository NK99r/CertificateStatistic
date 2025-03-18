using CertificateStatisticWPF.Models;
using DailyApp.WPF.HttpClients;
using ImTools;
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
			LoadAvailableYear();
        }

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

		private void LoadAvailableYear()
		{
			var request = new ApiRequest
			{
				Route = "api/Statistic/GetAvailableYears",
				Method = RestSharp.Method.GET,
			};

            var response = Client.Execute(request);
			if (response.Status == 1)
			{
                var yearList = response.Data as List<string>;
				yearList.Insert(0, "全部");

                YearButtonList = new ObservableCollection<YearButton>();
				foreach (var year in yearList)
				{
					YearButtonList.Add(new YearButton { Year = year });
				}
            }
			MessageBox.Show("1");
		}

	}
}
