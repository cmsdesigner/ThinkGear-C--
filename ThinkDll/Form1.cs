/*
@author
 */

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows.Forms;
using ZedGraph;

namespace ThinkDll
{
    public partial class Form1 : Form
    {
        //初始化
        private static bool isInit = false;

        //連接ID
        private static int connectionId = 0;

        //Com Port Name
        private static String comPortName = "\\\\.\\COM4";

        //Err Code
        private static int errCode = 0;

        //信號
        private static double poorSignal, battery;

        /*Chart*/

        // private static double delta = 0, theta = 0, lowAlpha = 0, highAlpha = 0, lowBeta = 0, highBeta = 0, lowGamma = 0, highGamma = 0;
        private static double[] array = new double[8];

        //X軸
        private static int[] x = new int[] { 1, 2, 3, 4, 5, 6, 7, 8 };

        //點清單
        private static PointPairList list = new PointPairList();

        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            //避免不同執行緒呼叫From元件出問題
            Form1.CheckForIllegalCrossThreadCalls = false;

            //Initialize
            init();
        }

        private void button_Stop_Click(object sender, EventArgs e)
        {
            //如果多執行緒正在處理
            if (this.backgroundWorker.IsBusy)
            {
                //尚未初始化
                isInit = false;

                //釋放佔用資源
                Think.TG_FreeConnection(connectionId);

                //取消非同步
                this.backgroundWorker.CancelAsync();
            }
        }

        private void button_Start_Click(object sender, EventArgs e)
        {
            this.backgroundWorker.WorkerSupportsCancellation = true;
            this.backgroundWorker.DoWork += new DoWorkEventHandler(DoWorkEventHandler);
            this.backgroundWorker.RunWorkerAsync(0);
        }

        private void init()
        {
            array = new double[8];
            this.zg1.GraphPane.CurveList.Clear();
            this.zg1.GraphPane.GraphObjList.Clear();
            list = new PointPairList();
            list.Clear();
            BarItem myCurve = this.zg1.GraphPane.AddBar("腦波數據圖", list, Color.Blue);  //建立長條圖
            this.zg1.GraphPane.Title.IsVisible = false; //主標題是否 顯示
            this.zg1.GraphPane.Title.FontSpec.FontColor = Color.Green;
            this.zg1.GraphPane.XAxis.Title.IsVisible = false;

            //X Label
            String[] xLabel = new String[] { "Delta", "Theta", "Low Alpha", "High Alpha", "Low Beta", "High Beta", "Low Gamma", "High Gamma" };

            //設定X Label
            this.zg1.GraphPane.XAxis.Scale.TextLabels = xLabel;

            //設定X種類
            this.zg1.GraphPane.XAxis.Type = AxisType.Text;

            //設定X Label大小
            this.zg1.GraphPane.XAxis.Scale.FontSpec.Size = 8;

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

                    if (Think.TG_GetValueStatus(connectionId, Think.DATA_BATTERY) != 0)
                    {
                        battery = Think.TG_GetValue(connectionId,
                                Think.DATA_BATTERY);
                        setBarBattery(battery);
                    }

                    //清除原先清單資料
                    list.Clear();
                    for (int i = 0; i < 8; i++)
                    {
                        list.Add(x[i], array[i]);
                    }

                    //改變目前數值
                    this.zg1.AxisChange();

                    //重新繪圖
                    this.zg1.RestoreScale(this.zg1.GraphPane);
                }
            }
        }

        private void DoWorkEventHandler(object sender, DoWorkEventArgs e)
        {
            this.backgroundWorker.WorkerReportsProgress = true;
            this.backgroundWorker.ProgressChanged += new ProgressChangedEventHandler(ProgressChangedEventHandler);
            this.backgroundWorker.RunWorkerCompleted += new RunWorkerCompletedEventHandler(RunWorkerCompletedEventHandler);
            DoAdd((BackgroundWorker)sender, e);
        }

        private void ProgressChangedEventHandler(object sender, ProgressChangedEventArgs e)
        {
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
    }
}