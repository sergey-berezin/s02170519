using Microsoft.Win32;
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
using System.Net.Http;
using Contracts;
using System.Threading.Tasks.Dataflow;

namespace WPF_RESNET
{    
    public partial class MainWindow : Window, INotifyPropertyChanged
    {
        public class SendDataView
        {
            public string NumberOfRequests { get; set; }
            public string TypeName { get; set; }
            public string Data { get; set; }
            public BitmapSource Image { get; set; }
            public SendDataView(string id, string data)
            {
                TypeName = id;
                NumberOfRequests = "-";
                Data = data;
                Image = Utils.LoadImage(Convert.FromBase64String(data));
            }
        }
        Client client;
        ObservableCollection<SendDataView> Results { get; set; }
        public IEnumerable<SendDataView> SelectedClass {  get
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
            //libraryContext.Clear();
            client = new Client();
            client.OnProcessedImage += () => Dispatcher.Invoke(ProcessedImageHandler);
            client.OnGetStatistic += () => Dispatcher.Invoke(StatisticHandler);
            client.OnConnectionFailed += () => Dispatcher.Invoke(() => Label.Content = "Connection failed");
            Results = new ObservableCollection<SendDataView>();
            grid.DataContext = this;
            lb.SelectionChanged += (s, e) =>
            {
                selected = lb.SelectedItem;
                OnPropertyChanged("SelectedClass");
            };
            client.GetAll();
        }
        void StatisticHandler()
        {
            var result = client.GetStatistic();
            foreach (var i in Results)
            {
                if (i.Data == result.Data)
                {
                    i.NumberOfRequests = result.TypeName;

                    OnPropertyChanged("SelectedClass");
                    return;
                }
            }
        }
        void ProcessedImageHandler()
        {
            var result = client.GetResult();
            if (result != null)
            {
                var r = new SendDataView(result.TypeName, result.Data);
                Results.Add(r);
                AddImageToUI(r);
            }
        }
        void AddImageToUI(SendDataView result)
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
	    private void Button_Click(object sender, RoutedEventArgs e)
        {
            Label.Content = "";
            Results.Clear();
            dictionary.Clear();
            var f = new FolderBrowserDialog();
            if (f.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                var workerBlock = new ActionBlock<string>
                        (
                            client.ProcessPicture,
                            new ExecutionDataflowBlockOptions { MaxDegreeOfParallelism = 4 }
                        );
                foreach (var i in Directory.GetFiles(f.SelectedPath))
                    workerBlock.Post(i);
                OnPropertyChanged("Classes");
                OnPropertyChanged("SelectedClass");
            }
        }
        public void OnPropertyChanged([CallerMemberName] string prop = "")
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(prop));
        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            Label.Content = "";  
            client.Clear();
        }

        private void Button_Click_2(object sender, RoutedEventArgs e)
        {
            Label.Content = "";
            foreach (var i in Results)
                client.GetStatistic(i);
        }
    }
}
