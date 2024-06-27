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
using Microsoft.Win32;
using Microsoft.WindowsAPICodePack.Dialogs;
using MasterIndexer;

namespace SearchMaster
{
    /// <summary>
    /// Interaction logic for CorpusWindow.xaml
    /// </summary>
    public partial class CorpusWindow : Window
    {
        private Corpus corpus;

        public CorpusWindow(Corpus corpus = null)
        {
            InitializeComponent();

            if (corpus == null)
            {
                this.corpus = new Corpus();
            }
            else
            {
                this.corpus = corpus.Duplicate();
            }

            this.DataContext = this.corpus;
        }

        public Corpus Corpus
        {
            get
            {
                return corpus;
            }
        }

        private void buttonOk_Click(object sender, RoutedEventArgs e)
        {
            if (textBoxCorpusName.Text.Trim().Length <= 0)
            {
                new Popup() { Title = "Warning", Message = "No corpus name.", Owner = this }.ShowDialog();
                return;
            }

            if (!System.IO.Directory.Exists(textBoxCorpusPath.Text) || textBoxCorpusPath.Text.Trim().Length <= 0)
            {
                new Popup() { Title = "Warning", Message = "Empty or invalid directory path.", Owner = this }.ShowDialog();
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

        private void buttonOpenFolder_Click(object sender, RoutedEventArgs e)
        {
            CommonOpenFileDialog dialog = new CommonOpenFileDialog();
            dialog.IsFolderPicker = true;
            if (dialog.ShowDialog() == CommonFileDialogResult.Ok)
            {
                textBoxCorpusPath.Text = dialog.FileName;
            }
        }
    }
}
