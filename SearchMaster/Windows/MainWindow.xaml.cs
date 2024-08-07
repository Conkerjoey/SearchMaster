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
using System.Text.Json;
using Microsoft.WindowsAPICodePack.Dialogs;
using System.Reflection.Emit;
using MS.WindowsAPICodePack.Internal;

namespace SearchMaster
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private static Settings defaultSettings;

        static MainWindow()
        {
            defaultSettings = Settings.Load();
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
            textBoxSearchInFilesFolder.Text = string.Empty;

            statusCorporaDirectory.DataContext = defaultSettings;

            // foreach (Corpus corpus in defaultSettings.Corpora)
            //     listBoxCorpora.Items.Add(corpus);

            BindingOperations.SetBinding(comboBoxQuery, ComboBox.ItemsSourceProperty, new Binding("Queries") { Source = defaultSettings, Mode = BindingMode.TwoWay, UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged });

            comboBoxResolverType.ItemsSource = Enum.GetValues(typeof(Settings.EResolverType)).Cast<Settings.EResolverType>();
            comboBoxResolverType.SelectedItem = defaultSettings.ResolverType;
            // TODO: Add binding for resolver type.

            DataContext = defaultSettings;
        }

        private void buttonSearch_Click(object sender, RoutedEventArgs e)
        {
            if (comboBoxQuery.Text.Length <= 0)
            {
                new Popup() { Title = Properties.lang.Warning, Message = Properties.lang.EmptyQuery, Owner = this, Type = Popup.PopupType.Warning }.ShowDialog();
                return;
            }
            QueryResult queryResult = null;
            SearchMaster.Engine.Query query = null;

            if (defaultSettings.NonIndexedSearch)
            {
                string folder = textBoxSearchInFilesFolder.Text;
                string queryStr = comboBoxQuery.Text;
                query = new Engine.Query(queryStr, Settings.EResolverType.LabelDensity);

                if (!Directory.Exists(folder))
                {
                    new Popup() { Title = Properties.lang.Warning, Message = Properties.lang.InvalidPath, Owner = this, Type = Popup.PopupType.Warning }.ShowDialog();
                }

                queryResult = NonIndexedEngine.SearchInFolder(folder, query);
            }
            else
            {
                if (listBoxCorpora.SelectedItems.Count <= 0)
                {
                    new Popup() { Title = Properties.lang.Warning, Message = Properties.lang.NoCorpusSelected, Owner = this, Type = Popup.PopupType.Warning }.ShowDialog();
                    return;
                }

                List<string> serializedDocumentsPaths = new List<string>();

                foreach (Corpus corpus in listBoxCorpora.SelectedItems)
                {
                    // serializedDocumentsPaths.AddRange(Utils.ListDirectory(Path.Combine(new string[] { defaultSettings.CacheManager.CacheDirectory, corpus.Name }), false, null, null));
                    serializedDocumentsPaths.AddRange(Utils.ListDirectory(Path.Combine(new string[] { defaultSettings.CorporaDirectory, corpus.Name }), false, null, null));
                }

                IResolver resolver = null;
                switch (defaultSettings.ResolverType)
                {
                    case Settings.EResolverType.FullMatch:
                        resolver = new FullMatchResolver(serializedDocumentsPaths, defaultSettings.MultithreadingEnable);
                        break;
                    case Settings.EResolverType.LabelDensity:
                        resolver = new LabelDensityResolver(serializedDocumentsPaths, defaultSettings.MultithreadingEnable);
                        break;
                    case Settings.EResolverType.Regex:
                        resolver = new RegexResolver(serializedDocumentsPaths, defaultSettings.MultithreadingEnable);
                        break;
                    case Settings.EResolverType.OkapiBM25:
                        resolver = new OkapiBM25(serializedDocumentsPaths, defaultSettings.MultithreadingEnable);
                        break;
                    case Settings.EResolverType.TFIDF:
                        resolver = new TFIDFResolver(serializedDocumentsPaths, defaultSettings.MultithreadingEnable);
                        break;
                    case Settings.EResolverType.CosineSimilarity:
                    default:
                        resolver = new CosineSimilarityResolver(serializedDocumentsPaths, defaultSettings.MultithreadingEnable);
                        break;
                }

                query = new SearchMaster.Engine.Query(comboBoxQuery.Text, defaultSettings.ResolverType);

                queryResult = resolver.SearchQuery(query);
            }


            if (defaultSettings.Queries.Contains(query))
            {
                defaultSettings.Queries.Remove(query);
            }
            defaultSettings.Queries.Insert(0, query);

            if (comboBoxQuery.Items.Count > Settings.MAX_QUERY_HISTORY)
            {
                defaultSettings.Queries.RemoveAt(defaultSettings.Queries.Count - 1);
            }

            textBlockSearchStatus.Text = queryResult.GetQueryResultStatus();

            listBoxSearchResults.Items.Clear();
            foreach (SearchResult result in queryResult.searchResults)
            {
                listBoxSearchResults.Items.Add(result);
            }
        }

        private async void buttonAddCorpus_Click(object sender, RoutedEventArgs e)
        {
            CorpusWindow corpusWindow = new CorpusWindow() { Title = Properties.lang.CorpusCreationWindow, Owner = this };
            if (true == corpusWindow.ShowDialog())
            {
                Indexer indexer = new Indexer(Properties.Settings.Default.IndexerVersion, defaultSettings.MultithreadingEnable, MainWindow.DefaultSettings.CorporaDirectory);
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
                defaultSettings.CacheManager.Sync();
            }
        }

        private void buttonRemoveCorpus_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button)
            {
                if (((Button) sender).DataContext is Corpus)
                {
                    Popup popup = new Popup() { Title = Properties.lang.ConfirmDeletion, Message = Properties.lang.ConfirmationMessage, Owner = this, Type = Popup.PopupType.Warning };
                    if (true == popup.ShowDialog())
                    {
                        Corpus corpus = (Corpus)((Button)sender).DataContext;
                        defaultSettings.Corpora.Remove(corpus);
                        try
                        {
                            Directory.Delete(Path.Combine(new string[] { defaultSettings.CorporaDirectory, corpus.Name }), true);
                        }
                        catch (Exception exception)
                        {
                            Console.WriteLine("Cannot remove corpus from disk at the installation path. " + exception.Message + Environment.NewLine + exception.StackTrace);
                        }
                        defaultSettings.Save();
                    }
                }
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
                defaultSettings.ResolverType = (Settings.EResolverType)((ComboBox)sender).SelectedIndex;
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
            SaveSettings();
        }

        private void SaveSettings()
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
            AppSettingsWindow appSettingsWindow = new AppSettingsWindow() { Title = Properties.lang.ApplicationWindow, Owner = this, DataContext = defaultSettings.Duplicate() };
            if (true == appSettingsWindow.ShowDialog())
            {
                defaultSettings = (Settings) appSettingsWindow.DataContext;
                try
                {
                    defaultSettings.LoadAcronyms();
                }
                catch (Exception)
                {
                    defaultSettings.AcronymFilepath = null;
                    new Popup() { Title = Properties.lang.Warning, Message = Properties.lang.JsonError, Owner = this, Type = Popup.PopupType.Error }.ShowDialog();
                }
                defaultSettings.Save();
            }
        }

        private void buttonSearchFolder_Click(object sender, RoutedEventArgs e)
        {
            CommonOpenFileDialog dialog = new CommonOpenFileDialog();
            dialog.IsFolderPicker = true;
            if (dialog.ShowDialog() == CommonFileDialogResult.Ok)
            {
                textBoxSearchInFilesFolder.Text = dialog.FileName;
            }
        }
    }
}
