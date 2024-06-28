using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.IO;
using SearchMaster.Engine;
using DocLib;
using MasterIndexer;
using DocumentFormat.OpenXml.Spreadsheet;
using SearchMaster.Windows;

namespace SearchMaster
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private static SearchEngine defaultSearchEngine;
        private static Settings defaultSettings;
        private OnScreenPreview currentPreview = null;

        static MainWindow()
        {
            defaultSearchEngine = SearchEngine.Load();
            defaultSettings = Settings.Load();
        }

        public static SearchEngine SearchEngine
        {
            get { return defaultSearchEngine; }
        }

        public static Settings DefaultSettings
        {
            get { return defaultSettings; }
        }

        public MainWindow()
        {
            InitializeComponent();

            statusProgressBar.Value = 0;
            statusProgressBar.Visibility = Visibility.Hidden;
            statusSummaryText.Text = string.Empty;
            statusCorporaDirectory.Text = "Corpora location: " + Path.Combine(Environment.CurrentDirectory, defaultSearchEngine.CorporaDirectory);

            textBlockCorporaSelectionStatus.Text = string.Empty;
            textBlockSearchStatus.Text = string.Empty;

            foreach (Corpus corpus in defaultSettings.Corpora)
                listBoxCorpora.Items.Add(corpus);

            foreach (SearchMaster.Engine.Query query in defaultSettings.Queries)
                comboBoxQuery.Items.Add(query);

            comboBoxResolverType.ItemsSource = Enum.GetValues(typeof(SearchEngine.ResolverType)).Cast<SearchEngine.ResolverType>();
            comboBoxResolverType.SelectedItem = defaultSettings.ResolverType;

            checkBoxMultithread.IsChecked = defaultSettings.MultithreadingEnable;
        }

        private void buttonSearch_Click(object sender, RoutedEventArgs e)
        {
            if (listBoxCorpora.SelectedItems.Count <= 0)
            {
                new Popup() { Title = "Warning", Message = "No corpus selected !", Owner = this }.ShowDialog();
                return;
            }

            if (comboBoxQuery.Text.Length <= 0)
            {
                new Popup() { Title = "Warning", Message = "Query is empty.", Owner = this }.ShowDialog();
                return;
            }

            List<string> serializedDocumentsPaths = new List<string>();

            foreach (Corpus corpus in listBoxCorpora.SelectedItems)
            {
                serializedDocumentsPaths.AddRange(Files.GetAllFiles(Path.Combine(new string[] { defaultSearchEngine.CorporaDirectory, corpus.Name }), false, null));
            }

            IResolver resolver = null;
            switch (defaultSettings.ResolverType)
            {
                case SearchEngine.ResolverType.FullMatch:
                    resolver = new FullMatchResolver(serializedDocumentsPaths, defaultSettings.MultithreadingEnable);
                    break;
                case SearchEngine.ResolverType.LabelDensity:
                    resolver = new LabelDensityResolver(serializedDocumentsPaths, defaultSettings.MultithreadingEnable);
                    break;
                case SearchEngine.ResolverType.Regex:
                    resolver = new RegexResolver(serializedDocumentsPaths, defaultSettings.MultithreadingEnable);
                    break;
                case SearchEngine.ResolverType.CosineSimilarity:
                default:
                    resolver = new CosineSimilarityResolver(serializedDocumentsPaths, defaultSettings.MultithreadingEnable);
                    break;
            }

            SearchMaster.Engine.Query query = new SearchMaster.Engine.Query(comboBoxQuery.Text, defaultSettings.ResolverType);
            if (defaultSettings.Queries.Contains(query))
            {
                defaultSettings.Queries.Remove(query);
                comboBoxQuery.Items.Remove(query);
            }
            defaultSettings.Queries.Insert(0, query);
            comboBoxQuery.Items.Insert(0, query);

            if (comboBoxQuery.Items.Count > Settings.MAX_QUERY_HISTORY)
            {
                comboBoxQuery.Items.RemoveAt(comboBoxQuery.Items.Count - 1);
                defaultSettings.Queries.RemoveAt(defaultSettings.Queries.Count - 1);
            }

            QueryResult queryResult = resolver.SearchQuery(query);

            textBlockSearchStatus.Text = queryResult.GetQueryResultStatus();

            listBoxSearchResults.Items.Clear();
            foreach (SearchResult result in queryResult.searchResults)
            {
                listBoxSearchResults.Items.Add(result);
            }
        }

        private async void buttonAddCorpus_Click(object sender, RoutedEventArgs e)
        {
            buttonAddCorpus.IsEnabled = false;
            CorpusWindow corpusWindow = new CorpusWindow() { Title = "Corpus Creation Window", Owner = this };
            if (true == corpusWindow.ShowDialog())
            {
                corpusWindow.Corpus.ListDocumentsAtLocation();
                Indexer indexer = new Indexer("1.0.0", defaultSettings.MultithreadingEnable, MainWindow.SearchEngine.CorporaDirectory);
                //TaskViewer taskViewer = new TaskViewer() { Title = "Task running", Summary = "Loading & analyzing documents... This may takes a while.", Owner = this.Owner };
                //taskViewer.Show();

                Progress<double> progressRead = new Progress<double>(value => {
                    if (value == -1)
                    {
                        //taskViewer.Progress = 0;
                        statusProgressBar.Value = 0;
                        return;
                    }
                    //taskViewer.Progress += value * 100.0D / corpusWindow.Corpus.DocumentCount;
                    //taskViewer.Summary = "Loading & analyzing documents... (" + (int)Math.Ceiling(taskViewer.Progress / 100 * corpusWindow.Corpus.DocumentCount) + "/" + corpusWindow.Corpus.DocumentCount + ")";

                    statusProgressBar.Value += value * 100.0D / corpusWindow.Corpus.DocumentCount;
                    if (statusProgressBar.Visibility != Visibility.Visible)
                        statusProgressBar.Visibility = Visibility.Visible;
                    statusSummaryText.Text = "Loading & analyzing documents... (" + (int)Math.Ceiling(statusProgressBar.Value / 100 * corpusWindow.Corpus.DocumentCount) + "/" + corpusWindow.Corpus.DocumentCount + ")";
                });
                Progress<double> progressCompute = new Progress<double>(value => {
                    //taskViewer.Progress += value * 100.0D / corpusWindow.Corpus.DocumentCount;
                    //taskViewer.Summary = "Compute weighted labels... (" + (int)Math.Ceiling(taskViewer.Progress / 100 * corpusWindow.Corpus.DocumentCount) + "/" + corpusWindow.Corpus.DocumentCount + ")";

                    statusProgressBar.Value += value * 100.0D / corpusWindow.Corpus.DocumentCount;
                    if (statusProgressBar.Visibility != Visibility.Visible)
                        statusProgressBar.Visibility = Visibility.Visible;
                    statusSummaryText.Text = "Compute weighted labels... (" + (int)Math.Ceiling(statusProgressBar.Value / 100 * corpusWindow.Corpus.DocumentCount) + "/" + corpusWindow.Corpus.DocumentCount + ")";
                });
                await Task.Run(() =>
                {
                    indexer.ProcessCorpus(corpusWindow.Corpus, progressRead, progressCompute);
                });
                statusProgressBar.Visibility = Visibility.Hidden;
                statusSummaryText.Text = string.Empty;

                //taskViewer.Close();
                listBoxCorpora.Items.Add(corpusWindow.Corpus);
                defaultSettings.Corpora.Add(corpusWindow.Corpus);
                defaultSettings.Save();
            }
            buttonAddCorpus.IsEnabled = true;
        }

        private void buttonRemoveCorpus_Click(object sender, RoutedEventArgs e)
        {
            Popup popup = new Popup() { Title = "Confirm deletion", Message = "Are you sure you want to delete the selected corpus ?", Owner = this };
            if (true == popup.ShowDialog())
            {
                Corpus[] corpora = new Corpus[listBoxCorpora.SelectedItems.Count];
                listBoxCorpora.SelectedItems.CopyTo(corpora, 0);
                foreach (Corpus corpus in corpora)
                {
                    defaultSettings.Corpora.Remove(corpus);
                    listBoxCorpora.Items.Remove(corpus);
                    try
                    {
                        Directory.Delete(System.IO.Path.Combine(new string[] { defaultSearchEngine.CorporaDirectory, corpus.Name }), true);
                    }
                    catch (Exception exception)
                    {
                        Console.WriteLine("Cannot remove corpus from disk at the installation path. " + exception.Message + Environment.NewLine + exception.StackTrace);
                    }
                }
                defaultSettings.Save();
            }
        }

        private void buttonCorpusSettings_Click(object sender, RoutedEventArgs e)
        {
            Button cmd = (Button) sender;
            if (cmd.DataContext is Corpus)
            {
                Corpus selectedCorpus = (Corpus) cmd.DataContext;
                CorpusWindow corpusWindow = new CorpusWindow(selectedCorpus) { Title = "Corpus Creation Window", Owner = this };
                if (true == corpusWindow.ShowDialog())
                {
                    selectedCorpus.Name = corpusWindow.Corpus.Name;
                    selectedCorpus.Location = corpusWindow.Corpus.Location;
                    selectedCorpus.DocumentsPath = corpusWindow.Corpus.DocumentsPath;
                    selectedCorpus.Whitelist = corpusWindow.Corpus.Whitelist;
                    selectedCorpus.Blacklist = corpusWindow.Corpus.Blacklist;
                    // TODO: Rescan directory
                }
            }
        }

        private void listBoxCorpora_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            
            textBlockCorporaSelectionStatus.Text = listBoxCorpora.SelectedItems.Count + " selected.";
        }

        private void listBoxSearchResults_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {

        }

        private void listBoxSearchResults_DoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (sender is ListBoxItem)
            {
                if (((ListBoxItem)sender).Content is SearchResult)
                {
                    SearchResult sr = (SearchResult)((ListBoxItem)sender).Content;
                    System.Diagnostics.Process.Start(sr.Document.DocumentPath.Path);
                }
            }
        }

        private void listBoxSearchResults_MouseEnter(object sender, MouseEventArgs e)
        {
            // currentPreview = new OnScreenPreview();
            // currentPreview.ShowDialog();
        }

        private void listBoxSearchResults_MouseLeave(object sender, MouseEventArgs e)
        {
            // currentPreview.Hide();
            // currentPreview = null;
        }

        private void listBoxSearchResults_MouseMove(object sender, MouseEventArgs e)
        {
            var mousePosition = e.GetPosition(this);
        }

        private void comboBoxResolverType_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (sender is ComboBox)
            {
                defaultSettings.ResolverType = (SearchEngine.ResolverType)((ComboBox)sender).SelectedIndex;
                defaultSettings.Save();
            }
        }

        private void checkBoxMultithread_CheckedChanged(object sender, RoutedEventArgs e)
        {
            if (sender is CheckBox)
            {
                defaultSettings.MultithreadingEnable = ((CheckBox)sender).IsChecked == true;
                defaultSettings.Save();
            }
        }

        private void comboBoxQuery_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (((ComboBox)sender).SelectedItem != null)
                comboBoxResolverType.SelectedItem = ((SearchMaster.Engine.Query)((ComboBox)sender).SelectedItem).Type;
        }
    }
}
