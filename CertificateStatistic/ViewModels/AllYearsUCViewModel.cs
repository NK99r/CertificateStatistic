using LiveCharts.Wpf;
using LiveCharts;
using Prism.Mvvm;
using Prism.Regions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CertificateStatisticWPF.Models;
using System.Windows.Media;

namespace CertificateStatisticWPF.ViewModels
{
    internal class AllYearsUCViewModel : BindableBase, INavigationAware
    {

        public AllYearsUCViewModel()
        {
            #region 左上柱状图
            //柱状图初始化
            ColumnSeries = new SeriesCollection();
            //x轴年份初始化
            YearLabels = new List<string>();
            #endregion
        }

        #region 左上角柱状图
        /// <summary>
        /// 数据系列
        /// </summary>
        private SeriesCollection _columnSeries;
        public SeriesCollection ColumnSeries
        {
            get { return _columnSeries; }
            set
            {
                _columnSeries = value;
                RaisePropertyChanged();
            }
        }

        /// <summary>
        /// X轴年份指标集合
        /// </summary>
        private List<string> _yearLabels;
        public List<string> YearLabels
        {
            get { return _yearLabels; }
            set
            {
                _yearLabels = value;
                RaisePropertyChanged();
            }
        }

        private void ProcessData(List<Certificate> certificates)
        {
            //按年份分组并排序
            var yearGroups = certificates
                .GroupBy(c => c.Date.Substring(0, 4))
                .OrderBy(g => g.Key)
                .ToList();

            //设置X轴标签
            YearLabels = yearGroups.Select(g => g.Key).ToList();

            //生成柱状图数据
            var columnValues = yearGroups
                .Select(g => (double)g.Count())
                .ToList();

            ColumnSeries.Clear();
            //添加柱状图系列
            ColumnSeries.Add(new ColumnSeries
            {
                Values = new ChartValues<double>(columnValues),
                Title = "年度数量",
                Fill = Brushes.SteelBlue,
                DataLabels = true
            });
        }
        #endregion

        #region INavigationAware接口实现
        public bool IsNavigationTarget(NavigationContext navigationContext)
        {
            return true;
        }

        public void OnNavigatedFrom(NavigationContext navigationContext)
        {

        }

        public void OnNavigatedTo(NavigationContext navigationContext)
        {
            //从StatisticViewModel中传来的键值对集合，如果能找到键为CertificateList的值，处理数据
            //out关键字用于若没找到值，默认为null
            if (navigationContext.Parameters.TryGetValue("CertificateList", out List<Certificate> certificates))
            {
                ProcessData(certificates);
            }
        }
        #endregion
    }
}
