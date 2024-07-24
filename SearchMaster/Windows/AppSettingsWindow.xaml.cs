using Microsoft.WindowsAPICodePack.Dialogs;
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

namespace SearchMaster.Windows
{
    /// <summary>
    /// Logique d'interaction pour AppSettings.xaml
    /// </summary>
    public partial class AppSettingsWindow : Window
    {
        public AppSettingsWindow()
        {
            InitializeComponent();
        }

        private void buttonOpenFolder_Click(object sender, RoutedEventArgs e)
        {
            CommonOpenFileDialog dialog = new CommonOpenFileDialog();
            dialog.IsFolderPicker = true;
            if (dialog.ShowDialog() == CommonFileDialogResult.Ok)
            {
                textBoxCorporaPath.Text = dialog.FileName;
            }
        }

        private void buttonOpenAcronymFile_Click(object sender, RoutedEventArgs e)
        {
            CommonOpenFileDialog dialog = new CommonOpenFileDialog();
            dialog.IsFolderPicker = false;
            if (dialog.ShowDialog() == CommonFileDialogResult.Ok)
            {
                textBoxAcronymPath.Text = dialog.FileName;
            }
        }

        private void buttonOk_Click(object sender, RoutedEventArgs e)
        {
            if (!System.IO.Directory.Exists(textBoxCorporaPath.Text))
            {
                new Popup() { Title = Properties.lang.Warning, Message = Properties.lang.InvalidPath + Environment.NewLine + textBoxCorporaPath.Text, Owner = this, Type = Popup.PopupType.Error }.ShowDialog();
                return;
            }
            DialogResult = true;
            Close();
        }

        private void buttonCancel_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }
    }
}
