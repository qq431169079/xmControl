using System.Net;

namespace CamControl
{
    public static class Program
    {
        public static readonly IPAddress CAM_IP = IPAddress.Parse("192.168.2.118");
        public static readonly int CAM_PORT = 34567;
        public static void Main()
        {
            var xm = new XMCameraConnection(CAM_IP, CAM_PORT, "admin", "gCnGEDW7");
            xm.HeartBeatWorker();
            // while (true)
            // {
            //     var m = xm.PtzControl(XMConstants.PTZ.RIGHT, true);
            //     System.Threading.Thread.Sleep(2000);
            //     xm.PtzControl(CamController.XMConstants.PTZ.LEFT);
            //     System.Threading.Thread.Sleep(2000);
            // }
        }
    }
}
