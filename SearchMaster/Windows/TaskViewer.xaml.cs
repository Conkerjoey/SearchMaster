using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.ComponentModel;

namespace SearchMaster
{
    /// <summary>
    /// Interaction logic for TaskViewer.xaml
    /// </summary>
    public partial class TaskViewer : Window, INotifyPropertyChanged
    {
        private double progress = 0.0D;
        private string summary = "Task Summary Placeholder";

        public event PropertyChangedEventHandler PropertyChanged;

        public TaskViewer()
        {
            InitializeComponent();

            DataContext = this;
        }

        public double Progress
        {
            set
            {
                if (value != this.progress)
                {
                    this.progress = value;
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("Progress"));
                }
            }
            get
            {
                return progress;
            }
        }

        public string Summary
        {
            set
            {
                if (value != this.summary)
                {
                    this.summary = value;
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("Summary"));
                }
            }
            get { return summary; }
        }
    }
}
