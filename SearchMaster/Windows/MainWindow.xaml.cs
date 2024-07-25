using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.IO;
using SearchMaster.Engine;
using SearchMaster.Indexing;
using DocumentFormat.OpenXml.Spreadsheet;
using System.Windows.Data;
using System.ComponentModel;
using SearchMaster.Windows;
using System.Threading;
using System.Globalization;
using SearchMaster.Properties;

namespace SearchMaster
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private static SearchEngine defaultSearchEngine;
        private static Settings defaultSettings;

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

            textBlockCorporaSelectionStatus.Text = string.Empty;
            textBlockSearchStatus.Text = string.Empty;
            statusCorporaDirectory.DataContext = defaultSearchEngine;

            // foreach (Corpus corpus in defaultSettings.Corpora)
            //     listBoxCorpora.Items.Add(corpus);

            BindingOperations.SetBinding(comboBoxQuery, ComboBox.ItemsSourceProperty, new Binding("Queries") { Source = defaultSettings, Mode = BindingMode.TwoWay, UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged });

            comboBoxResolverType.ItemsSource = Enum.GetValues(typeof(SearchEngine.ResolverType)).Cast<SearchEngine.ResolverType>();
            comboBoxResolverType.SelectedItem = defaultSettings.ResolverType;

            checkBoxMultithread.IsChecked = defaultSettings.MultithreadingEnable;

            DataContext = defaultSettings;
        }

        private void buttonSearch_Click(object sender, RoutedEventArgs e)
        {
            if (listBoxCorpora.SelectedItems.Count <= 0)
            {
                new Popup() { Title = Properties.lang.Warning, Message = Properties.lang.NoCorpusSelected, Owner = this, Type = Popup.PopupType.Warning }.ShowDialog();
                return;
            }

            if (comboBoxQuery.Text.Length <= 0)
            {
                new Popup() { Title = Properties.lang.Warning, Message = Properties.lang.EmptyQuery, Owner = this, Type = Popup.PopupType.Warning }.ShowDialog();
                return;
            }

            List<string> serializedDocumentsPaths = new List<string>();

            foreach (Corpus corpus in listBoxCorpora.SelectedItems)
            {
                serializedDocumentsPaths.AddRange(Utils.ListDirectory(Path.Combine(new string[] { defaultSearchEngine.CorporaDirectory, corpus.Name }), false, null));
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
                case SearchEngine.ResolverType.OkapiBM25:
                    resolver = new OkapiBM25(serializedDocumentsPaths, defaultSettings.MultithreadingEnable);
                    break;
                case SearchEngine.ResolverType.TFIDF:
                    resolver = new TFIDFResolver(serializedDocumentsPaths, defaultSettings.MultithreadingEnable);
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
            }
            defaultSettings.Queries.Insert(0, query);

            if (comboBoxQuery.Items.Count > Settings.MAX_QUERY_HISTORY)
            {
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
            buttonRemoveCorpus.IsEnabled = false;
            CorpusWindow corpusWindow = new CorpusWindow() { Title = Properties.lang.CorpusCreationWindow, Owner = this };
            if (true == corpusWindow.ShowDialog())
            {
                Indexer indexer = new Indexer(Properties.Settings.Default.IndexerVersion, defaultSettings.MultithreadingEnable, MainWindow.SearchEngine.CorporaDirectory);
                statusProgressBar.Visibility = Visibility.Visible;
                statusSummaryText.Text = Properties.lang.Indexing + "...";
                await Task.Run(() =>
                {
                    indexer.ProcessCorpus(corpusWindow.Corpus);
                });
                statusProgressBar.Visibility = Visibility.Hidden;
                statusSummaryText.Text = string.Empty;

                // listBoxCorpora.Items.Add(corpusWindow.Corpus);
                defaultSettings.Corpora.Add(corpusWindow.Corpus);
                defaultSettings.Save();
            }
            buttonAddCorpus.IsEnabled = true;
            buttonRemoveCorpus.IsEnabled = true;
        }

        private void buttonRemoveCorpus_Click(object sender, RoutedEventArgs e)
        {
            if (listBoxCorpora.SelectedItems.Count <= 0)
                return;

            Popup popup = new Popup() { Title = Properties.lang.ConfirmDeletion, Message = Properties.lang.ConfirmationMessage, Owner = this, Type = Popup.PopupType.Warning };
            if (true == popup.ShowDialog())
            {
                for (int i = listBoxCorpora.SelectedItems.Count - 1; i >= 0; i--)
                {
                    Corpus corpus = (Corpus) listBoxCorpora.SelectedItems[i];
                    defaultSettings.Corpora.Remove(corpus);
                    try
                    {
                        Directory.Delete(Path.Combine(new string[] { defaultSearchEngine.CorporaDirectory, corpus.Name }), true);
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
            Button cmd = (Button)sender;
            if (cmd.DataContext is Corpus)
            {
                Corpus selectedCorpus = (Corpus)cmd.DataContext;
                CorpusWindow corpusWindow = new CorpusWindow(selectedCorpus) { Title = Properties.lang.CorpusCreationWindow, Owner = this };
                if (true == corpusWindow.ShowDialog())
                {
                    selectedCorpus.Name = corpusWindow.Corpus.Name;
                    selectedCorpus.Location = corpusWindow.Corpus.Location;
                    selectedCorpus.DocumentCount = corpusWindow.Corpus.DocumentCount;
                    selectedCorpus.Whitelist = corpusWindow.Corpus.Whitelist;
                    selectedCorpus.Blacklist = corpusWindow.Corpus.Blacklist;
                    selectedCorpus.CrawlUrlEnabled = corpusWindow.Corpus.CrawlUrlEnabled;
                    // TODO: Rescan directory and recreate indexed folder.
                }
            }
        }

        private void listBoxCorpora_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            textBlockCorporaSelectionStatus.Text = listBoxCorpora.SelectedItems.Count + " " + Properties.lang.Selected + ".";
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
                    try
                    {
                        System.Diagnostics.Process.Start(sr.Document.DocumentSource.Path);
                        sr.Opened = true;
                    }
                    catch (Win32Exception ex)
                    {
                        Console.WriteLine(ex.ToString());
                    }
                }
            }
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
        
        private void checkBoxUseAcronym_CheckedChanged(object sender, RoutedEventArgs e)
        {
            if (sender is CheckBox)
            {
                defaultSettings.UseAcronymEnable = ((CheckBox)sender).IsChecked == true;
                defaultSettings.Save();
            }
        }

        private void comboBoxQuery_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (((ComboBox)sender).SelectedItem != null)
                comboBoxResolverType.SelectedItem = ((SearchMaster.Engine.Query)((ComboBox)sender).SelectedItem).Type;
        }

        private void Window_Closed(object sender, EventArgs e)
        {
            defaultSettings.Save();
        }

        private void MenuItemManual_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                CultureInfo currentCulture = Thread.CurrentThread.CurrentCulture;
                string targetedGuidePath = Path.Combine(Environment.CurrentDirectory, "help_" + currentCulture.TwoLetterISOLanguageName.ToLower() + ".html");
                if (!File.Exists(targetedGuidePath))
                {
                    targetedGuidePath = Path.Combine(Environment.CurrentDirectory, "help_en.html");
                }
                System.Diagnostics.Process.Start(targetedGuidePath);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }
        }

        private void MenuItemAbout_Click(object sender, RoutedEventArgs e)
        {
            new Popup() { Message = Properties.lang.AboutMessage + Environment.NewLine + Properties.lang.Version + " " + Properties.Settings.Default.IndexerVersion, Owner = this }.ShowDialog();
        }

        private void buttonOpenAppSettings_Click(object sender, RoutedEventArgs e)
        {
            AppSettingsWindow appSettingsWindow = new AppSettingsWindow() { Title = Properties.lang.ApplicationWindow, Owner = this, DataContext = defaultSearchEngine.Duplicate() };
            if (true == appSettingsWindow.ShowDialog())
            {
                defaultSearchEngine = (SearchEngine) appSettingsWindow.DataContext;
                defaultSearchEngine.Save();
                defaultSearchEngine.LoadAcronyms();
            }
        }
    }
}
