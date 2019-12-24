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
    /// 
    public partial class MainWindow : Window
    {
        private PLCMachine plc;
        private List<PLCMachine> machineList;
        public ObservableCollection<PlcIO> ListIO { set; get; }
        public ObservableCollection<PLCModel> ListPLC { set; get; }
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
            ListPLC = LoadSetupPLC();
            dataGridPLCConfig.ItemsSource = ListPLC;
            machineList = new List<PLCMachine>();
            ShowNotification("All configurations are stored.", "Welcome", 4000);

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
        private async void ButtonStart_Click(object sender, RoutedEventArgs e)
        {/*
            if (String.IsNullOrEmpty(controllerAddressTb.Text))
            {
                MessageBox.Show("Please input controller address", "Error");
                return;
            }*/

          //
          if (isStart)
            {
                ShowNotification("Stopping all jobs!", "Info", 2000);
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
                foreach (var io in ListIO)
                {
                    io.Value = "Stop";
                }
                ShowNotification("Stopped all jobs!", "Info", 2000);
                return;
            }
            ShowNotification("Starting all jobs!", "Info", 2000);
            isStart = true;
            StartIOButton.Content = "Stop";
            foreach (var io in ListIO)
            {
                io.Value = "Connecting...";
            }
            for (int i =0; i<ListIO.Count;i++) 
            {
                var io = ListIO.ElementAt(i);
                var plc = machineList.Where(machine => machine.Address == io.Address).FirstOrDefault();
                if (plc==null)
                {
                    plc = new PLCMachine(io.Controller, io.CPUType.Type);
                    plc.ListIO.Add(io);
                    machineList.Add(plc);
                } else
                {
                    plc.ListIO.Add(io);
                }
            }
            foreach (var plc in machineList)
            {
                new Thread(async () =>
               {
                   var isOK = await plc.Start();
                   if (!isOK)
                   {
                       foreach (var io in plc.ListIO)
                       {
                           io.Value = "Time out";
                           ShowNotification("Failed to start plc: " + plc.Address, "Error", 2000);
                       }
                   } else
                   {
                       ShowNotification("Success start plc: "+plc.Address, "Info", 2000);
                   }
               }).Start();
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
            AddIODialog();
            if (AddIOView.Visibility== Visibility.Collapsed)
            {

                AddIOView.Visibility = Visibility.Visible;
            } else
            {
                AddIOView.Visibility = Visibility.Collapsed;
            }
        }

        private void SaveIOButton_Click(object sender, RoutedEventArgs e)
        {
            File.WriteAllText("config.json", JsonConvert.SerializeObject(ListIO));
            ShowNotification("Saved IO configuration", "Info", 3000);
        }
        private ObservableCollection<PlcIO> LoadSetup()
        {
            try
            {
                var text = File.ReadAllText("config.json");
                var config = JsonConvert.DeserializeObject<ObservableCollection<PlcIO>>(text);
                foreach (var io in config )
                {
                    io.Value = "Stop";
                }
                return config;
            } catch
            {
                return new ObservableCollection<PlcIO>();
            }
           
        }

        private ObservableCollection<PLCModel> LoadSetupPLC()
        {
            try
            {
                var text = File.ReadAllText("configPLC.json");
                var config = JsonConvert.DeserializeObject<ObservableCollection<PLCModel>>(text);

                return config;
            }
            catch
            {
                try
                {
                    File.WriteAllText("configPLC.json", JsonConvert.SerializeObject(ControllerInfomationHolder.Instance.ListController));
                    return LoadSetupPLC();
                } catch
                {
                    return new ObservableCollection<PLCModel>();
                }
            }

        }



        private void DeletedIOButton_Click(object sender, RoutedEventArgs e)
        {

            var listIO = ListIO.ToList();
            listIO.RemoveAll(i => i.IsEnable == true);
            ListIO = new ObservableCollection<PlcIO>( listIO);
            dataGrid.ItemsSource = ListIO;
            ShowNotification("Deleted selected IO!", "Info", 3000);

        }

        #region Add IO
        public class TypeModel
        {
            public int Type { set; get; }
            public string Name { set; get; }
        }
        public List<TypeModel> ListType;
        public List<CpuTypeModel> ListCPUType;
        public void AddIODialog()
        {
            ListType = new List<TypeModel>();
            ListType.Add(new TypeModel()
            {
                Name = "Int",
                Type = IOTypeConstants.INT_TYPE
            });
            ListType.Add(new TypeModel()
            {
                Name = "Boolean",
                Type = IOTypeConstants.BOOL_TYPE
            });
            ListType.Add(new TypeModel()
            {
                Name = "Float",
                Type = IOTypeConstants.FLOAT_TYPE
            });
            ListType.Add(new TypeModel()
            {
                Name = "Byte",
                Type = IOTypeConstants.BYTE_TYPE
            });
            ListCPUType = new List<CpuTypeModel>();
            ListCPUType.Add(new CpuTypeModel()
            {
                Type = S7.Net.CpuType.S71200
            });
            ListCPUType.Add(new CpuTypeModel()
            {
                Type = S7.Net.CpuType.S71500
            });
            ListCPUType.Add(new CpuTypeModel()
            {
                Type = S7.Net.CpuType.S7200
            });
            ListCPUType.Add(new CpuTypeModel()
            {
                Type = S7.Net.CpuType.S7300
            });
            ListCPUType.Add(new CpuTypeModel()
            {
                Type = S7.Net.CpuType.S7400
            });
            InitializeComponent();
            cb_Type.ItemsSource = ListType;
            cb_Type.SelectedIndex = 0;
            cbCpuType.ItemsSource = ListPLC;
            cbCpuType.SelectedIndex = 0;
        }
        private void Button_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(tb_Name.Text))
            {
                ShowNotification("Please input name of IO", "Input Error");
                return;
            }
            if (string.IsNullOrEmpty(tb_Address.Text))
            {
                ShowNotification("Please input address of IO", "Input Error");
                return;
            }
            if (cb_Type.SelectedItem == null)
            {
                ShowNotification("Please input address of IO", "Input Error");
                return;
            }
            if (cbCpuType.SelectedItem==null)
            {
                ShowNotification("Please select a controller!", "Input Error");
                return;
            }
            var selectController = cbCpuType.SelectedItem as PLCModel;
            ToAddIO = new PlcIO()
            {
                Name = tb_Name.Text,
                Value = "N/A",
                Address = tb_Address.Text,
                Type = (cb_Type.SelectedItem as TypeModel).Type,
                Controller = selectController.Address,
                CPUType = new CpuTypeModel()
                {
                    Type = selectController.CpuType
                },
                PlcId = selectController.Id
            };
            ListIO.Add(ToAddIO);
            tb_Name.Text = "";
            tb_Address.Text = "";
            AddIOView.Visibility = Visibility.Collapsed;
            ShowNotification("Added new IO!", "Info", 3000);
        }
        public PlcIO ToAddIO { set; get; }
        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            //to-do clear
            AddIOView.Visibility = Visibility.Collapsed;
        }


        #endregion

        private void Button_Click_2(object sender, RoutedEventArgs e)
        {
            File.WriteAllText("configPLC.json", JsonConvert.SerializeObject(ListPLC));
            foreach (var item in ListIO)
            {
                var plc = ListPLC.FirstOrDefault(i => i.Id == item.PlcId);
                if (plc==null)
                {
                    continue;
                }
                    item.Controller = plc.Address;

                    item.CPUType = new CpuTypeModel()
                    {
                        Type = plc.CpuType
                    };
            }
            File.WriteAllText("configPLC.json", JsonConvert.SerializeObject(ListIO));
            ShowNotification("Saved all configuration!", "Info", 3000);
        }

        private void TabControl_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (AddIOView.Visibility == Visibility.Visible)
            {
                if (tabView.SelectedItem!= ioTab)
                {
                    AddIOView.Visibility = Visibility.Collapsed;
                }
            }
        }
        DispatcherTimer ShowNotificationTimer = null;
        public void ShowNotification(string content,string title, double milisecond= 3000)
        {
            Dispatcher.BeginInvoke(DispatcherPriority.Normal, new Action( () =>
            {
                if (ShowNotificationTimer != null)
                {
                    ShowNotificationTimer.Stop();
                    viewDialog.Visibility = Visibility.Collapsed;
                }
                ShowNotificationTimer = new DispatcherTimer();
                ShowNotificationTimer.Interval = TimeSpan.FromMilliseconds(milisecond);
                tvDialogTile.Text = title;
                tvDialogContent.Text = content;
                viewDialog.Visibility = Visibility.Visible;
                ShowNotificationTimer.Tick += (s, e) =>
                {
                    ShowNotificationTimer.Stop();
                    ShowNotificationTimer = null;
                    
                    viewDialog.Visibility = Visibility.Collapsed;

                };
                ShowNotificationTimer.Start();
            }));
          
        }
        private void Button_Click_3(object sender, RoutedEventArgs e)
        {
            viewDialog.Visibility = Visibility.Collapsed;
        }
    }
}
