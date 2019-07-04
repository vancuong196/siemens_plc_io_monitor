using PLCDiagnostic.Data;
using PLCDiagnostic.Models;
using PLCDiagnostic.PLC;
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

namespace PLCDiagnostic.Views
{
    /// <summary>
    /// Interaction logic for AddIODialog.xaml
    /// </summary>
    public partial class AddIODialog : Window
    {
        public class TypeModel{
            public int Type { set; get; }
            public string Name { set; get; }
        }
        public List<TypeModel> ListType;
        public List<CpuTypeModel> ListCPUType;
        public AddIODialog()
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
            cbCpuType.ItemsSource = ListCPUType;
            cbCpuType.SelectedIndex = 0;
        }

        private void TextBox_TextChanged(object sender, TextChangedEventArgs e)
        {

        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(tb_Name.Text))
            {
                MessageBox.Show("Please input name of IO", "Input Error");
                return;
            }
            if (string.IsNullOrEmpty(tb_Address.Text))
            {
                MessageBox.Show("Please input address of IO", "Input Error");
                return;
            }
            if (cb_Type.SelectedItem == null)
            {
                MessageBox.Show("Please input address of IO", "Input Error");
                return;
            }
            if (String.IsNullOrEmpty(controllerAddressTb.Text ))
            {
                MessageBox.Show("Please input address of controller", "Input Error");
                return;
            }
            ToAddIO = new PlcIO()
            {
                Name = tb_Name.Text,
                Value = "N/A",
                Address = tb_Address.Text,
                Type = (cb_Type.SelectedItem as TypeModel).Type,
                Controller = controllerAddressTb.Text.Trim(),
                CPUType = cbCpuType.SelectedItem as CpuTypeModel
            };
            DialogResult = true;
        }
        public PlcIO ToAddIO { set; get; } 
        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
        }
    }
}
