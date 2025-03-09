using CertificateStatistic.Models;
using Prism.Commands;
using Prism.Mvvm;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Threading;

namespace CertificateStatistic.ViewModels
{
    internal class HomeUCViewModel:BindableBase
    {
        #region 轮播图

        public DelegateCommand NextCommand { get; }

        public DelegateCommand PreviousCommand { get; }

        public DelegateCommand<string> OpenLinkCommand { get; }

        #region 当前图片索引
        private int _currentIndex;

        public int CurrentIndex
        {
            get { return _currentIndex; }
            set
            {
                if (_currentIndex != value)
                {
                    _currentIndex = value;
                    RaisePropertyChanged();
                    CurrentSlide = SlideList.Count > 0 ? SlideList[_currentIndex] : null;
                }
            }
        }
        #endregion

        #region 当前图片
        private Slide _currentSlide;

        public Slide CurrentSlide
        {
            get { return _currentSlide; }
            set
            {
                _currentSlide = value;
                RaisePropertyChanged();
            }
        }
        #endregion

        #region 轮播图集合
        private ObservableCollection<Slide> _slideList;

        public ObservableCollection<Slide> SlideList
        {
            get { return _slideList; }
            set
            {
                _slideList = value;
                RaisePropertyChanged();
            }
        }
        #endregion

        #region 轮播方法
        /// <summary>
        /// 添加导航项
        /// </summary>
        private void CreateSlideList()
        {
            SlideList.Add(new Slide { ImagePath = "/StaticResource/pic/image1.jpg", Title = "Slide 1", Url = "https://www.baidu.com" });
            SlideList.Add(new Slide { ImagePath = "/StaticResource/pic/image2.jpg", Title = "Slide 2", Url = "https://www.baidu.com" });
            SlideList.Add(new Slide { ImagePath = "/StaticResource/pic/image3.jpg", Title = "Slide 3", Url = "https://www.baidu.com" });
            CurrentIndex = 0;
        }

        /// <summary>
        /// 下一张
        /// </summary>
        private void OnNext()
        {
            CurrentIndex = (CurrentIndex + 1) % SlideList.Count;
            //求余确保防止CurrentIndex不会超出数组的索引范围
            //如当前集合有5张图片，最后一张图片索引为4，+1为5会超出范围，5求余5张图片为0，回到第一张图片
        }

        /// <summary>
        /// 上一张
        /// </summary>
        private void OnPrevious()
        {
            CurrentIndex = (CurrentIndex - 1 + SlideList.Count) % SlideList.Count;
        }
        #endregion

        #endregion

        public HomeUCViewModel()
        {
            //超链接
            OpenLinkCommand = new DelegateCommand<string>(OpenLink);

            #region 轮播图初始化
            SlideList = new ObservableCollection<Slide>();

            CreateSlideList();

            NextCommand = new DelegateCommand(OnNext);

            PreviousCommand = new DelegateCommand(OnPrevious);
            #endregion

            #region 快速通道初始化
            LinkList = new ObservableCollection<Link>();

            CreateHSTCLinkList();
            #endregion
        }

        #region 快速通道

        #region 通道项
        private Link _Link;

        public Link Link
        {
            get { return _Link; }
            set
            {
                _Link = value;
                RaisePropertyChanged();
            }
        }
        #endregion

        #region 通道集合
        private ObservableCollection<Link> _LinkList;

        public ObservableCollection<Link> LinkList
        {
            get { return _LinkList; }
            set
            {
                _LinkList = value;
                RaisePropertyChanged();
            }
        }

        #endregion

        private void CreateHSTCLinkList()
        {
            LinkList.Add(new Link { Icon = "/StaticResource/Link/jwgl.png", Title = "教务管理", Url = "https://www.hstc.edu.cn/index/jxgl1.htm" });
            LinkList.Add(new Link { Icon = "/StaticResource/Link/zhhy.png", Title = "智慧韩园", Url = "https://hscas.hstc.edu.cn/cas" });
            LinkList.Add(new Link { Icon = "/StaticResource/Link/tsg.png", Title = "图书馆", Url = "https://newlib.hstc.edu.cn/" });
            LinkList.Add(new Link { Icon = "/StaticResource/Link/xxt.png", Title = "学习通", Url = "https://passport2.chaoxing.com/" });
            LinkList.Add(new Link { Icon = "/StaticResource/Link/wlfw.png", Title = "网络服务", Url = "https://www.hstc.edu.cn/xyfw/wlfw.htm" });
            LinkList.Add(new Link { Icon = "/StaticResource/Link/zw.png", Title = "知网", Url = "https://www.cnki.net/" });
            LinkList.Add(new Link { Icon = "/StaticResource/Link/lanqiao.png", Title = "蓝桥杯全国大学生TMT行业赛事", Url = "https://dasai.lanqiao.cn/" });
            LinkList.Add(new Link { Icon = "/StaticResource/Link/blcu.png", Title = "中国大学生计算机设计大赛", Url = "https://jsjds.blcu.edu.cn/index.htm" });
            LinkList.Add(new Link { Icon = "/StaticResource/Link/bxg.png", Title = "传智杯全国IT技能大赛", Url = "https://www.boxuegu.com/match/" });
            LinkList.Add(new Link { Icon = "/StaticResource/Link/hwb.png", Title = "全国大学生物联网设计大赛(华为杯)", Url = "https://iot.sjtu.edu.cn/Default.aspx" });
            LinkList.Add(new Link { Icon = "/StaticResource/Link/ncccu.png", Title = "全国高校计算机能力挑战赛", Url = "http://www.ncccu.org.cn/" });
        }
        
        
        #endregion
        /// <summary>
        /// 超链接
        /// </summary>
        /// <param name="url">链接</param>
        private void OpenLink(string url)
        {
            if (!string.IsNullOrEmpty(url))
            {
                Process.Start(new ProcessStartInfo(url) { UseShellExecute = true });
            }
        }
    }
}
