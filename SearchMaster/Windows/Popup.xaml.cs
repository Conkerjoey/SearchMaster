using Org.BouncyCastle.Cms;
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

namespace SearchMaster
{
    /// <summary>
    /// Interaction logic for Popup.xaml
    /// </summary>
    public partial class Popup : Window
    {
        private string message;
        public enum PopupType
        {
            Info,
            Warning,
            Error
        }

        public Popup()
        {
            InitializeComponent();

            DataContext = this;
        }

        public PopupType Type
        {
            get; set;
        }

        public string Message
        {
            get { return message; }
            set { message = value; }
        }

        private void buttonOK_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = true;
            Close();
        }

        private void buttonCancel_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }

        public string ImageIcon
        {
            get
            {
                switch (Type)
                {
                    case PopupType.Warning:
                        return "../Resources/icon_warning.png";
                    case PopupType.Error:
                        return "../Resources/icon_error.png";
                    case PopupType.Info:
                    default:
                        return "../Resources/icon_info.png";
                }
            }
        }
    }
}
