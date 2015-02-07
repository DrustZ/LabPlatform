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
using System.Windows.Controls.DataVisualization.Charting;

using Leap;

using KalmanCS;

namespace WpfApplication3
{
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow : Window, ILeapEventDelegate
    {
        private string inputfile;
        private string outputfile;
        private List<string> sentenses;
        private int countnow,datanow,valuenow;
        private int FingerToShow, CoordToShow;
        private bool started, recordVelocity, recordJoint, kalmaned;
        private bool getW, getR;
        private string outputName;
        private Controller controller = new Controller();
        private LeapEventListener listener;
        private Boolean isClosing = false;
        private double[] measuredValue, filteredValue;
        private Leap.Vector[][][] tempValue;
        private double sourceSig, measuredSig;
        private string[] time;
        private List<KeyValuePair<int, float>> Actualvalue, Filteredvalue;
        private List<List<KeyValuePair<int, float>>> dataSourceList;
        public MainWindow()
        {
            try
            {
                InitializeComponent();
                FingerToShow = 5;
                CoordToShow = 0;
                inputfile = outputfile = "";
                measuredValue = new double[1000];
                filteredValue = new double[1000];
                tempValue = new Leap.Vector[10][][];
                for (int i = 0; i < 10; ++i)
                {
                    tempValue[i] = new Leap.Vector[10][];
                    for (int j = 0; j < 10; ++j)
                        tempValue[i][j] = new Leap.Vector[60];
                }

                getR = false;

                time = new string[100];
                sentenses = new List<string>();
                started = false;
                stop.IsEnabled = false;
                next.IsEnabled = false;
                type.IsEnabled = false;
                keyBoard.Source = new BitmapImage(new Uri(System.AppDomain.CurrentDomain.SetupInformation.ApplicationBase + @"../../keyboard.jpg"));

                this.controller = new Controller();
                this.listener = new LeapEventListener(this);
                controller.AddListener(listener);
                Actualvalue = new List<KeyValuePair<int, float>>();
                Filteredvalue = new List<KeyValuePair<int, float>>();
                dataSourceList = new List<List<KeyValuePair<int, float>>>();
                dataSourceList.Add(Actualvalue);
                dataSourceList.Add(Filteredvalue);

                chart1.DataContext = dataSourceList;
            }
            catch (Exception e)
            {
            }
        }

        delegate void LeapEventDelegate(string EventName);

        public bool startTest()
        {
            valuenow = countnow = datanow = 0;
            recordVelocity = false;
            recordJoint = false;

            //check the selections
            if (Velo.IsChecked == true)
            {
                recordVelocity = true;
            }
            if (Join.IsChecked == true)
            {
                recordJoint = true;
            }
                      
            if (!getR )
            {
                MessageBox.Show("您还没有选择输入文件！");
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
                    sr.Close();
                }

            }
            catch (Exception err)
            {
                // Let the user know what went wrong.
                Console.WriteLine("The file could not be reada:");
                Console.WriteLine(err.Message);
            }
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

        //显示下一句话
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

        private void Button_Start_Click(object sender, RoutedEventArgs e)//start 操作，将读入的句子依次显示
        {
            if (startTest())
            {
                valuenow = datanow = 0;
                dataSourceList[0].Clear();
                dataSourceList[1].Clear();
                started = true;

                sourceSig = double.Parse(sourceSigma.Text);//卡尔曼滤波的参数初始化
                measuredSig = double.Parse(measuredSigma.Text);

                start.IsEnabled = false;
                stop.IsEnabled = true;
                next.IsEnabled = true;
                type.IsEnabled = true;
                Velo.IsEnabled = false;
                Join.IsEnabled = false;
                string nextText = showNext();
                if (nextText != "")
                    DisplayTest.Text = nextText;
            }
        }

        private void Button_ReadF_Click(object sender, RoutedEventArgs e)//read file 操作
        {
            getReadFile();
        }

        private void Button_Next_Click(object sender, RoutedEventArgs e)//next 操作
        {
            string nextText = showNext();
            if (nextText != "")
                DisplayTest.Text = nextText;
            if (nextText == "所有输入已完成，谢谢您的参与")
            {
                next.IsEnabled = false;
                start.IsEnabled = true;
                stop.IsEnabled = false;
                type.IsEnabled = false;
                Velo.IsEnabled = true;
                Join.IsEnabled = true;
            }
        }

        private void Button_Stop_Click(object sender, RoutedEventArgs e)
        {
            if (stopTest())
            {
                DisplayTest.Text = "您已经结束测试。";
                stop.IsEnabled = false;
                start.IsEnabled = true;
                next.IsEnabled = false;
                type.IsEnabled = false;
                Velo.IsEnabled = true;
                Join.IsEnabled = true;
            }
        }

        private void Button_Type_Click(object sender, RoutedEventArgs e)
        {
            Dictionary<string, string> infomation = new Dictionary<string, string>();
            infomation.Add("test", "test");
            infomation.Add("test2", "test2");
        }

        //leap motion 监听函数
        public void LeapEventNotification(string EventName)
        {
            if (this.CheckAccess())
            {
                switch (EventName)
                {
                    case "onInit":
                        //Debug.WriteLine("Init");
                        break;
                    case "onConnect":
                        this.connectHandler();
                        break;
                    case "onFrame":
                        if(!this.isClosing)
                            this.newFrameHandler(this.controller.Frame());
                        break;
                }
            }
            else
            {
                Dispatcher.Invoke(new LeapEventDelegate(LeapEventNotification), new object[] { EventName });
            }
        }

        void connectHandler()
        {
            this.controller.SetPolicy(Controller.PolicyFlag.POLICY_OPTIMIZE_HMD);
        }

        //主要函数：start后获取到每一个有效帧后（即有手的帧）的处理函数
        void newFrameHandler(Leap.Frame frame)
        {
            HandList hands = frame.Hands;

            if (hands.Count == 1 && this.started)
            {
                Hand hand = hands[0];
                FingerList fingers = hand.Fingers;
                if (hand.StabilizedPalmPosition == null)
                    return;
                string outputLine = "";
                time[valuenow] = string.Format("{0:HHmmssffff}", DateTime.Now);
                //infoShow.Text = hand.StabilizedPalmPosition.ToString();
                Leap.Vector diffvec = hand.StabilizedPalmPosition;
                Leap.Vector diffvel = hand.PalmVelocity;
                tempValue[5][0][valuenow] = diffvec;
                tempValue[5][1][valuenow] = diffvel;

                Leap.Vector[] tipp = new Leap.Vector[5];
                Leap.Vector[] tipv = new Leap.Vector[5];
                Leap.Vector[,] jointp = new Leap.Vector[5,10];

                for (int i = 0; i < 5; i++)
                {
                    tempValue[i][0][valuenow] = tipp[i] = fingers[i].StabilizedTipPosition;//【】【0】代表指尖位置
                    tempValue[i][1][valuenow] = tipv[i] = fingers[i].TipVelocity;//【】【1】代表指尖速度
                }
                for (int i = 0; i < 5; i++)
                {
                    tempValue[i][2][valuenow] = jointp[i,0] = fingers[i].Bone(Bone.BoneType.TYPE_DISTAL).PrevJoint;//【】【2】代表第一关节
                    tempValue[i][3][valuenow] = jointp[i,1] = fingers[i].Bone(Bone.BoneType.TYPE_INTERMEDIATE).PrevJoint;//【】【3】代表第二关节
                    tempValue[i][4][valuenow] = jointp[i,2] = fingers[i].Bone(Bone.BoneType.TYPE_METACARPAL).PrevJoint;//【】【4】代表第三关节（接近手掌）
                }
                //infoShow.Text = diffvec.ToString();

                if (valuenow == 49)
                {                    

                    for (int i = 0; i < 50; ++i)
                    {
                        float coor = tempValue[FingerToShow][CoordToShow][i].y;
                        dataSourceList[0].Add(new KeyValuePair<int, float>(datanow+i-49, coor));
                    }
                        
                    for (int i = 0; i < 5; ++i)
                        for (int j = 0; j < 5; ++j)
                            tempValue[i][j] = KalmanFilter.RunIt(sourceSig, measuredSig, tempValue[i][j]);
                    tempValue[5][0] = KalmanFilter.RunIt(sourceSig, measuredSig, tempValue[5][0]);
                    tempValue[5][1] = KalmanFilter.RunIt(sourceSig, measuredSig, tempValue[5][1]);

                    for (int i = 0; i < 50; ++i)
                    {
                        float coor = tempValue[FingerToShow][CoordToShow][i].y;
                        dataSourceList[1].Add(new KeyValuePair<int, float>(datanow + i - 49, coor));
                    }
                    LoadLineChartData();
                    StreamWriter writer = new StreamWriter(outputName, true);

                    for (int i = 0; i < 50; ++i)
                    {
                        outputLine = outputLine + time[i] + tempValue[5][0][i].ToString() + tempValue[0][0][i].ToString() + tempValue[1][0][i].ToString() + tempValue[2][0][i].ToString() + tempValue[3][0][i].ToString() + tempValue[4][0][i].ToString();
                        if (recordVelocity)
                            outputLine = outputLine + "V" + tempValue[5][1][i].ToString() + tempValue[0][1][i].ToString() + tempValue[1][1][i].ToString() + tempValue[2][1][i].ToString() + tempValue[3][1][i].ToString() + tempValue[4][1][i].ToString();
                        if (recordJoint)
                            outputLine = outputLine + "J" + tempValue[0][2][i].ToString() + tempValue[0][3][i].ToString() + tempValue[0][4][i].ToString()
                                                          + tempValue[0][3][i].ToString() + tempValue[0][3][i].ToString() + tempValue[0][4][i].ToString()
                                                          + tempValue[0][4][i].ToString() + tempValue[0][3][i].ToString() + tempValue[0][4][i].ToString()
                                                          + tempValue[1][2][i].ToString() + tempValue[0][3][i].ToString() + tempValue[0][4][i].ToString()
                                                          + tempValue[1][3][i].ToString() + tempValue[0][3][i].ToString() + tempValue[0][4][i].ToString()
                                                          + tempValue[1][4][i].ToString() + tempValue[0][3][i].ToString() + tempValue[0][4][i].ToString()
                                                          + tempValue[2][2][i].ToString() + tempValue[0][3][i].ToString() + tempValue[0][4][i].ToString()
                                                          + tempValue[2][3][i].ToString() + tempValue[0][3][i].ToString() + tempValue[0][4][i].ToString()
                                                          + tempValue[2][4][i].ToString() + tempValue[0][3][i].ToString() + tempValue[0][4][i].ToString()
                                                          + tempValue[3][2][i].ToString() + tempValue[0][3][i].ToString() + tempValue[0][4][i].ToString()
                                                          + tempValue[3][3][i].ToString() + tempValue[0][3][i].ToString() + tempValue[0][4][i].ToString()
                                                          + tempValue[3][4][i].ToString() + tempValue[0][3][i].ToString() + tempValue[0][4][i].ToString()
                                                          + tempValue[4][2][i].ToString() + tempValue[0][3][i].ToString() + tempValue[0][4][i].ToString()
                                                          + tempValue[4][3][i].ToString() + tempValue[0][3][i].ToString() + tempValue[0][4][i].ToString()
                                                          + tempValue[4][4][i].ToString() + tempValue[0][3][i].ToString() + tempValue[0][4][i].ToString();

                        writer.WriteLine(outputLine);
                        outputLine = "";
                    }
                    //infoShow.Text = outputLine;
                    writer.Close();
                }



                ++datanow;
                if (datanow == 10000)
                {
                    datanow = 0;
                    dataSourceList[0].Clear();
                    dataSourceList[1].Clear();
                }
                ++valuenow;
                if (valuenow == 50) valuenow = 0;

            }
        }

        //50个数据刷新一次图表
        private void LoadLineChartData()
        {
            LineSeries lineSeries = new LineSeries();
            System.Windows.Data.Binding keyBinding = new System.Windows.Data.Binding();
            keyBinding.Path = new PropertyPath("Key");
            lineSeries.IndependentValueBinding = keyBinding;
            System.Windows.Data.Binding valueBinding = new System.Windows.Data.Binding();
            valueBinding.Path = new PropertyPath("Value");
            lineSeries.DependentValueBinding = valueBinding;
            lineSeries.ItemsSource = dataSourceList[0];
            lineSeries.DataPointStyle = GetNewDataPointStyle(1);
            this.chart1.Series.Clear();
            LineSeries lineSeries1 = new LineSeries();
            lineSeries1.IndependentValueBinding = keyBinding;
            lineSeries1.DependentValueBinding = valueBinding;
            lineSeries1.ItemsSource = dataSourceList[1];
            lineSeries1.DataPointStyle = GetNewDataPointStyle(2);
            //Console.WriteLine(dataSourceList[0].Count);

            this.chart1.Series.Add(lineSeries);
            this.chart1.Series.Add(lineSeries1);

        }

        void MainWindow_Closing(object sender, EventArgs e)
        {
            this.isClosing = true;
            this.controller.RemoveListener(this.listener);
            this.controller.Dispose();
        }

        private void CheckBox_Checked_1(object sender, RoutedEventArgs e)//useless function
        {

        }

        private static Style GetNewDataPointStyle(int type)
        {
        //设定曲线的样式
            Color background = Colors.Red;
            if (type == 2)
                background = Colors.LightGreen;
            Style style = new Style(typeof(DataPoint));
            Setter st1 = new Setter(DataPoint.BackgroundProperty,
                                        new SolidColorBrush(background));
            Setter st2 = new Setter(DataPoint.BorderBrushProperty,
                                        new SolidColorBrush(Colors.White));
            Setter st3 = new Setter(DataPoint.BorderThicknessProperty, new Thickness(0.0001));

            Setter st4 = new Setter(DataPoint.TemplateProperty, null);
           
            style.Setters.Add(st1);
            style.Setters.Add(st2);
            style.Setters.Add(st3);
            style.Setters.Add(st4);
            return style;
        }

        //measure sigma / source sigma 越大，滤波效果越明显
        private void sourceSigma_TextChanged(object sender, TextChangedEventArgs e)//随时调整filter 的参数 
        {
            sourceSig = double.Parse(sourceSigma.Text);
        }

        private void measuredSigma_TextChanged(object sender, TextChangedEventArgs e)
        {
            measuredSig = double.Parse(measuredSigma.Text);
            //Console.Write(measuredSig);
        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            outputName = "out" + Index.Text + ".txt";
        }

        private void whichFinger_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            int lastFinger = FingerToShow;
            FingerToShow = whichFinger.SelectedIndex;
            if (lastFinger != FingerToShow)
            {
                dataSourceList[0].Clear();
                dataSourceList[1].Clear();
            }
            Console.WriteLine(FingerToShow);
        }

        private void whichiCoordinate_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            int lastCoor = CoordToShow;
            CoordToShow = whichiCoordinate.SelectedIndex;
            if (lastCoor != CoordToShow)
            {
                dataSourceList[0].Clear();
                dataSourceList[1].Clear();
            }
        }
    }

    //和leap motion sdk 有关的代码
    ///leap Delegate
    public interface ILeapEventDelegate
    {
        void LeapEventNotification(string EventName);
    }

    public class LeapEventListener : Listener
    {
        ILeapEventDelegate eventDelegate;

        public LeapEventListener(ILeapEventDelegate delegateObject)
        {
            this.eventDelegate = delegateObject;
        }
        public override void OnInit(Controller controller)
        {
            this.eventDelegate.LeapEventNotification("onInit");
        }
        public override void OnConnect(Controller controller)
        {
            controller.SetPolicy(Controller.PolicyFlag.POLICY_OPTIMIZE_HMD);
            //controller.EnableGesture(Gesture.GestureType.TYPE_SWIPE);
            this.eventDelegate.LeapEventNotification("onConnect");
        }

        public override void OnFrame(Controller controller)
        {
            this.eventDelegate.LeapEventNotification("onFrame");
        }
        public override void OnExit(Controller controller)
        {
            this.eventDelegate.LeapEventNotification("onExit");
        }
        public override void OnDisconnect(Controller controller)
        {
            this.eventDelegate.LeapEventNotification("onDisconnect");
        }

    }

}
