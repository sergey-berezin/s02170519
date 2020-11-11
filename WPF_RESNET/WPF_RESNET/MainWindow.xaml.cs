using Microsoft.Win32;
using MyLibrary;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Forms;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Collections.ObjectModel;
using Microsoft.EntityFrameworkCore;
using System.IO;

namespace WPF_RESNET
{    
    public partial class MainWindow : Window, INotifyPropertyChanged
    {
        LibraryContext libraryContext;
	    ResNet resNet;
        CancellationTokenSource cts;
        ObservableCollection<FileView> Results { get; set; }
        public IEnumerable<FileView> SelectedClass {  get
            {
                foreach (var i in Results)
                    if (selected == null || i.TypeName == (selected as string).Split(';')[0])
                            yield return i;
            } }
        object selected;

        Dictionary<string, int> dictionary = new Dictionary<string, int>();
        public IEnumerable<string> Classes { get {
                foreach (var i in dictionary)
                    yield return i.Key + "; Count " + i.Value;
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        public MainWindow()
        {
            InitializeComponent();
            libraryContext = new LibraryContext();
            //libraryContext.Clear();
            resNet = new ResNet();
            resNet.OnProcessedImage += () => Dispatcher.Invoke(ProcessedImageHandler);
            Results = new ObservableCollection<FileView>();
            grid.DataContext = this;
            lb.SelectionChanged += (s, e) =>
            {
                selected = lb.SelectedItem;
                OnPropertyChanged("SelectedClass");
            };
        }
        void ProcessedImageHandler()
        {
            var result = resNet.GetResult();
            if (result != null)
            {
                var r = libraryContext.AddResNetResult(result);
                Results.Add(r);
                AddImageToUI(r);
            }
        }
        void AddImageToUI(FileView result)
        {
            if (dictionary.ContainsKey(result.TypeName))
            {
                int value;
                dictionary.TryGetValue(result.TypeName, out value);
                value++;
                dictionary.Remove(result.TypeName);
                dictionary.Add(result.TypeName, value);
            }
            else
                dictionary.Add(result.TypeName, 1);

            OnPropertyChanged("Classes");
            if (selected != null && (selected as string).Split(';')[0] == result.TypeName)
                OnPropertyChanged("SelectedClass");
        }
	    private async void Button_Click(object sender, RoutedEventArgs e)
        {
            if (!resNet.IsProcessingNow)
            {
                Results.Clear();
                dictionary.Clear();
                cts = new CancellationTokenSource();
                var f = new FolderBrowserDialog();
                if (f.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                {
                    button.Content = "Отмена";
                    var nonProcessedFiles = new List<string>();
                    FileView file = null;
                    foreach (var i in Directory.GetFiles(f.SelectedPath))
                    {
                        if (!(i.Contains(".png") ||
                            i.Contains(".jpg") ||
                            i.Contains(".jpeg") ||
                            i.Contains(".bmp")))
                            continue;

                        file = null;
                        file = libraryContext.GetFile(i);
                        if (file == null)
                            nonProcessedFiles.Add(i);
                        else
                        {
                            Results.Add(file);
                            AddImageToUI(file);
                        }
                    }
                    Task t = resNet.ProcessFiles(nonProcessedFiles, cts.Token);
                    if (t != null)
                        await t;
                    button.Content = "Обработать";
                }
            }
            else
            {
                cts.Cancel();
            }            
        }
        public void OnPropertyChanged([CallerMemberName] string prop = "")
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(prop));
        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            libraryContext.Clear();
        }
    }
}
