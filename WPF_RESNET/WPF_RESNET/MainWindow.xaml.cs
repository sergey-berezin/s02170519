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

namespace WPF_RESNET
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window, INotifyPropertyChanged
    {
	    ResNet resNet;
        CancellationTokenSource cts;
        ObservableCollection<ResNetResult> Results { get; set; }
        public IEnumerable<ResNetResult> SelectedClass {  get
            {
                foreach (var i in Results)
                    if (selected == null || i.Label == (selected as string).Split(';')[0])
                            yield return i;
            } }
        object selected;
        public IEnumerable<string> Classes { get {
                var dictionary = new Dictionary<string, int>();
                foreach (var i in Results)
                    if (dictionary.ContainsKey(i.Label))
                    {
                        int value;
                        dictionary.TryGetValue(i.Label, out value);
                        value++;
                        dictionary.Remove(i.Label);
                        dictionary.Add(i.Label, value);
                    }
                    else
                        dictionary.Add(i.Label, 1);

                foreach (var i in dictionary)
                    yield return i.Key + "; Count " + i.Value;
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        //public IEnumerable<string> Classes { get => classes as IEnumerable<string>; }

        public MainWindow()
        {
            InitializeComponent();
            resNet = new ResNet();
            resNet.OnProcessedImage += () => Dispatcher.Invoke(ProcessedImageHandler);
            Results = new ObservableCollection<ResNetResult>();
            //Classes = new ObservableCollection<string>();
            grid.DataContext = this;
            lb.SelectionChanged += (s, e) =>
            {
                selected = lb.SelectedItem;
                OnPropertyChanged("SelectedClass");
            };
        }
        async void ProcessedImageHandler()
        {
            Results.Add(resNet.GetResult());
            OnPropertyChanged("Classes");
            await Task.Run(() =>
            {
                Task.Delay(0);
            });
            if (!resNet.IsProcessingNow)
                button.Content = "Обработать";
        }
	    private void Button_Click(object sender, RoutedEventArgs e)
        {
            if (!resNet.IsProcessingNow)
            {
                Results.Clear();
                cts = new CancellationTokenSource();
                var f = new FolderBrowserDialog();
                if (f.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                {
                    button.Content = "Отмена";
                    resNet.ProcessDirectory(f.SelectedPath, cts.Token);
                }
            }
            else
            {
                button.Content = "Обработать";
                cts.Cancel();
            }            
        }
        public void OnPropertyChanged([CallerMemberName] string prop = "")
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(prop));
        }
    }
}
