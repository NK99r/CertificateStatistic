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
    internal class MultiYearsUCViewModel : BindableBase, INavigationAware
    {

        public MultiYearsUCViewModel()
        {
            //获得所有年份
            YearLabels = new List<string>();

            //左上柱状图初始化
            ColumnSeries = new SeriesCollection(); 

            //右上饼图初始化
            PieSeries = new SeriesCollection();

            //左下分组柱状图初始化
            EventLevelColumnSeries = new SeriesCollection();
        }

        #region 左上柱状图
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

        private void ColumnChartData(List<Certificate> certificates)
        {
            /*
                var yearGroups = new Dictionary<string, List<Certificate>>();

                foreach (var certificate in certificates)
                {
                    //从日期截取年份作为键
                    string year = certificate.Date.Substring(0, 4);
                    if (!yearGroups.ContainsKey(year))
                    {
                        //如果字典中不包含本次遍历到的年份
                        yearGroups[year] = new List<Certificate>();
                        //则为其新建一个空列表
                    }
                    yearGroups[year].Add(certificate);
                }
             */
            //按年份分组并排序
            var yearGroups = certificates
                .GroupBy(c => c.Date.Substring(0, 4))
                .OrderBy(g => g.Key)
                .ToList();

            /*
                var yearLabels = new List<string>();
                foreach (var group in yearGroups)
                {
                    //将年份添加到X轴标签列表中
                    yearLabels.Add(group.Key); 
                }
             */
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

        #region 左下分组柱状图
        /// <summary>
        /// 数据系列
        /// </summary>
        private SeriesCollection _eventLevelColumnSeries;
        public SeriesCollection EventLevelColumnSeries
        {
            get { return _eventLevelColumnSeries; }
            set
            {
                _eventLevelColumnSeries = value;
                RaisePropertyChanged();
            }
        }

        private void EvenLevelColumnChartData(List<Certificate> certificates)
        {
            //按年份和级别分组
            var groupedData = certificates
                .GroupBy(c => new { Year = c.Date.Substring(0, 4), Level = c.EventLevel }) // 按年份和级别分组
                .Select(g => new
                {
                    Year = g.Key.Year,
                    Level = g.Key.Level,
                    Count = g.Count()
                })
                .ToList();

            //创建分组柱状图数据
            var provincialSeries = new ColumnSeries
            {
                Title = "省部级",
                Values = new ChartValues<double>(),
                Fill = Brushes.Orange,
                DataLabels = true
            };

            var nationalSeries = new ColumnSeries
            {
                Title = "国家级",
                Values = new ChartValues<double>(),
                Fill = Brushes.OrangeRed,
                DataLabels = true
            };

            //填充数据
            foreach (var year in YearLabels)
            {
                //省部级数量
                var provincialCount = groupedData
                    .FirstOrDefault(g => g.Year == year && g.Level == "省部级")?.Count ?? 0;
                provincialSeries.Values.Add((double)provincialCount);

                //国家级数量
                var nationalCount = groupedData
                    .FirstOrDefault(g => g.Year == year && g.Level == "国家级")?.Count ?? 0;
                nationalSeries.Values.Add((double)nationalCount);
            }

            EventLevelColumnSeries.Clear();
            EventLevelColumnSeries.Add(provincialSeries);
            EventLevelColumnSeries.Add(nationalSeries);
        }
        #endregion

        private void CreateCharts(List<Certificate> certificates)
        {
            //左上柱状图
            ColumnChartData(certificates);

            //右上饼图
            PieChartData(certificates);

            //左下分组柱状图
            EvenLevelColumnChartData(certificates);
        }

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
                //导航到该页时，执行此方法
                CreateCharts(certificates);
            }
        }
        #endregion
    }
}
