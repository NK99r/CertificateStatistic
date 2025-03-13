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

        }

        #region 读写Excel操作

        public DelegateCommand ImportExcelCommand { get; set; }

        public DelegateCommand ExportExcelCommand { get; set; }

        #region Excel表数据集合

        #region Excel原始数据，用于备用保留
        /// <summary>
        /// DataRowView是DataTable中的一行数据，而DataTable代表整张表
        /// </summary>
        private ObservableCollection<DataRowView> _originalexcelData;

        public ObservableCollection<DataRowView> OriginalExcelData
        {
            get { return _originalexcelData; }
            set
            {
                _originalexcelData = value;
                RaisePropertyChanged();
            }
        }
        #endregion

        #region 处理后数据
        private ObservableCollection<Certificate> _processedExcelData;

        public ObservableCollection<Certificate> ProcessedExcelData
        {
            get { return _processedExcelData; }
            set
            {
                _processedExcelData = value;
                RaisePropertyChanged();
            }
        }

        #endregion

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

    }
}

