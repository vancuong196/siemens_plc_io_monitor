using Newtonsoft.Json;
using PLCDiagnostic.Data;
using PLCDiagnostic.Event;
using PLCDiagnostic.Models;
using PLCDiagnostic.PLC;
using PLCDiagnostic.Services;
using PLCDiagnostic.Views;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
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
using System.Windows.Threading;

namespace PLCDiagnostic
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private PLCMachine plc;
        private List<PLCMachine> machineList;
        public ObservableCollection<PlcIO> ListIO { set; get; }
        public MainWindow()
        {
            ListIO = new ObservableCollection<PlcIO>();
            ListIO.Add(new PlcIO
            {
                Name = "Test 1",
                Address = "DB14.DBD52",
                Type = IOTypeConstants.FLOAT_TYPE,
                Value = "N/A"
            });
            ListIO.Add(new PlcIO
            {
                Name = "Test 2",
                Address = "DB34.DBX16.2",
                Type = IOTypeConstants.BOOL_TYPE,
                Value = "N/A"
            });
            InitializeComponent();
            ApplicationService.Instance.EventAggregatorService.GetEvent<DataReadEvent>().Subscribe(handleDataFromEvent);
            ApplicationService.Instance.EventAggregatorService.GetEvent<ValueChangedEvent>().Subscribe(handleValueChangedEvent);
            ListIO = LoadSetup();
            dataGrid.ItemsSource = ListIO;
            machineList = new List<PLCMachine>();
           
        }
        private void Window_Closing(object sender, CancelEventArgs e)
        {
            onClosed();
        }
        public void onClosed()
        {
            foreach (var machine in machineList)
            {
                try
                {
                    machine.Stop();
                }catch
                {

                }
            }
        }
        public bool isStart = false;
        private void ButtonStart_Click(object sender, RoutedEventArgs e)
        {/*
            if (String.IsNullOrEmpty(controllerAddressTb.Text))
            {
                MessageBox.Show("Please input controller address", "Error");
                return;
            }*/

          //
          if (isStart)
            {
                StartIOButton.Content = "Start";
                isStart = false;
                foreach (var machine in machineList)
                {
                    try
                    {
                        machine.Stop();
                    }catch
                    {

                    }
                }
                machineList.Clear();
                return;
            }
            isStart = true;
            StartIOButton.Content = "Stop";

            foreach (var io in ListIO)
            {
                var plc = machineList.Where(machine => machine.Address == io.Address).FirstOrDefault();
                if (plc==null)
                {
                    
                    plc = new PLCMachine(io.Controller, io.CPUType.Type);
                    plc.ListIO.Add(io);
                    plc.Start();
                    machineList.Add(plc);
                } else
                {
                    plc.ListIO.Add(io);
                }

               
            }
           
        }
        private void subscribeDataFromPLC()
        {
            ApplicationService.Instance.EventAggregatorService.GetEvent<DataReadEvent>().Subscribe(handleDataFromEvent);
        }
        private void handleDataFromEvent(PlcEventModel plcEventModel)
        {
            if (plcEventModel == null)
            {
                return;
            }
            var currentIO = ListIO.FirstOrDefault(io => io.Address.Equals(plcEventModel.Address));
            if (currentIO != null)
            {
                currentIO.Value = plcEventModel.Data;
            }

        }
          private void handleValueChangedEvent(ValueChangedModel valueChangedModel)
        {
            DateTime date = DateTime.Now;
            Run run = new Run(date.ToString()+": Detect value changed in "+valueChangedModel.Address+" from "+valueChangedModel.OldValue+" to "+valueChangedModel.NewValue);
            Paragraph p = new Paragraph(run);
            Console.WriteLine(run.ToString());

            Dispatcher.CurrentDispatcher.BeginInvoke((Action)(() =>
            {
                LogViewer.Blocks.Add(p);
            }));
        }

        private void AddIOButton_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new AddIODialog();
            if (dialog.ShowDialog() == true)
            {
                if (ListIO.Any(io=>io.Address.Equals(dialog.ToAddIO.Address)))
                {
                    MessageBox.Show("Address is selected", "Error");
                    return;
                }
                if (!isStart)
                {
                    ListIO.Add(dialog.ToAddIO);
                    return;
                }

                    var plc = machineList.Where(machine => machine.Address == dialog.ToAddIO.Address).FirstOrDefault();
                    if (plc == null)
                    {

                        plc = new PLCMachine(dialog.ToAddIO.Controller, dialog.ToAddIO.CPUType.Type);
                        plc.ListIO.Add(dialog.ToAddIO);
                        plc.Start();
                    machineList.Add(plc);
                    }
                    else
                    {
                        plc.ListIO.Add(dialog.ToAddIO);
                    }
                ListIO.Add(dialog.ToAddIO);
            }
        }

        private void SaveIOButton_Click(object sender, RoutedEventArgs e)
        {
            File.WriteAllText("config.json", JsonConvert.SerializeObject(ListIO));
        }
        private ObservableCollection<PlcIO> LoadSetup()
        {
            try
            {
                var text = File.ReadAllText("config.json");
                var config = JsonConvert.DeserializeObject<ObservableCollection<PlcIO>>(text);

                return config;
            } catch
            {
                return new ObservableCollection<PlcIO>();
            }
           
        }


        private void DeletedIOButton_Click(object sender, RoutedEventArgs e)
        {

            var listIO = ListIO.ToList();
            listIO.RemoveAll(i => i.IsEnable == true);
            ListIO = new ObservableCollection<PlcIO>( listIO);
            dataGrid.ItemsSource = ListIO;

           
        }
    }
}
