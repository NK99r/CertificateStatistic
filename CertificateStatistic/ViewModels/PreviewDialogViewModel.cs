using Prism.Commands;
using Prism.Mvvm;
using Prism.Services.Dialogs;
using System;

namespace CertificateStatisticWPF.ViewModels
{
    class PreviewDialogViewModel : BindableBase, IDialogAware
    {
        public PreviewDialogViewModel() 
        {
            CloseCommand = new DelegateCommand(CloseDialog);
        }

        /// <summary>
        /// 预览提示
        /// </summary>
        private string _tip;
        public string Tip
        {
            get { return _tip; }
            set
            {
                _tip = value;
                RaisePropertyChanged();
            }
        }

        /// <summary>
        /// 预览图
        /// </summary>
        private string _imagepath;
        public string ImagePath
        {
            get { return _imagepath; }
            set
            {
                _imagepath = value;
                RaisePropertyChanged();
            }
        }

        #region IDialogAware实现
        public string Title { get; set; } = "规范预览";

        public event Action<IDialogResult> RequestClose;

        /// <summary>
        /// 决定窗体是否关闭
        /// </summary>
        /// <returns></returns>
        public bool CanCloseDialog()
        {
            return true;
        }

        /// <summary>
        /// 窗体关闭时触发
        /// </summary>
        public void OnDialogClosed()
        {
        }

        /// <summary>
        /// 窗体打开时触发，比窗体Loaded事件早触发
        /// </summary>
        /// <param name="parameters">传递来的参数为键值对，从中以键获值</param>
        public void OnDialogOpened(IDialogParameters parameters)
        {
            Tip = parameters.GetValue<string>("Tip");
            ImagePath = parameters.GetValue<string>("Preview");
        }
        #endregion

        public DelegateCommand CloseCommand { get; set; }

        private void CloseDialog()
        {
            //点击确认按钮后关闭对话框
            RequestClose?.Invoke(new DialogResult(ButtonResult.OK));
        }

    }
}
