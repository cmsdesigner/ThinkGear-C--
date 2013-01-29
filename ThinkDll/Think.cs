using System;
using System.Runtime.InteropServices;

namespace ThinkDll
{
    public class Think
    {
        public static int BAUD_1200 = 1200;
        public static int BAUD_2400 = 2400;
        public static int BAUD_4800 = 4800;
        public static int BAUD_9600 = 9600;
        public static int BAUD_57600 = 57600;
        public static int BAUD_115200 = 115200;

        public static int STREAM_PACKETS = 0;
        public static int STREAM_5VRAW = 1;
        public static int STREAM_FILE_PACKETS = 2;

        public static int DATA_BATTERY = 0;
        public static int DATA_POOR_SIGNAL = 1;
        public static int DATA_ATTENTION = 2;
        public static int DATA_MEDITATION = 3;
        public static int DATA_RAW = 4;
        public static int DATA_DELTA = 5;
        public static int DATA_THETA = 6;
        public static int DATA_ALPHA1 = 7;
        public static int DATA_ALPHA2 = 8;
        public static int DATA_BETA1 = 9;
        public static int DATA_BETA2 = 10;
        public static int DATA_GAMMA1 = 11;
        public static int DATA_GAMMA2 = 12;

        [DllImport("thinkgear.dll")]
        public static extern int TG_GetDriverVersion();

        [DllImport("thinkgear.dll")]
        public static extern int TG_GetNewConnectionId();

        [DllImport("thinkgear.dll")]
        public static extern int TG_SetStreamLog(int connectionId, String filename);

        [DllImport("thinkgear.dll")]
        public static extern int TG_SetDataLog(int connectionId, String filename);

        [DllImport("thinkgear.dll")]
        public static extern int TG_Connect(int connectionId, String serialPortName, int serialBaudrate, int serialDataFormat);

        [DllImport("thinkgear.dll")]
        public static extern int TG_ReadPackets(int connectionId, int numPackets);

        [DllImport("thinkgear.dll")]
        public static extern double TG_GetValue(int connectionId, int dataType);

        [DllImport("thinkgear.dll")]
        public static extern int TG_GetValueStatus(int connectionId, int dataType);

        [DllImport("thinkgear.dll")]
        public static extern int TG_SendByte(int connectionId, int b);

        [DllImport("thinkgear.dll")]
        public static extern int TG_SetBaudrate(int connectionId, int serialBaudrate);

        [DllImport("thinkgear.dll")]
        public static extern int TG_SetDataFormat(int connectionId, int serialDataFormat);

        [DllImport("thinkgear.dll")]
        public static extern void TG_Disconnect(int connectionId);

        [DllImport("thinkgear.dll")]
        public static extern void TG_FreeConnection(int connectionId);
    }
}