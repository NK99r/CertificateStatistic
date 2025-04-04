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
using Prism.Services.Dialogs;
using Newtonsoft.Json;

namespace CertificateStatistic.ViewModels
{
    internal class CertificateViewModel : BindableBase
    {
        /// <summary>
        /// RestSharp客户端
        /// </summary>
        private readonly HttpRestClient Client;

        /// <summary>
        /// 对话服务
        /// </summary>
        private readonly IDialogService DialogService;

        public CertificateViewModel(HttpRestClient Client, IDialogService DialogService)
        {
            //构造器注入
            this.Client = Client;

            //构造器注入
            this.DialogService = DialogService;

            #region Excel操作初始化
            ImportExcelCommand = new DelegateCommand(LoadExcelData);

            ExportExcelCommand = new DelegateCommand(ExportExcelData);
            #endregion

            #region 管理操作初始化
            HightLightCommand = new DelegateCommand(HighlightData);

            FilterCommand = new DelegateCommand<string>(FilterData);

            LoadProfessionList();
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
                //调用整合后的方法
                ProcessedExcelData = ExcelTool.ReadAndProcessExcel(openFileDialog.FileName);
                CertificateView = CollectionViewSource.GetDefaultView(ProcessedExcelData);
                //统计方法
                Statisitc();
            }
        }

        /// <summary>
        /// 导出当前当前DataGridView控件里的数据
        /// </summary>
        private void ExportExcelData()
        {
            try
            {
                if (ProcessedExcelData == null || ProcessedExcelData.Count == 0)
                {
                    MessageBox.Show("没有数据可以导出！");
                    return;
                }

                //打开保存文件对话框
                var saveFileDialog = new SaveFileDialog
                {
                    Filter = "Excel Files|*.xls;*.xlsx",
                    FileName = "ExportedData.xlsx"
                };

                if (saveFileDialog.ShowDialog() == true)
                {
                    //取得当前DataGridView控件里的数据
                    ObservableCollection<Certificate> filterData = new ObservableCollection<Certificate>(CertificateView.Cast<Certificate>().ToList());
                    //调用ExcelTool中的导出方法
                    ExcelTool.ExportToExcel(filterData, saveFileDialog.FileName);
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
   
        /// <summary>
        /// CollectionView提供一个视图层，用于管理数据集合，允许方便的进行筛选、排序、分组、导航动作
        /// </summary>
        private ICollectionView _certificateView;
        public ICollectionView CertificateView
        {
            get { return _certificateView; }
            set
            {
                _certificateView = value;
                RaisePropertyChanged();
            }
        }
        /// <summary>
        /// 筛选表
        /// </summary>
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

        /// <summary>
        /// 搜索框绑定的文本
        /// </summary>
        private string _searchText;
        public string SearchText
        {
            get { return _searchText; }
            set
            {
                _searchText = value;
                RaisePropertyChanged();
                //高亮搜索到的行
                HighlightData();
            }
        }

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
        /// 被选择的专业
        /// </summary>
        private Profession _selectedProfession;
        public Profession SelectedProfession
        {
            get { return _selectedProfession; }
            set
            {
                _selectedProfession = value;
                RaisePropertyChanged();
                // 触发筛选
                if (value != null)
                {
                    FilterCommand.Execute(value.ProID);
                }
            }
        }

        #region 管理方法
        /// <summary>
        /// 高亮包含搜索的关键词的行
        /// </summary>
        private void HighlightData()
        {
            if (ProcessedExcelData == null) return;
            //重置所有行的高亮状态
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

            //ICollection过滤链，对于每一行数据
            CertificateView.Filter = item =>
            {
                var certificate = item as Certificate;
                if (certificate == null) return false;

                //如果没有选中任何条件，显示所有数据
                if (FilterList.Count == 0) return true;

                /*
                 * 检查 EventLevel 是否满足任一条件（或逻辑）使用linq语句
                 * eventLevelFilterList.Count == 0表示若没有选中国家级或省部级则默认返回所有数据
                 * 否则，任何certificate.EventLevel里包含至少一个条件（"国家级" 或 "省部级"）的就为truefgg
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
                    //如果同时选中"科研类"和"非科研类"，则跳过Category筛选
                    matchCategory = true;
                }
                else if (categoryFilterList.Contains("科研类"))
                {
                    //如果选中了"科研类"，则只显示科研类
                    matchCategory = certificate.Category.Contains("科研类", StringComparison.OrdinalIgnoreCase);
                }
                else if (categoryFilterList.Contains("非科研类"))
                {
                    //如果选中了"非科研类"，则显示非科研类（即不是"科研类"的行）
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

                bool matchProfession = true;
                if (SelectedProfession != null)
                {
                    // 从 StudentID 倒数第4位提取专业号
                    string studentId = certificate.StudentID;
                    string proIdFromStudent = "Other"; // 默认视为其他专业

                    if (!string.IsNullOrEmpty(studentId) && studentId.Length >= 4)
                    {
                        proIdFromStudent = studentId.Substring(studentId.Length - 4, 1);
                    }

                    //判断是否匹配
                    if (SelectedProfession.ProID == "Other")
                    {
                        //"其他专业"：检查专业号是否不在已知 ProID 列表中
                        var validProIds = ProfessionList
                            .Where(p => p.ProID != "Other")
                            .Select(p => p.ProID)
                            .ToList();
                        matchProfession = !validProIds.Contains(proIdFromStudent);
                    }
                    else
                    {
                        matchProfession = (proIdFromStudent == SelectedProfession.ProID);
                    }
                }

                //同时满足 EventLevel、Category、Organizer、Profession的条件
                return matchEventLevel && matchCategory && matchPatent && matchProfession;
            };

            CertificateView.Refresh();
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
                    // 添加"其他专业"选项
                    professionList.Add(new Profession { ProID = "Other", ProfessionName = "其他专业" });
                    ProfessionList = new ObservableCollection<Profession>(professionList);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("获取专业数据失败：" + ex.Message);
            }
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

                if (certificates.Count == 0)
                {
                    MessageBox.Show("无有效数据可导入");
                    return;
                }

                var apiRequest = new ApiRequest
                {
                    Route = "api/Certificate/AddCertificate",
                    Method = RestSharp.Method.POST,
                    Parameters = certificates
                };

                var apiResponse = Client.Execute(apiRequest);

                if (apiResponse.Status == 1)
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
                + "2.如果出现Unable to cast object of type 'MS.Internal.NamedObject' to type 'CertificateStatisticWPF.Models.Certificate'，说明您还没有关闭编辑模式" + "\n"
                + "3.如确实有同一学生在同一年的不同月份获得同一奖项(概率不大)，请联系我" + "\n"
                + "4.导入的原Excel表格中的日期列的格式尽量为年份/月份，确保年份在开头且为4位数" + "\n"
                + "5.有专利/软著行的，应尽量填写其专利号/软著号，没有填写编号则填写“中华人民共和国国家版权局”"
                + "6.有其他报错或问题，请联系我");
            parameters.Add("Preview", "pack://application:,,,/Asset/pic/Preview.png");

            DialogService.ShowDialog("PreviewDialog", parameters, result =>
            {
                //对话框返回结果，什么都不做
            });

        }
        #endregion
    }
}

