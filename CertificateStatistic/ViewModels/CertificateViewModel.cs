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

namespace CertificateStatistic.ViewModels
{
    internal class CertificateViewModel : BindableBase
    {
        public CertificateViewModel()
        {
            #region Excel操作初始化
            
            LoadExcelCommand = new DelegateCommand(LoadExcelData);
            #endregion

        }

        #region 读写Excel操作

        public DelegateCommand LoadExcelCommand { get; set; }

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

        private void LoadExcelData()
        {
            var openFileDialog = new OpenFileDialog { Filter = "Excel Files|*.xls;*.xlsx" };
            if (openFileDialog.ShowDialog() == true)
            {
                // 直接调用整合后的方法
                ProcessedExcelData = ExcelTool.ReadAndProcessExcel(openFileDialog.FileName);
            }
        }
        #endregion

    }
}

