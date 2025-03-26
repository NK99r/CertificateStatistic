using CertificateStatisticAPI.DataModels.DTOs;
using CertificateStatisticWPF.Models;
using DailyApp.WPF.HttpClients;
using LiveCharts.Wpf;
using LiveCharts;
using Newtonsoft.Json;
using Prism.Commands;
using Prism.Mvvm;
using Prism.Regions;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;
using System.Windows;

namespace CertificateStatisticWPF.ViewModels
{
    internal class TotalYearsUCViewModel : BindableBase, INavigationAware
    {
        /// <summary>
        /// webapi工具
        /// </summary>
        private readonly HttpRestClient Client;

        public TotalYearsUCViewModel(HttpRestClient Client)
        {
            this.Client = Client;

            //获得所有年份
            YearLabels = new List<string>();

            //获得所有专业
            ProfessionLabels = new List<string>();

            //左上分组柱状图初始化
            YearColumnSeries = new SeriesCollection();

            //右上饼图初始化
            PieSeries = new SeriesCollection();

            //左下柱状图初始化
            ProfessionColumnSeries = new SeriesCollection();

            //右下堆叠面积图初始化
            LoadProfessionList();
            StackedAreaSeries = new SeriesCollection();
            RecentThreeYearLabels = new List<string>();
            ChartClickCommand = new DelegateCommand<ChartPoint>(ChartClick);
        }

        #region 左上分组柱状图
        /// <summary>
        /// 数据系列
        /// </summary>
        private SeriesCollection _yearColumnSeries;
        public SeriesCollection YearColumnSeries
        {
            get { return _yearColumnSeries; }
            set
            {
                _yearColumnSeries = value;
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


        private void YearColumnChartData(List<Certificate> certificates)
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
            //按年份分组（总数量）
            var yearGroups = certificates
                .GroupBy(c => c.Date.Substring(0, 4))
                .OrderBy(g => g.Key)
                .ToList();

            //按年份和级别分组（省部级/国家级）
            var levelGroups = certificates
                .GroupBy(c => new
                {
                    Year = c.Date.Substring(0, 4),
                    Level = c.EventLevel
                })
                .Select(g => new
                {
                    Year = g.Key.Year,
                    Level = g.Key.Level,
                    Count = g.Count()
                })
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

            //创建数据系列
            var totalSeries = new ColumnSeries
            {
                Title = "总数",
                Values = new ChartValues<double>(),
                Fill = Brushes.SteelBlue,
                DataLabels = true
            };

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

            // 填充数据
            foreach (var year in YearLabels)
            {
                //总数量
                var total = yearGroups.First(g => g.Key == year).Count();
                totalSeries.Values.Add((double)total);

                //省部级数量
                var provincial = levelGroups
                    .FirstOrDefault(g => g.Year == year && g.Level == "省部级")?.Count ?? 0;
                provincialSeries.Values.Add((double)provincial);

                //国家级数量
                var national = levelGroups
                    .FirstOrDefault(g => g.Year == year && g.Level == "国家级")?.Count ?? 0;
                nationalSeries.Values.Add((double)national);
            }

            YearColumnSeries.Clear();
            YearColumnSeries.Add(provincialSeries);
            YearColumnSeries.Add(totalSeries);
            YearColumnSeries.Add(nationalSeries);
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

        private void ProfessionColumnChartData(List<Certificate> certificates)
        {
            var request = new ApiRequest
            {
                Route = "api/Statistic/GetTotalYearProfessionCount",
                Method = RestSharp.Method.GET
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

        #region 右下堆叠面积图
        #region 面积集合
        /// <summary>
        /// 专业列表
        /// </summary>
        private ObservableCollection<Profession> _professionList;
        public ObservableCollection<Profession> ProfessionList
        {
            get { return _professionList; }
            set
            {
                _professionList = value;
                RaisePropertyChanged();
            }
        }

        /// <summary>
        /// 堆叠面积图数据系列
        /// </summary>
        private SeriesCollection _stackedAreaSeries;
        public SeriesCollection StackedAreaSeries
        {
            get { return _stackedAreaSeries; }
            set
            {
                _stackedAreaSeries = value;
                RaisePropertyChanged();
            }
        }

        /// <summary>
        /// X轴近三年（从去年开始）
        /// </summary>
        private List<string> _recentThreeYearLabels;
        public List<string> RecentThreeYearLabels
        {
            get { return _recentThreeYearLabels; }
            set
            {
                _recentThreeYearLabels = value;
                RaisePropertyChanged();
            }
        }
        #endregion

        /// <summary>
        /// 生成堆叠图
        /// </summary>
        /// <param name="certificates"></param>
        private void ProfessionStackedAreaData(List<Certificate> certificates)
        {
            //获得近三年
            var threeYears = new List<string>
            {
                (DateTime.Now.Year - 3).ToString(),
                (DateTime.Now.Year - 2).ToString(),
                (DateTime.Now.Year - 1).ToString()
            };

            //设置 X 轴标签
            RecentThreeYearLabels = threeYears;

            // 2. 获取所有有效 ProID（从已加载的专业列表中提取，包含 "Other"）
            var validProIDs = ProfessionList
                .Select(p => p.ProID)
                .ToList();

            //3. 按年份和专业统计证书数量（LINQ 分组）
            var groupedData = certificates
                .Where(c => threeYears.Contains(c.Year))
                .GroupBy(c =>
                {
                    //分类逻辑：ProID 有效则保留，否则归为 "Other"
                    return validProIDs.Contains(c.ProID) ? c.ProID : "Other";
                })
                .ToDictionary(
                    g => g.Key, //专业 ProID 或 "Other"
                    g => threeYears.ToDictionary(
                        year => year,
                        year => g.Count(c => c.Year == year)
                    )
                );

            StackedAreaSeries = new SeriesCollection();
            foreach (var proID in groupedData.Keys.OrderBy(k => k == "Other" ? 1 : 0)) // 其他专业排在最后
            {
                var values = new ChartValues<int>();
                foreach (var year in threeYears)
                {
                    values.Add(groupedData[proID][year]);
                }

                // 获取专业名称（若为 "Other" 显示“其他专业”）
                string professionName = proID == "Other"
                    ? "其他专业"
                    : ProfessionList.FirstOrDefault(p => p.ProID == proID)?.ProfessionName ?? proID;

                StackedAreaSeries.Add(new StackedAreaSeries
                {
                    Title = professionName,
                    Values = values,
                    Stroke = Brushes.Gray,
                    PointGeometrySize = 8,
                    StrokeThickness = 0.7,
                    DataLabels = true,
                    Foreground = Brushes.Black
                });
            }


            RaisePropertyChanged(nameof(StackedAreaSeries));
        }

        /// <summary>
        /// 加载专业列表
        /// </summary>
        private void LoadProfessionList()
        {
            try
            {
                var request = new ApiRequest
                {
                    Route = "api/Certificate/GetAllProfession",
                    Method = RestSharp.Method.GET
                };

                var response = Client.Execute(request);
                if (response.Status == 1)
                {
                    var professionList = JsonConvert.DeserializeObject<List<Profession>>(response.Data.ToString());
                    //添加 "其他专业" 选项
                    professionList.Add(new Profession { ProID = "Other", ProfessionName = "其他专业" });
                    ProfessionList = new ObservableCollection<Profession>(professionList);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("获取专业数据失败：" + ex.Message);
            }
        }


        #region 点击面积切换
        /// <summary>
        /// 原统计图全专业面积系列集合
        /// </summary>
        private SeriesCollection _originalSeriesCollection;
        public SeriesCollection OriginalSeriesCollection
        {
            get { return _originalSeriesCollection; }
            set
            {
                _originalSeriesCollection = value;
                RaisePropertyChanged();
            }
        }

        private bool _isSingleMode = false;
        public bool IsSingleMode
        {
            get { return _isSingleMode; }
            set
            {
                _isSingleMode = value;
                RaisePropertyChanged();
            }
        }

        public DelegateCommand<ChartPoint> ChartClickCommand { get; set; }

        private void ChartClick(ChartPoint selectedPoint)
        {
            if (selectedPoint?.SeriesView is StackedAreaSeries clickedSeries)
            {
                if (IsSingleMode)
                {
                    // 从单专业模式恢复
                    StackedAreaSeries = OriginalSeriesCollection;
                }
                else
                {
                    // 进入单专业模式
                    OriginalSeriesCollection = StackedAreaSeries;
                    StackedAreaSeries = new SeriesCollection { clickedSeries };
                }
                IsSingleMode = !IsSingleMode;
                RaisePropertyChanged(nameof(StackedAreaSeries));
            }
        }
        #endregion
        #endregion

        private void CreateCharts(List<Certificate> certificates)
        {
            //左上分组柱状图
            YearColumnChartData(certificates);

            //右上饼图
            PieChartData(certificates);

            //左下柱状图
            ProfessionColumnChartData(certificates);

            //右下堆叠面积图
            ProfessionStackedAreaData(certificates);
        }

        #region INavigationAware接口实现
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

        public bool IsNavigationTarget(NavigationContext navigationContext)
        {
            return true;
        }

        public void OnNavigatedFrom(NavigationContext navigationContext)
        {

        }
        #endregion

    }
}

