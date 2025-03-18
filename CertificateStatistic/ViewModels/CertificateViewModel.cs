﻿using Microsoft.Win32;
using Prism.Commands;
using Prism.Mvvm;
using System.Collections.ObjectModel;
using System.Data;
using System.Linq;
using System.Windows;
using System;
using CertificateStatisticWPF.Tools;
using System.ComponentModel;
using System.Windows.Data;
using System.Collections.Generic;
using DailyApp.WPF.HttpClients;
using CertificateStatisticWPF.Models;
using CertificateStatisticAPI.Tools.Enum;
using Prism.Services.Dialogs;

namespace CertificateStatistic.ViewModels
{
    internal class CertificateViewModel : BindableBase
    {
        /// <summary>
        /// RestSharp客户端
        /// </summary>
        private readonly HttpRestClient Client;

        private readonly IDialogService DialogService;

        public CertificateViewModel(HttpRestClient _client, IDialogService _dialogService)
        {
            //构造器注入
            this.Client = _client;

            //构造器注入
            this.DialogService = _dialogService;

            #region Excel操作初始化
            ImportExcelCommand = new DelegateCommand(LoadExcelData);

            ExportExcelCommand = new DelegateCommand(ExportExcelData);
            #endregion

            #region 管理操作初始化
            HightLightCommand = new DelegateCommand(HighlightData);

            FilterCommand = new DelegateCommand<string>(FilterData);
            #endregion

            #region 数据库操作初始化
            DBImportCommand = new DelegateCommand(DBImport);

            OpenPreviewDialogCommand = new DelegateCommand(OpenPreviewDialog);
            #endregion

        }

        #region 读写Excel操作

        public DelegateCommand ImportExcelCommand { get; set; }

        public DelegateCommand ExportExcelCommand { get; set; }

        /// <summary>
        /// Excel表数据集合
        /// </summary>
        private ObservableCollection<CertificateStatisticWPF.Models.Certificate> _processedExcelData;
        public ObservableCollection<CertificateStatisticWPF.Models.Certificate> ProcessedExcelData
        {
            get { return _processedExcelData; }
            set
            {
                _processedExcelData = value;
                RaisePropertyChanged();
            }
        }

        #region Excel文件操作方法

        /// <summary>
        /// 打开Window文件选择并读取Excel数据
        /// </summary>
        private void LoadExcelData()
        {
            //打开导入文件对话框
            var openFileDialog = new OpenFileDialog { Filter = "Excel Files|*.xls;*.xlsx" };
            if (openFileDialog.ShowDialog() == true)
            {
                //直接调用整合后的方法
                ProcessedExcelData = ExcelTool.ReadAndProcessExcel(openFileDialog.FileName);
                CertificateView = CollectionViewSource.GetDefaultView(ProcessedExcelData);
                //统计方法
                Statisitc();
            }
        }

        private void ExportExcelData()
        {
            try
            {
                if (ProcessedExcelData == null || ProcessedExcelData.Count == 0)
                {
                    MessageBox.Show("没有数据可以导出！");
                    return;
                }

                // 打开保存文件对话框
                var saveFileDialog = new SaveFileDialog
                {
                    Filter = "Excel Files|*.xls;*.xlsx",
                    FileName = "ExportedData.xlsx"
                };

                if (saveFileDialog.ShowDialog() == true)
                {
                    // 调用 ExcelTool 中的导出方法
                    ExcelTool.ExportToExcel(ProcessedExcelData, saveFileDialog.FileName);
                    MessageBox.Show("导出成功！");
                }
            }
            catch (Exception e)
            {
                MessageBox.Show("导出失败：" + e.Message);
            }
        }

        #endregion

        #endregion

        #region 管理操作

        public DelegateCommand HightLightCommand { get; set; }

        public DelegateCommand<string> FilterCommand { get; set; }

        #region 集合和属性

        #region 表格操作视图CertificateView
        /// <summary>
        /// CollectionView提供一个视图层，用于管理数据集合，允许方便的进行筛选、排序、分组、导航动作
        /// </summary>
        private ICollectionView _certificateView;

        /// <summary>
        /// CollectionView提供一个视图层，用于管理数据集合，允许方便的进行筛选、排序、分组、导航动作
        /// </summary>
        public ICollectionView CertificateView
        {
            get { return _certificateView; }
            set
            {
                _certificateView = value;
                RaisePropertyChanged();
            }
        }
        #endregion

        #region 筛选表FilterList
        private List<string> _filterList = new List<string>();

        public List<string> FilterList
        {
            get { return _filterList; }
            set
            {
                _filterList = value;
                RaisePropertyChanged();
            }
        }
        #endregion

        #region 文本属性SearchText
        private string _searchText;

        public string SearchText
        {
            get { return _searchText; }
            set
            {
                _searchText = value;
                RaisePropertyChanged();
                HighlightData();
            }
        }
        #endregion

        #endregion

        #region 管理方法
        /// <summary>
        /// 高亮包含搜索的关键词的行
        /// </summary>
        private void HighlightData()
        {
            //重置所有行的高亮状态
            if (ProcessedExcelData == null) return;
            foreach (var item in ProcessedExcelData)
            {
                item.IsHighlighted = false;
            }

            //如果搜索内容为空，则不清除高亮
            if (string.IsNullOrEmpty(SearchText)) return;

            //高亮符合条件的行
            foreach (var item in CertificateView)
            {
                var certificate = item as CertificateStatisticWPF.Models.Certificate;
                if (certificate == null) continue;

                bool match = certificate.StudentID.Contains(SearchText) ||
                             certificate.Name.Contains(SearchText) ||
                             certificate.CertificateProject.Contains(SearchText, StringComparison.OrdinalIgnoreCase) ||
                             certificate.EventLevel.Contains(SearchText) ||
                             certificate.Date.Contains(SearchText) ||
                             certificate.Category.Contains(SearchText);

                if (match)
                {
                    certificate.IsHighlighted = true;
                }
            }
        }

        /// <summary>
        /// 过滤筛选
        /// </summary>
        /// <param name="filter">按钮上的的文本</param>
        private void FilterData(string filter)
        {
            if (CertificateView == null)
            {
                MessageBox.Show("请先从数据库导数据或从文件导入数据");
                return;
            }
            if (FilterList.Contains(filter)) FilterList.Remove(filter); //如果已选中，移出
            else FilterList.Add(filter); //如果未选中，添加
            //ICollection类的过滤逻辑。false不显示数据，true显示数据

            CertificateView.Filter = item =>
            {
                var certificate = item as Certificate;
                if (certificate == null) return false;

                //如果没有选中任何条件，显示所有数据
                if (FilterList.Count == 0) return true;

                /*
                 * 检查 EventLevel 是否满足任一条件（或逻辑）使用linq语句
                 * eventLevelFilterList.Count == 0表示若没有选中国家级或省部级则默认返回所有数据
                 * 否则，任何certificate.EventLevel里包含至少一个条件（"国家级" 或 "省部级"）的就为true
                 * Any()可以判断List里是否有国家级或省部级或二者都有
                */
                //从总筛选表里抽出赛事级别的条件表
                List<string> eventLevelFilterList = FilterList.Where(f => f == "国家级" || f == "省部级").ToList();
                bool matchEventLevel = eventLevelFilterList.Count == 0 || eventLevelFilterList.Any(filter => certificate.EventLevel.Contains(filter));

                /*
                 * 检查 Category 是否满足任一条件（或逻辑）
                 * 需要独立处理这一筛选逻辑，因为非科研类有文体类、品德类等其他类，无法做“或”的逻辑
                */
                List<string> categoryFilterList = FilterList.Where(f => f == "科研类" || f == "非科研类").ToList();
                bool matchCategory = true;
                if (categoryFilterList.Contains("科研类") && categoryFilterList.Contains("非科研类"))
                {
                    //如果同时选中"科研类"和"非科研类"，则跳过 Category 筛选
                    matchCategory = true;
                }
                else if (categoryFilterList.Contains("科研类"))
                {
                    //如果选中了"科研类"，则只显示科研类
                    matchCategory = certificate.Category.Contains("科研类", StringComparison.OrdinalIgnoreCase);
                }
                else if (categoryFilterList.Contains("非科研类"))
                {
                    //如果选中了"非科研类"，则显示非科研类（即不包含 "科研类" 的行）
                    matchCategory = !certificate.Category.Contains("科研类", StringComparison.OrdinalIgnoreCase);
                }

                /*
                 * 检查Organizer是否满足任一条件（或逻辑）
                 * 主办单位这一列中其他单位不包含数字和字母，纯数字字母组合必表示专利或软著
                */
                bool ZLButtonSelected = FilterList.Contains("专利/软著");
                bool matchPatent = true;
                if (ZLButtonSelected)
                {
                    //判断主办单位是否以 ZL 开头或以数字开头
                    bool isPatent1 = certificate.Organizer.StartsWith("ZL", StringComparison.OrdinalIgnoreCase);
                    bool isPatent2 = certificate.Organizer.EndsWith("版权局");
                    bool isSoftware = char.IsDigit(certificate.Organizer[0]); //判断第一个字符是否为数字
                    matchPatent = isPatent1 || isSoftware || isPatent2;
                }

                //同时满足 EventLevel、Category、Organizer的条件
                return matchEventLevel && matchCategory && matchPatent;
            };

            CertificateView.Refresh();
        }
        #endregion

        #endregion

        #region 统计

        #region 统计项
        /// <summary>
        /// 总数
        /// </summary>
        private int _totalCount;
        public int TotalCount
        {
            get { return _totalCount; }
            set
            {
                _totalCount = value;
                RaisePropertyChanged();
            }
        }

        /// <summary>
        /// 总人数
        /// </summary>
        private int _totalPeople;
        public int TotalPeople
        {
            get { return _totalPeople; }
            set
            {
                _totalPeople = value;
                RaisePropertyChanged();
            }
        }

        /// <summary>
        /// 科研类总数
        /// </summary>
        private int _researchCount;
        public int ResearchCount
        {
            get { return _researchCount; }
            set
            {
                _researchCount = value;
                RaisePropertyChanged();
            }
        }

        /// <summary>
        /// 专利/软著总数
        /// </summary>
        private int _patentCount;
        public int PatentCount
        {
            get { return _patentCount; }
            set
            {
                _patentCount = value;
                RaisePropertyChanged();
            }
        }

        /// <summary>
        /// 国家级证书占比
        /// </summary>
        private double _multipleAwardPeople;
        public double MultipleAwardPeople
        {
            get { return _multipleAwardPeople; }
            set
            {
                _multipleAwardPeople = value;
                RaisePropertyChanged();
            }
        }
        #endregion

        /// <summary>
        /// linq语句统计
        /// </summary>
        private void Statisitc()
        {
            TotalCount = ProcessedExcelData.Count;
            TotalPeople = ProcessedExcelData.Select(item => item.StudentID).Distinct().Count();
            //
            var personAwardList = ProcessedExcelData.GroupBy(item => item.StudentID)    //按照学号，一个学生的多个奖项分为一组
                                                    .Select(gruop => new {StudentID = gruop.Key, AwardCount = gruop.Count() }) //每项格式都是“{StudentID = n, AwardCount = n}”
                                                    .ToList();
            MultipleAwardPeople = personAwardList.Count(p => p.AwardCount > 1);
            ResearchCount = ProcessedExcelData.Count(item => item.Category == "科研类");
            PatentCount = ProcessedExcelData.Count(item => item.Organizer.StartsWith("ZL") || char.IsDigit(item.Organizer[0]) || item.Organizer.EndsWith("版权局"));
        }

        #endregion

        #region 数据库操作
        public DelegateCommand DBImportCommand { get; set; }

        public DelegateCommand OpenPreviewDialogCommand { get; set; }

        /// <summary>
        /// 导入到数据库，应防止连续导入相同的数据
        /// </summary>
        private void DBImport()
        {
            try
            {
                if (CertificateView == null || CertificateView.IsEmpty)
                {
                    MessageBox.Show("没有导入文件或当前数据为空");
                    return;
                }

                var confirmResult = MessageBox.Show(
                    "是否确认导入当前数据？请检查无误后继续！",
                    "导入确认",
                    MessageBoxButton.OKCancel,
                    MessageBoxImage.Question
                );
                if (confirmResult != MessageBoxResult.OK) return;

                //获取当前DataGrid数据并去重（根据 StudentID + CertificateProject + 年份 分组。）
                var certificates = CertificateView.Cast<Certificate>()
                                    .GroupBy(c => new
                                    {
                                        c.StudentID,
                                        c.CertificateProject,
                                        Year = c.Date.Substring(0, 4)
                                    })
                                    .Select(g => g.First()) // 每组取第一条
                                    .ToList();
                /*  
                    .GroupBy()后可能会有这样的数据：
                    [
                        [
                            { StudentID = 123, 其他字段, CertificateProject = "奖项1", 其他字段, Date = "2022-05" }
                        ],
                        [
                            { StudentID = 123, 其他字段, CertificateProject = "奖项1", 其他字段, Date = "2021-08" }
                        ],                     
                        [                      
                            { StudentID = 456, 其他字段, CertificateProject = "奖项2", 其他字段, Date = "2022-01" },
                            { StudentID = 456, 其他字段, CertificateProject = "奖项2", 其他字段, Date = "2022-03" }
                        ],                     
                        [                      
                            { StudentID = 123, 其他字段, CertificateProject = "奖项1", 其他字段, Date = "2020-07" }
                        ]
                        ...
                    ]
                    第三组的学号-奖项-年份相同
                    因为部分人在填写Excel表格时填写的某个赛事没有具体年份或哪一届，因此仅靠 学号+获奖项目名 无法保证数据库中一定没有这条数据
                */

                if (certificates.Count == 0)
                {
                    MessageBox.Show("无有效数据可导入");
                    return;
                }

                var apiRequest = new ApiRequest<List<Certificate>>
                {
                    Route = "api/Statistic/AddCertificate",
                    Method = RestSharp.Method.POST,
                    Parameters = certificates
                };

                var apiResponse = Client.Execute(apiRequest);

                if (apiResponse.Status == ResultStatus.Success)
                {
                    MessageBox.Show($"成功导入 {certificates.Count} 条数据");
                }
                else
                {
                    MessageBox.Show($"导入失败：{apiResponse.Msg}");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"发生异常：{ex.Message}");
            }
        }

        private void OpenPreviewDialog()
        {
            var parameters = new DialogParameters();
            parameters.Add("Tip",
                  "1.如果出现Index and length must refer to a location within the string，说明学生填写的获奖项目数与日期数对不上" + "\n" 
                + "2.有专利/软著行的，应尽量填写其专利号/软著号，没有填写编号则填写“中华人民共和国国家版权局”" + "\n"
                + "3.如确实有同一学生在同一年的不同月份获得同一奖项(概率不大)，请联系我" + "\n"
                + "4.导入的原Excel表格中的日期列的格式尽量为年份/月份，确保年份在开头且为4位数" + "\n"
                + "5.有其他报错或问题，请联系我");
            parameters.Add("Preview", "pack://application:,,,/Asset/pic/Preview.png");

            DialogService.ShowDialog("PreviewDialog", parameters, result =>
            {
                //对话框返回结果，什么都不做
            });

        }
        #endregion
    }
}

