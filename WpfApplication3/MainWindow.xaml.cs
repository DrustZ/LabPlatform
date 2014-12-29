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
        private int countnow;
        private bool started;
        private bool getW, getR;

        public MainWindow()
        {
            InitializeComponent();
            inputfile = outputfile = "";
            sentenses = new List<string>();
            countnow = 0;
            stop.IsEnabled = false;
            next.IsEnabled = false;
            type.IsEnabled = false;
        }

        public bool startTest()
        {
            if (!getR || !getW)
            {
                MessageBox.Show("您还没有选择输入输出文件！");
                return false;
            }
            try
            {
                // Create an instance of StreamReader to read from a file.
                // The using statement also closes the StreamReader.
                using (StreamReader sr = new StreamReader(inputfile, Encoding.UTF8))
                {
                    String line;
                    // Read and display lines from the file until the end of 
                    // the file is reached.
                    while ((line = sr.ReadLine()) != null)
                    {
                        sentenses.Add(line);
                    }
                }
            }
            catch (Exception err)
            {
                // Let the user know what went wrong.
                Console.WriteLine("The file could not be read:");
                Console.WriteLine(err.Message);
            }
            countnow = 0;
            return true;
        }

        public bool stopTest()
        {
            if (started)
            {
                started = false;
                return true;
            }
            return false;
        }

        public void getWirteFile()
        {
            if (started)
            {
                MessageBox.Show("测试已开始，请先停止");
                return;
            }
            Microsoft.Win32.OpenFileDialog ofd = new Microsoft.Win32.OpenFileDialog();
            ofd.DefaultExt = ".txt";
            if (ofd.ShowDialog() == true)
                outputfile = ofd.FileName;
            started = false;
            getW = true;
        }

        public void getReadFile()
        {
            if (started)
            {
                MessageBox.Show("测试已开始，请先停止");
                return;
            }
            Microsoft.Win32.OpenFileDialog ofd = new Microsoft.Win32.OpenFileDialog();
            ofd.DefaultExt = ".txt";
            if (ofd.ShowDialog() == true)
                inputfile = ofd.FileName;
            started = false;
            getR = true;
        }

        public string showNext()
        {
            if (started)
                if (sentenses.Count > countnow)
                    return sentenses[countnow++];
                else
                {
                    started = false;
                    string s = "所有输入已完成，谢谢您的参与";
                    return s;
                }
            else {
                MessageBox.Show("请先开始");
                return "";
            }
        }

        private void Button_Click_1(object sender, RoutedEventArgs e)//start 操作，将读入的句子依次显示
        {
            if (startTest())
            {
                started = true;
                start.IsEnabled = false;
                stop.IsEnabled = true;
                next.IsEnabled = true;
                string nextText = showNext();
                if (nextText != "")
                    DisplayTest.Text = nextText;
            }
        }

        private void Button_Click_2(object sender, RoutedEventArgs e)//write file 操作
        {
            getWirteFile();     
        }

        private void Button_Click_3(object sender, RoutedEventArgs e)//read file 操作
        {
            getReadFile();
        }

        private void next_Click(object sender, RoutedEventArgs e)//next 操作
        {
            string nextText = showNext();
            if (nextText != "")
                DisplayTest.Text = nextText;
            if (nextText == "所有输入已完成，谢谢您的参与")
            {
                next.IsEnabled = false;
                start.IsEnabled = true;
                stop.IsEnabled = false;
            }
        }

        private void stop_Click(object sender, RoutedEventArgs e)
        {
            if (stopTest())
            {
                DisplayTest.Text = "您已经结束测试。";
                stop.IsEnabled = false;
                start.IsEnabled = true;
                next.IsEnabled = false;
            }
        }

        
    }
}
