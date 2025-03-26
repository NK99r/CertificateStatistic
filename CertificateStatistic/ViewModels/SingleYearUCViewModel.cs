using CertificateStatisticAPI.DataModels.DTOs;
using CertificateStatisticWPF.Models;
using DailyApp.WPF.HttpClients;
using LiveCharts;
using LiveCharts.Wpf;
using Newtonsoft.Json;
using Prism.Mvvm;
using Prism.Regions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;

namespace CertificateStatisticWPF.ViewModels
{
    internal class SingleYearUCViewModel : BindableBase, INavigationAware
    {
        private readonly HttpRestClient Client;

        public SingleYearUCViewModel(HttpRestClient Client)
        {
            this.Client = Client;

            //获得所有专业
            ProfessionLabels = new List<string>();

            //右上饼图初始化
            PieSeries = new SeriesCollection();

            //左下柱状图初始化
            ProfessionColumnSeries = new SeriesCollection();
        }

        #region 右上饼图
        /// <summary>
        /// 数据系列
        /// </summary>
        private SeriesCollection _pieSeries;
        public SeriesCollection PieSeries
        {
            get { return _pieSeries; }
            set
            {
                _pieSeries = value;
                RaisePropertyChanged();
            }
        }

        private void PieChartData(List<Certificate> certificates)
        {
            /*
                var categoryGroups = new Dictionary<string, List<Certificate>>();

                foreach (var certificate in certificates)
                {
                    if (!categoryGroups.ContainsKey(certificate.Category))
                    {
                        //如果字典中不包含本次遍历到的类别名称
                        categoryGroups[certificate.Category] = new List<Certificate>();
                        //则为其新建一个空列表
                    }
                    //把本证书添加进这个类别的列表
                    categoryGroups[certificate.Category].Add(certificate);
                }
             */
            //按照类别分组并统计各类别的数量
            var categoryGroups = certificates
                .GroupBy(c => c.Category)
                .ToList();


            /*
                var pieValues = new List<PieSeries>();

                foreach (var group in categoryGroups)
                {
                    //遍历字典，以类别名(键)为Title,对应的数量(值.count)为Values
                    var pieSeries = new PieSeries
                    {
                        Title = group.Key, // 类别名称
                        Values = new ChartValues<int> { group.Value.Count } // 类别对应的数量
                    };
                    pieValues.Add(pieSeries);
                }
             */
            //构建饼图数据
            var pieValues = categoryGroups
                .Select(g => new PieSeries
                {
                    Title = g.Key,
                    Values = new ChartValues<int> { g.Count() },
                })
                .ToList();

            PieSeries.Clear();
            // 添加新的饼图数据系列
            foreach (var pie in pieValues)
            {
                PieSeries.Add(pie);
            }
        }

        #endregion

        #region 左下柱状图
        /// <summary>
        /// 数据系列
        /// </summary>
        private SeriesCollection _professionColumnSeries;
        public SeriesCollection ProfessionColumnSeries
        {
            get { return _professionColumnSeries; }
            set
            {
                _professionColumnSeries = value;
                RaisePropertyChanged();
            }
        }

        /// <summary>
        /// X轴年份指标集合
        /// </summary>
        private List<string> _professionLabels;
        public List<string> ProfessionLabels
        {
            get { return _professionLabels; }
            set
            {
                _professionLabels = value;
                RaisePropertyChanged();
            }
        }

        private void ProfessionColumnChartData(List<Certificate> certificates, string year)
        {
            var request = new ApiRequest
            {
                Route = $"api/Statistic/GetSingleYearProfessionCount?year={year}",
                Method = RestSharp.Method.GET,
            };
            var response = Client.Execute(request);

            if (response.Status == 1)
            {
                var stats = JsonConvert.DeserializeObject<List<ProfessionCountDTO>>(response.Data.ToString());
                ProfessionLabels = stats.Select(s => s.ProfessionName).ToList();
                var values = stats.Select(s => (double)s.Count).ToList();

                ProfessionColumnSeries = new SeriesCollection
                {
                    new ColumnSeries
                    {
                        Title = "专业分布",
                        Values = new ChartValues<double>(values),
                        Fill = Brushes.SkyBlue,
                        DataLabels = true
                    }
                };
            }
        }
        #endregion

        private void CreateCharts(List<Certificate> certificates,string year)
        {
            //右上饼图
            PieChartData(certificates);

            //左下柱状图
            ProfessionColumnChartData(certificates,year);
        }

        #region INavigationAware接口实现
        public void OnNavigatedTo(NavigationContext navigationContext)
        {
            //从StatisticViewModel中传来的键值对集合，如果能找到键为CertificateList的值，处理数据
            //out关键字用于若没找到值，默认为null
            if (navigationContext.Parameters.TryGetValue("CertificateList", out List<Certificate> certificates))
            {
                //导航到该页时，执行此方法
                navigationContext.Parameters.TryGetValue("Year", out string year);
                CreateCharts(certificates,year);
            }
        }

        public bool IsNavigationTarget(NavigationContext navigationContext)
        {
            return true;
        }

        public void OnNavigatedFrom(NavigationContext navigationContext){}
        #endregion
    }
}
