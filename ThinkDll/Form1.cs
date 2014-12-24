/*
@author C.Y. Fang
 */

using System;
using System.Windows;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;
using ZedGraph;
using MySql.Data.MySqlClient;
using System.IO.Ports;
using System.Data;
using System.Threading;
using System.Linq;

namespace ThinkDll
{
    public partial class DrawPlut : Form
    {
        /// <summary>
        /// 初始化
        /// </summary>
        private static bool isInit = false;

        /// <summary>
        /// 連接ID
        /// </summary>
        private static int connectionId = 0;

        /// <summary>
        /// Com Port Name
        /// </summary>
        private String comPortName
        {
            get { return "\\\\.\\" + comboBox1.SelectedItem.ToString(); }
        }



        /// <summary>
        /// Err Code
        /// </summary>
        private static int errCode = 0;

        /// <summary>
        /// 信號
        /// </summary>
        private static double poorSignal, battery;

        /*Chart*/

        // private static double delta = 0, theta = 0, lowAlpha = 0, highAlpha = 0, lowBeta = 0, highBeta = 0, lowGamma = 0, highGamma = 0, Attention=0, Meditation=0 
        private static double[] array = new double[10];

        /// <summary>
        /// X軸
        /// </summary>

        private static int[] x = new int[] { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10 };
        /// <summary>
        /// 使用者名稱
        /// </summary>
        private string name { get { return textBoxName.Text; } }

        /// <summary>
        /// 點清單
        /// </summary>
        private static PointPairList list = new PointPairList();
        
        /// <summary>
        /// DB Class
        /// </summary>
        private static MyDB db = new MyDB();

        private             //X Label
            String[] xLabel = new String[] { "Delta", "Theta", "Low Alpha", "High Alpha", 
                "Low Beta", "High Beta", "Low Gamma", "High Gamma", "Attention","Meditation" };

        private int[] points; 

        public DrawPlut()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            //避免不同執行緒呼叫From元件出問題
            DrawPlut.CheckForIllegalCrossThreadCalls = false;

            //Initialize
            SetComPort();
            Init();
        }

        private void button_Stop_Click(object sender, EventArgs e)
        {
            //如果多執行緒正在處理
            if (this.backgroundWorker.IsBusy)
            {
                db.Close();

                checkedListBox1.Enabled = true;
                
                //尚未初始化
                isInit = false;

                //釋放佔用資源
                Think.TG_FreeConnection(connectionId);

                //取消非同步
                this.backgroundWorker.CancelAsync();
                
            }
        }

        private int [] GetCBSelect()
        {
            int sum = 0;

            for (int i = 0; i < checkedListBox1.Items.Count; i++)
                if (checkedListBox1.GetItemChecked(i))
                    sum++;

            if (sum == 0)
                return new int[1];

            int [] temp = new int[sum];
            sum = 0;
            for (int i = 0; i < checkedListBox1.Items.Count; i++)
                if (checkedListBox1.GetItemChecked(i))
                {
                    temp[sum] = i;
                    sum++;
                }

            return temp;
        }

        private void button_Start_Click(object sender, EventArgs e)
        {
            points = GetCBSelect();
            string[] label = new string[points.Length];


            for (int i = 0; i < points.Length; i++)
                label[i] = xLabel[points[i]];

            //設定X Label
            this.zg1.GraphPane.XAxis.Scale.TextLabels = label;
            //改變圖片
            this.zg1.AxisChange();
            //設定X種類
            this.zg1.GraphPane.XAxis.Type = AxisType.Text;
            //設定X Label大小
            this.zg1.GraphPane.XAxis.Scale.FontSpec.Size = 8;

            db.Open();
            checkedListBox1.Enabled = false;
            this.backgroundWorker.WorkerSupportsCancellation = true;
            this.backgroundWorker.DoWork += new DoWorkEventHandler(DoWorkEventHandler);
            this.backgroundWorker.RunWorkerAsync(0);
        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            db.Close();
        }

        /// <summary>
        /// get com port added for the combobox list
        /// </summary>
        private void SetComPort()
        {

            foreach (string com in SerialPort.GetPortNames())
            {
                comboBox1.Items.Add(com);
            }

            comboBox1.SelectedIndex = 0;
        }

        private void Init()
        {
            array = new double[10];


            this.zg1.GraphPane.CurveList.Clear();
            this.zg1.GraphPane.GraphObjList.Clear();
            list = new PointPairList();
            list.Clear();
            BarItem myCurve = this.zg1.GraphPane.AddBar("腦波數據圖", list, Color.Blue);  //建立長條圖
            this.zg1.GraphPane.Title.IsVisible = false; //主標題是否 顯示
            this.zg1.GraphPane.Title.FontSpec.FontColor = Color.Green;
            this.zg1.GraphPane.XAxis.Title.IsVisible = false;

            //設定Y Label大小
            this.zg1.GraphPane.YAxis.Scale.FontSpec.Size = 5;

            //設定Y Color
            this.zg1.GraphPane.YAxis.MajorGrid.Color = Color.Black;

            //Chart 填色
            this.zg1.GraphPane.Chart.Fill = new Fill(Color.White, Color.LightGoldenrodYellow, -45F);

            //外部 填色
            this.zg1.GraphPane.Fill = new Fill(Color.White, Color.FromArgb(220, 220, 255), -45F);
            this.zg1.GraphPane.Legend.Position = ZedGraph.LegendPos.InsideTopRight;
            this.zg1.GraphPane.Legend.FontSpec.Size = 20;

            //設定Y軸數值大小
            this.zg1.GraphPane.YAxis.Scale.Max = 999999;

            //改變圖片
            this.zg1.AxisChange();
        }

        private void DoAdd(BackgroundWorker worker, DoWorkEventArgs e)
        {
            if (!isInit)
            {
                //取得通訊ID
                connectionId = Think.TG_GetNewConnectionId();
                if (connectionId < 0)
                {
                    return;
                }

                /* Set/open stream (raw bytes) log file for connection */
                errCode = Think.TG_SetStreamLog(connectionId, "streamLog.txt");
                if (errCode < 0)
                {
                    return;
                }

                /* Set/open data (Think values) log file for connection */
                errCode = Think.TG_SetDataLog(connectionId, "dataLog.txt");
                if (errCode < 0)
                {
                    return;
                }

                errCode = Think.TG_Connect(connectionId, comPortName,
                        Think.BAUD_57600, Think.STREAM_PACKETS);
                if (errCode < 0)
                {
                    return;
                }

                //
                isInit = true;
            }

            while (true)
            {
                /* Attempt to read a Packet of data from the connection */
                errCode = Think.TG_ReadPackets(connectionId, 1);

                /* If TG_ReadPackets() was able to read a complete Packet of data... */
                if (errCode == 1)
                {
                    if (Think.TG_GetValueStatus(connectionId,
                            Think.DATA_POOR_SIGNAL) != 0)
                    {
                        poorSignal = Think.TG_GetValue(connectionId,
                                Think.DATA_POOR_SIGNAL);
                    }

                    if (Think
                            .TG_GetValueStatus(connectionId, Think.DATA_DELTA) != 0)
                    {
                        array[0] = Think.TG_GetValue(connectionId,
                                Think.DATA_DELTA);
                    }


                    if (Think
                            .TG_GetValueStatus(connectionId, Think.DATA_THETA) != 0)
                    {
                        array[1] = Think.TG_GetValue(connectionId,
                                Think.DATA_THETA);
                    }

                    if (Think.TG_GetValueStatus(connectionId,
                            Think.DATA_ALPHA1) != 0)
                    {
                        array[2] = Think.TG_GetValue(connectionId,
                                Think.DATA_ALPHA1);
                    }

                    if (Think.TG_GetValueStatus(connectionId,
                            Think.DATA_ALPHA2) != 0)
                    {
                        array[3] = Think.TG_GetValue(connectionId,
                                Think.DATA_ALPHA2);
                    }

                    if (Think
                            .TG_GetValueStatus(connectionId, Think.DATA_BETA1) != 0)
                    {
                        array[4] = Think.TG_GetValue(connectionId,
                                Think.DATA_BETA1);
                    }

                    if (Think
                            .TG_GetValueStatus(connectionId, Think.DATA_BETA2) != 0)
                    {
                        array[5] = Think.TG_GetValue(connectionId,
                                Think.DATA_BETA2);
                    }

                    if (Think.TG_GetValueStatus(connectionId,
                            Think.DATA_GAMMA1) != 0)
                    {
                        array[6] = Think.TG_GetValue(connectionId,
                                Think.DATA_GAMMA1);
                    }

                    if (Think.TG_GetValueStatus(connectionId,
                            Think.DATA_GAMMA2) != 0)
                    {
                        array[7] = Think.TG_GetValue(connectionId,
                                Think.DATA_GAMMA2);
                    }

                    if (Think.TG_GetValueStatus(connectionId, Think.DATA_GAMMA2) != 0)
                    {
                        array[7] = Think.TG_GetValue(connectionId,
                                Think.DATA_GAMMA2);
                    }
                    if (Think.TG_GetValueStatus(connectionId, Think.DATA_ATTENTION)!=0)
                    {
                        array[8] = Think.TG_GetValue(connectionId,
                                Think.DATA_ATTENTION);
                    }

                    if (Think.TG_GetValueStatus(connectionId, Think.DATA_MEDITATION) != 0)
                    {
                        array[9] = Think.TG_GetValue(connectionId,
      Think.DATA_MEDITATION);
                    }



                    if (Think.TG_GetValueStatus(connectionId, Think.DATA_BATTERY) != 0)
                    {
                        battery = Think.TG_GetValue(connectionId,
                                Think.DATA_BATTERY);
                        setBarBattery(battery);
                    }

                    string temp = string.Format(MyDB.sql, name, DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"), 
                            array[0].ToString(), array[1].ToString(), array[2].ToString(),
                        array[3].ToString(), array[4].ToString(), array[5].ToString(), array[6].ToString(),
                        array[7].ToString(), array[8].ToString(), array[9].ToString());
                    db.Insert(temp);

                    //清除原先清單資料
                    list.Clear();

                    for (int i = 0; i < points.Length; i++)
                    {
                        list.Add((i + 1), array[i]);
                    }

                    //改變目前數值
                    this.zg1.AxisChange();

                    //重新繪圖
                    this.zg1.RestoreScale(this.zg1.GraphPane);
                }
            }

            db.Close();
        }

        private void DoWorkEventHandler(object sender, DoWorkEventArgs e)
        {
            this.backgroundWorker.WorkerReportsProgress = true;
            this.backgroundWorker.RunWorkerCompleted += new RunWorkerCompletedEventHandler(RunWorkerCompletedEventHandler);
            DoAdd((BackgroundWorker)sender, e);
        }



        private void RunWorkerCompletedEventHandler(object sender, RunWorkerCompletedEventArgs e)
        {
            if (e.Cancelled)
            {
                MessageBox.Show("作業尚未完成");
            }
            else
            {
                MessageBox.Show("作業完成");
            }
        }

        private void setBarBattery(Double data)
        {
            //設定電池電量
            this.progressBarBattery.Value = (int)data;
        }

        protected class MyDB
        {
            /// <summary>
            /// MySQL Connection
            /// </summary>
            private MySqlConnection connection;

            private Thread thread;

            /// <summary>
            /// Connection String
            /// </summary>
            private String connStr = "server=127.0.0.1;user=root;database=thinkgear;port=3306;password=123456789;charset=utf8;";
            
            /// <summary>
            /// Insert sql
            /// </summary>
            public const String sql = @"INSERT INTO `data` (`Name`, `Time`, `Delta`, `Theta`, `Low Alpha`, `High Alpha`, `Low Beta`, `High Beta`, `Low Gamma`, `High Gamma`, `Attention`, `Meditation`) VALUES ({0}, {1}, {2}, {3}, {4}, {5}, {6}, {7}, {8}, {9}, {10}, {11});";
            
            /// <summary>
            /// Insert Type
            /// </summary>
            private  MySqlDbType[] parsTypes = new MySqlDbType[] {MySqlDbType.Text, MySqlDbType.DateTime, MySqlDbType.Int32, MySqlDbType.Int32, MySqlDbType.Int32, MySqlDbType.Int32, MySqlDbType.Int32 
            , MySqlDbType.Int32, MySqlDbType.Int32, MySqlDbType.Int32, MySqlDbType.Int32, MySqlDbType.Int32};
            
            /// <summary>
            /// Alias
            /// </summary>
            private String[] alias = new String[] {"?Name","?Time", "?Delta", "?Theta", "?LowAlpha", "?HighAlpha", 
                "?LowBeta", "?HighBeta", "?LowGamma", "?HighGamma", "?Attention", "?Meditation"};
            
            /// <summary>
            /// MySQL Command
            /// </summary>
            private MySqlCommand cmd;

            /// <summary>
            /// Constructor
            /// </summary>
            public MyDB()
            {
                this.connection = new MySqlConnection(connStr);
            }

            /// <summary>
            /// Open MySQL
            /// </summary>
            public void Open()
            {
                if (connection.ConnectionString != null
                   && connection.ConnectionString != string.Empty
                    && connection.State != ConnectionState.Executing)
                {
                    this.connection.Open();
                    this.cmd = NewCommand();
                }
            }

            /// <summary>
            /// Close MySQL
            /// </summary>
            public void Close()
            {
                if (cmd != null)
                    this.cmd.Cancel();
                this.connection.Close();
            }

            /// <summary>
            /// Insert data for the database
            /// database name is thinkgear
            /// </summary>
            /// <param name="args">data</param>
            public void Insert(string sql)
            {
                cmd = NewCommand();
                thread = new Thread(new ThreadStart(InsertSql));
                thread.Start();
            }

            public void InsertSql()
            {
                cmd.ExecuteNonQuery();
            }

            /// <summary>
            /// Get new MySqlCommand
            /// </summary>
            /// <returns>MySqlCommand</returns>
            protected MySqlCommand NewCommand()
            {
                return new MySqlCommand(sql, connection);
            }

        }

    }
}