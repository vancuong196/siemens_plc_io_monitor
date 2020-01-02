using Newtonsoft.Json;
using PLCDiagnostic.Data;
using PLCDiagnostic.Event;
using PLCDiagnostic.Models;
using PLCDiagnostic.PLC;
using PLCDiagnostic.Services;
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
        private List<PLCMachine> _runningPLCList = new List<PLCMachine>();
        private DispatcherTimer _showNotificationTimer = null;
        private bool _isWatcherStarted = false;
        public ObservableCollection<PlcIO> PLCIOList { set; get; }
        public ObservableCollection<PLCModel> ListPLC { set; get; }

        public MainWindow()
        {

            ShowSplashScreen(500);

            InitializeComponent();

            InitEvent();

            LoadSavedConfig();

        }

        private void InitEvent()
        {
            ApplicationService.Instance.EventAggregatorService.GetEvent<DataReadEvent>().Subscribe(handleDataFromEvent);
            ApplicationService.Instance.EventAggregatorService.GetEvent<ValueChangedEvent>().Subscribe(handleValueChangedEvent);
        }

        private void LoadSavedConfig()
        {
            PLCIOList = new ObservableCollection<PlcIO>();
            PLCIOList = LoadIOsConfigFromFile();
            datagridIOs.ItemsSource = PLCIOList;
            ListPLC = LoadPLCConfigFromFile();
            dataGridPLCConfig.ItemsSource = ListPLC;
            ShowNotification("All configurations are restored.", "WELCOME", 5000);
        }

        private void ShowSplashScreen(int time)
        {
            SplashScreen splash = new SplashScreen("Assets/splash.png");
            splash.Show(true);
            Thread.Sleep(time);
            splash.Show(false);
        }

        private void Window_Closing(object sender, CancelEventArgs e)
        {
            StopThreadBeforeClosing();
        }

        public void StopThreadBeforeClosing()
        {
            foreach (var machine in _runningPLCList)
            {
                try
                {
                    machine.Stop();
                }catch
                {

                }
            }
        }


        private void btnStartStopWatcher_Click(object sender, RoutedEventArgs e)
        {
            if (_isWatcherStarted)
            {
                StopPLCWatcher();
            }
            else
            {
                StartPLCWatcher();
            }
        }

        private void StartPLCWatcher()
        {
            ShowNotification("Starting all jobs.", "INFO", 20000);
            _isWatcherStarted = true;
            btnStartPLCWatcher.Content = "Stop";
            this.btnDeleteSelectedIO.IsEnabled = false;
            this.btnShowAddIODialog.IsEnabled = false;
            this.btnSaveIOConfig.IsEnabled = false;
            foreach (var io in PLCIOList)
            {
                io.Value = "Connecting...";
            }
            for (int i = 0; i < PLCIOList.Count; i++)
            {
                var io = PLCIOList.ElementAt(i);
                var plc = _runningPLCList.Where(machine => machine.Address == io.Controller).FirstOrDefault();
                if (plc == null)
                {
                    plc = new PLCMachine(io.Controller, io.CPUType.Type);
                    plc.ListIO.Add(io);
                    _runningPLCList.Add(plc);
                }
                else
                {
                    plc.ListIO.Add(io);
                }
            }
            foreach (var plc in _runningPLCList)
            {
                new Thread(async () =>
                {
                    var isOK = await plc.Start();
                    if (!isOK)
                    {
                        foreach (var io in plc.ListIO)
                        {
                            io.Value = "Timeout";
                            ShowNotification("Failed to start plc: " + plc.Address, "ERROR", 2000);
                        }
                    }
                    else
                    {
                        ShowNotification("Success start plc: " + plc.Address, "INFO", 2000);
                    }
                }).Start();
            }
        }

        private void StopPLCWatcher()
        {
            ShowNotification("Stopping all jobs.", "INFO", 20000);
            btnStartPLCWatcher.Content = "Start";
            _isWatcherStarted = false;
            foreach (var plc in _runningPLCList)
            {
                try
                {
                    plc.Stop();
                }
                catch
                {

                }
            }
            _runningPLCList.Clear();
            foreach (var io in PLCIOList)
            {
                io.Value = "N/A";
            }

            btnDeleteSelectedIO.IsEnabled = true;
            btnShowAddIODialog.IsEnabled = true;
            btnSaveIOConfig.IsEnabled = true;

            ShowNotification("Stopped all jobs.", "INFO", 3000);
            return;
        }


        private void handleDataFromEvent(PlcEventModel plcEventModel)
        {
            if (plcEventModel == null)
            {
                return;
            }
            var currentIO = PLCIOList.FirstOrDefault(io => io.Address.Equals(plcEventModel.Address));
            if (currentIO != null)
            {
                currentIO.Value = plcEventModel.Data;
            }
        }

        private void handleValueChangedEvent(ValueChangedModel valueChangedModel)
        {
            DateTime date = DateTime.Now;
            Run run = new Run(date.ToString() + ": Detect value changed in " + valueChangedModel.Address + " from " + valueChangedModel.OldValue + " to " + valueChangedModel.NewValue);
            Paragraph p = new Paragraph(run);
            Console.WriteLine(run.ToString());

            Dispatcher.CurrentDispatcher.BeginInvoke((Action)(() =>
            {
                //      LogViewer.Blocks.Add(p);
            }));
        }

        private void BtnShowAddIODiaglog_Click(object sender, RoutedEventArgs e)
        {
            InitAddIODialog();
        }

        private void BtnSaveIOConfig_Click(object sender, RoutedEventArgs e)
        {
            File.WriteAllText("config.json", JsonConvert.SerializeObject(PLCIOList));
            ShowNotification("Saved IOs configuration.", "INFO", 3000);
        }

        private ObservableCollection<PlcIO> LoadIOsConfigFromFile()
        {
            try
            {
                var iosCongfigJson = File.ReadAllText("config.json");
                var iosConfig = JsonConvert.DeserializeObject<ObservableCollection<PlcIO>>(iosCongfigJson);
                foreach (var io in iosConfig )
                {
                    io.Value = "N/A";
                }
                return iosConfig;
            } catch
            {
                return new ObservableCollection<PlcIO>();
            }
           
        }
        private ObservableCollection<PLCModel> LoadPLCConfigFromFile()
        {
            try
            {
                var plcConfigJson = File.ReadAllText("configPLC.json");
                var plcConfig = JsonConvert.DeserializeObject<ObservableCollection<PLCModel>>(plcConfigJson);

                return plcConfig;
            }
            catch
            {
                try
                {
                    File.WriteAllText("configPLC.json", JsonConvert.SerializeObject(ControllerInfomationHolder.Instance.ListController));
                    return LoadPLCConfigFromFile();
                } catch
                {
                    return new ObservableCollection<PLCModel>();
                }
            }

        }

        private void BtnDeleteButtonClicked(object sender, RoutedEventArgs e)
        {
            var toRemoveIOs = new List<PlcIO>();
            foreach (var selectedItem in datagridIOs.SelectedItems)
            {
                toRemoveIOs.Add(selectedItem as PlcIO);
            }
            foreach (var selectedItem in toRemoveIOs)
            {
                PLCIOList.Remove(selectedItem as PlcIO);
            }
            ShowNotification("Deleted selected IO.", "INFO", 3000);

        }

        private void BtnSavePLCConfig_Click(object sender, RoutedEventArgs e)
        {
            File.WriteAllText("configPLC.json", JsonConvert.SerializeObject(ListPLC));
            foreach (var item in PLCIOList)
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
            File.WriteAllText("config.json", JsonConvert.SerializeObject(PLCIOList));
            ShowNotification("Saved all configurations.", "INFO", 3000);
        }

        private void MainTabControl_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (dialogViewAddIO.Visibility == Visibility.Visible)
            {
                if (tabView.SelectedItem!= ioTab)
                {
                    dialogViewAddIO.Visibility = Visibility.Collapsed;
                }
            }
        }

        public void ShowNotification(string content,string title, double milisecond= 3000)
        {
            Dispatcher.BeginInvoke(DispatcherPriority.Normal, new Action( () =>
            {
                if (_showNotificationTimer != null)
                {
                    _showNotificationTimer.Stop();
                    viewNotification.Visibility = Visibility.Collapsed;
                }
                _showNotificationTimer = new DispatcherTimer();
                _showNotificationTimer.Interval = TimeSpan.FromMilliseconds(milisecond);
                tvDialogTile.Text = title;
                tvDialogContent.Text = content;
                viewNotification.Visibility = Visibility.Visible;
                _showNotificationTimer.Tick += (s, e) =>
                {
                    _showNotificationTimer.Stop();
                    _showNotificationTimer = null;
                    
                    viewNotification.Visibility = Visibility.Collapsed;

                };
                _showNotificationTimer.Start();
            }));
          
        }

        private void BtnDismissNotificationClicked(object sender, RoutedEventArgs e)
        {
            viewNotification.Visibility = Visibility.Collapsed;
        }

        private void BtnWriteIOValue_Click(object sender, RoutedEventArgs e)
        {
            var clickedButton = sender as Button;
            var selectedIO = clickedButton.DataContext as PlcIO;
            if (selectedIO!=null)
            {
                var plc = _runningPLCList.FirstOrDefault(p => p.Address == selectedIO.Controller);
                if (plc!=null)
                {
                    var isCompleted = plc.WriteIOToPLC(selectedIO);
                    if (isCompleted)
                    {
                        ShowNotification("Write: " + selectedIO.WriteValue + " to " + selectedIO.WriteAddress, "INFO", 3000);
                    } else
                    {
                        ShowNotification("Can't Write: " + selectedIO.WriteValue + " to " + selectedIO.WriteAddress, "INFO", 3000);
                    }
                }
            }
        }


        #region Add IO
        public class TypeModel
        {
            public int Type { set; get; }
            public string Name { set; get; }
        }
        public List<TypeModel> ListSupportValueType;

        public void InitAddIODialog()
        {
            ListSupportValueType = new List<TypeModel>();
            ListSupportValueType.Add(new TypeModel()
            {
                Name = "Int",
                Type = IOTypeConstants.INT_TYPE
            });
            ListSupportValueType.Add(new TypeModel()
            {
                Name = "Boolean",
                Type = IOTypeConstants.BOOL_TYPE
            });
            ListSupportValueType.Add(new TypeModel()
            {
                Name = "Float",
                Type = IOTypeConstants.FLOAT_TYPE
            });
            ListSupportValueType.Add(new TypeModel()
            {
                Name = "Byte",
                Type = IOTypeConstants.BYTE_TYPE
            });
            ListSupportValueType.Add(new TypeModel()
            {
                Name = "Word",
                Type = IOTypeConstants.WORD_TYPE
            });
            comboboxValueType.ItemsSource = ListSupportValueType;
            comboboxValueType.SelectedIndex = 0;
            comboboxCPUType.ItemsSource = ListPLC;
            comboboxCPUType.SelectedIndex = 0;

            dialogViewAddIO.Visibility = Visibility.Visible;
        }
        private void Button_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(tb_Name.Text))
            {
                ShowNotification("Please input name of IO", "ERROR");
                return;
            }
            if (string.IsNullOrEmpty(tb_Address.Text))
            {
                ShowNotification("Please input address of IO", "ERROR");
                return;
            }
            if (comboboxValueType.SelectedItem == null)
            {
                ShowNotification("Please input address of IO", "ERROR");
                return;
            }
            if (comboboxCPUType.SelectedItem == null)
            {
                ShowNotification("Please select a controller!", "ERROR");
                return;
            }
            var selectController = comboboxCPUType.SelectedItem as PLCModel;
            var toAddIO = new PlcIO()
            {
                Name = tb_Name.Text,
                Value = "N/A",
                Address = tb_Address.Text,
                Type = (comboboxValueType.SelectedItem as TypeModel).Type,
                Controller = selectController.Address,
                CPUType = new CpuTypeModel()
                {
                    Type = selectController.CpuType
                },
                PlcId = selectController.Id
            };
            PLCIOList.Add(toAddIO);
            tb_Name.Text = "";
            tb_Address.Text = "";
            dialogViewAddIO.Visibility = Visibility.Collapsed;
            ShowNotification("Added new IO!", "INFO", 3000);
        }

        private void btnCancelAddIO_Click(object sender, RoutedEventArgs e)
        {
            //to-do clear
            dialogViewAddIO.Visibility = Visibility.Collapsed;
        }


        #endregion

      
    }
}
