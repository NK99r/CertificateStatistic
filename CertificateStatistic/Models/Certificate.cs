using Prism.Mvvm;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CertificateStatisticWPF.Models
{
    internal class Certificate:BindableBase
    {
        public string StudentID { get; set; }

        public string Name { get; set; }

        public string CertificateProject { get; set; }

        public string Organizer { get; set; }

        public string EventLevel { get; set; }

        public string Category { get; set; }

        public string Date { get; set; }

        public string ProID { get; set; }

        public string Year
        {
            get => Date.Substring(0,4); 
        }

        private bool _isHighlight;
        public bool IsHighlighted
        {
            get { return _isHighlight; }
            set
            {
                _isHighlight = value;
                RaisePropertyChanged();
            }
        }

    }
}
