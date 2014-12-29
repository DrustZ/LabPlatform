using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Collections.Generic;

namespace WpfApplication3
{
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow : Window
    {
        private string inputfile;
        private string outputfile;
        private List<string> sentenses;

        public MainWindow()
        {
            InitializeComponent();
            inputfile = outputfile = "";
            sentenses = new List<string>();
        }

        private void Button_Click_1(object sender, RoutedEventArgs e)//start 操作，将读入的句子依次显示
        {
            if （inputfile != 
            StreamReader sr = File.OpenText（inputfile);
            return;
        }

        private void Button_Click_2(object sender, RoutedEventArgs e)//write file 操作
        {
           Microsoft.Win32.OpenFileDialog ofd = new Microsoft.Win32.OpenFileDialog();
           ofd.DefaultExt = ".txt";
           if (ofd.ShowDialog() == true)
               outputfile = ofd.FileName;        
        }

        private void Button_Click_3(object sender, RoutedEventArgs e)//read file 操作
        {
           Microsoft.Win32.OpenFileDialog ofd = new Microsoft.Win32.OpenFileDialog();
           ofd.DefaultExt = ".txt";
           if (ofd.ShowDialog() == true)
               inputfile = ofd.FileName;
        }
    }
}
