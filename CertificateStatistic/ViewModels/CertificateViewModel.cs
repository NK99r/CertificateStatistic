using Microsoft.Win32;
using NPOI.XSSF.UserModel;
using Prism.Commands;
using Prism.Mvvm;
using System.IO;
using System.Collections.ObjectModel;
using System.Data;
using System.Linq;
using System.Windows;
using System;
using NPOI.HSSF.UserModel;
using NPOI.SS.UserModel;
using CertificateStatisticWPF.Tools;
using CertificateStatisticWPF.Models;
using System.Windows.Controls;
using System.ComponentModel;
using System.Windows.Data;
using Org.BouncyCastle.Tls;
using System.Collections.Generic;

namespace CertificateStatistic.ViewModels
{
    internal class CertificateViewModel : BindableBase
    {
        public CertificateViewModel()
        {
            #region Excel操作初始化
            ImportExcelCommand = new DelegateCommand(LoadExcelData);

            ExportExcelCommand = new DelegateCommand(ExportExcelData);
            #endregion

            #region 管理操作初始化
            HightLightCommand = new DelegateCommand(HighlightData);

            FilterCommand = new DelegateCommand<string>(FilterData);
            #endregion
        }

        #region 读写Excel操作

        public DelegateCommand ImportExcelCommand { get; set; }

        public DelegateCommand ExportExcelCommand { get; set; }

        #region Excel表数据集合
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
        #endregion

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
                // 直接调用整合后的方法
                ProcessedExcelData = ExcelTool.ReadAndProcessExcel(openFileDialog.FileName);
                CertificateView = CollectionViewSource.GetDefaultView(ProcessedExcelData);
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
                var certificate = item as CertificateStatisticWPF.Models.Certificate;
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
                    bool isPatent = certificate.Organizer.StartsWith("ZL", StringComparison.OrdinalIgnoreCase);
                    bool isSoftware = char.IsDigit(certificate.Organizer[0]); //判断第一个字符是否为数字
                    matchPatent = isPatent || isSoftware;
                }

                //同时满足 EventLevel、Category、Organizer的条件
                return matchEventLevel && matchCategory && matchPatent;
            };

            CertificateView.Refresh();
        }
        #endregion

        #endregion



    }
}

