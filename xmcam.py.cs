using Newtonsoft.Json;

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;

using static CamControl.Utils;

namespace CamControl
{
    public class XMCameraConnection
    {
        private const string RTSP_STREAM_DEFAULT = "rtsp://192.168.2.118/user=admin&password=gCnGEDW7&channel=1&stream=0.sdp";
        public XMCameraConnection instance;
        public Socket mainSocket;
        public int socketTimeout;
        public uint camSid;
        public uint sequence;
        public IPAddress camIp;
        public int camPort;
        public string cameraUser;
        public string camPassword;
        public Thread keepAliveWorker;
        public bool IsSubConnection => instance != null;
        public XMCameraConnection(IPAddress ip, int port, string username, string password, XMCameraConnection instance = null, uint sid = 0)
        {
            camIp = ip;
            camPort = port;
            cameraUser = username;
            camPassword = password;
            this.instance = instance;
            camSid = sid;

            try
            {
                mainSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp)
                {
                    ReceiveTimeout = socketTimeout,
                    SendTimeout = socketTimeout
                };
                mainSocket.Connect(camIp, camPort);
                var pkt = new Dictionary<string, object>
                {
                    { "EncryptType", "MD5" },
                    { "UserName", cameraUser },
                    { "PassWord", camPassword },
                    { "LoginType", "DVRIP-Web" }
                };
                var response = _getResponse(XMConstants.User.LOGIN_REQ2, pkt);
                var respdict = JsonConvert.DeserializeObject<Dictionary<string, object>>(response);
                Console.WriteLine(response);
                if (!IsSubConnection && respdict != null && respdict.ContainsKey("Ret") && (long)respdict["Ret"] == 100)
                {
                    StartHeartBeat();
                }
                else
                {
                    Console.WriteLine("XMCameraConnection: Cannot start keepalive");
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }

        public string PtzControl(int direction, bool stop = false)
        {
            var aux = new Dictionary<string, object>
            {
                { "Number", 0 },
                { "Status", "On" }
            };
            var point = new Dictionary<string, object>
            {
                { "bottom", 0 },
                { "left", 0 },
                { "right", 0 },
                { "top", 0 }
            };
            var parameter = new Dictionary<string, object>
            {
                { "AUX", aux },
                { "Channel", 0 },
                { "MenuOpts", "Enter" },
                { "POINT", point },
                { "Pattern", "Start" },
                { "Preset", stop ? -1 : 65535 },
                { "Step", 30 },
                { "Tour", 0 }
            };
            var opptzcontrol = new Dictionary<string, object>
            {
                { "Command", direction },
                { "Parameter", parameter }
            };

            var pkt = new Dictionary<string, object>
            {
                { "Name", "OPPTZControl" },
                { "OPPTZControl", opptzcontrol }
            };

            var response = instance._getResponse(XMConstants.PlayBack.PTZ_REQ, pkt);
            return Prettify(response);
        }

        ~XMCameraConnection()
        {
            mainSocket.Disconnect(true);
            StopHeartBeat();
        }



        public Dictionary<string, object> SendCommand(int msgid, object _param)
        {
            if (_param is Dictionary<string, object>)
            {
                if (msgid != XMConstants.User.LOGIN_REQ2)
                {
                    (_param as Dictionary<string, object>)["SessionID"] = camSid.ToString();
                }
                _param = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(_param as Dictionary<string, object>));
            }
            byte[] _data = _param as byte[];
            byte[] cmd_data = StructConverter.Pack((byte)0xff, (byte)0, (byte)0, (byte)0, (uint)camSid, (uint)sequence, (byte)0, (byte)0, (ushort)msgid, (uint)_data.Length + (uint)2, _data, (byte)0x0a, (byte)0x00);
            mainSocket.Send(cmd_data);

            byte[] rcvdBytes = new byte[20];
            mainSocket.Receive(rcvdBytes, 0, 20, SocketFlags.None);
            var result = StructConverter.Unpack("BBxxIIxxHI", rcvdBytes);
            var response_head = new Dictionary<string, object>
            {
                { "Version", (byte)result[1] },
                { "SessionID", (uint)result[2] },
                { "Sequence", (uint)result[3] },
                { "MessageId", (ushort)result[4] },
                { "Content_Length", (uint)result[5] }
            };

            sequence = (uint)response_head["Sequence"];
            return response_head;
        }

        public string _getResponse(int msgid, Dictionary<string, object> _params)
        {
            var response_head = SendCommand(msgid, _params) as Dictionary<string, object>;
            var @out = _GetResponse(response_head);
            if (msgid == XMConstants.User.LOGIN_REQ2 && response_head.ContainsKey("SessionID"))
            {
                camSid = (uint)response_head["SessionID"];
            }
            return @out;
        }

        public bool _download(int msgid, object _params, string file)
        {
            var reply_head = SendCommand(msgid, _params);
            var @out = _GetResponse(reply_head as Dictionary<string, object>);
            File.WriteAllText(file, @out);
            return true;

        }

        public string _GetResponse(Dictionary<string, object> reply_head)
        {
            var reply = reply_head;
            var length = (uint)reply["Content_Length"];
            var @out = "";
            byte[] data = new byte[length];
            _ = mainSocket.Receive(data, 0, Convert.ToInt32(length), SocketFlags.None);
            @out += Encoding.UTF8.GetString(data);
            return @out;
        }

        public void StartHeartBeat()
        {
            keepAliveWorker = new Thread(HeartBeatWorker);
            keepAliveWorker.Start();
        }

        public void StopHeartBeat()
        {
            if (keepAliveWorker != null)
            {
                keepAliveWorker.Abort();
            }
        }


        public void HeartBeatWorker()
        {
            while (true)
            {
                lock (mainSocket)
                {
                    var pkt = new Dictionary<string, object>
                    {
                        { "Name", "KeepAlive" }
                    };
                    var response = _getResponse(XMConstants.KEEPALIVE_REQ, pkt);
                    Console.WriteLine(DateTime.Now);
                    Console.WriteLine(response);
                }
                Thread.Sleep(10 * 1000);
            }
        }

        public XMCameraConnection CreateSubConnection()
        {
            var subconn = new XMCameraConnection(camIp, camPort, cameraUser, camPassword, sid: camSid, instance: this);
            return subconn;
        }

        public object GetSystemFunctions()
        {
            var pkt = new Dictionary<string, object>
            {
                { "Name", "SystemFunction" }
            };
            var response = _getResponse(XMConstants.System.ABILITY_GET, pkt);
            return Prettify(response);
        }

        public object GetSystemInfo()
        {
            var pkt = new Dictionary<string, object>
            {
                { "Name", "SystemInfo" }
            };
            var response = _getResponse(XMConstants.System.SYSINFO_REQ, pkt);
            return Prettify(response);
        }

        public object KeepAlive()
        {
            var pkt = new Dictionary<string, object>
            {
                { "Name","KeepAlive" }
            };
            return _getResponse(XMConstants.KEEPALIVE_REQ, pkt);
        }

        public object GetChannelTitle()
        {
            var pkt = new Dictionary<string, object>
            {
                { "Name", "ChannelTitle" }
            };
            var response = _getResponse(XMConstants.System.CONFIG_CHANNELTILE_GET, pkt);
            return Prettify(response);
        }

        public object GetOEMInfo()
        {
            var pkt = new Dictionary<string, object>
            {
                { "Name", "OEMInfo" }
            };
            var response = _getResponse(XMConstants.System.SYSINFO_REQ, pkt);
            return Prettify(response);
        }

        public object GetStorageInfo()
        {
            var pkt = new Dictionary<string, object>
            {
                { "Name", "StorageInfo" }
            };
            var response = _getResponse(XMConstants.System.SYSINFO_REQ, pkt);
            return Prettify(response);
        }

        public object SyncTime(bool noRTC = false)
        {
            var cmd = "OPTimeSetting";
            var pkt_type = XMConstants.SYSMANAGER_REQ;
            if (noRTC)
            {
                cmd += "NoRTC";
                pkt_type = XMConstants.SYNC_TIME_REQ;
            }
            var pkt = new Dictionary<string, object>
            {
                { "Name", cmd },
                { cmd, string.Format("{0}-{1}-{2} {3}:{4}5{5}",DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day, DateTime.Now.Hour, DateTime.Now.Minute, DateTime.Now.Second) }
            };
            var response = _getResponse(pkt_type, pkt);
            return response;
        }

        public object GetRemoteTime()
        {
            var pkt = new Dictionary<string, object>
            {
                { "Name", "OPTimeQuery" }
            };
            var response = _getResponse(XMConstants.TIMEQUERY_REQ, pkt);
            return response;
        }

        public object GetUsers()
        {
            var pkt = new Dictionary<string, object> { };
            var response = _getResponse(XMConstants.User.USERS_GET, pkt);
            return Prettify(response);
        }


        public object DownloadPhoto(string file)
        {
            var pkt = new Dictionary<string, object> { };
            var reply = _download(XMConstants.PHOTO_GET_REQ, pkt, file);
            return reply;
        }

        public bool ExportConfig(string file)
        {
            var pkt = new Dictionary<string, object>
            {
                { "Name", "" }
            };
            var reply = _download(XMConstants.CONFIG_EXPORT_REQ, pkt, file);
            return reply;
        }

        // Just because no snap command supported, we need external program to capture from RTSP stream
        // using avconv or ffmpeg
        public static bool ExternalSnapshot(object snap_file, string app = "/usr/bin/avconv", string rtsp = "rtsp://192.168.1.10/user=admin&password=admin&channel=1&stream=0.sdp", List<string> args = null)
        {
            if (args == null)
            {
                args = new List<string> { "-y", "-f", "image2", "-vframes", "1", "-pix_fmt", "yuvj420p" };
            }
            if (!File.Exists(app))
            {
                return false;
            }
            // Add executable
            var fullargs = new StringBuilder();
            // Make silent except errors
            fullargs.Append(" -loglevel");
            fullargs.Append(" panic");
            // Append input arg
            fullargs.Append(" -i");
            fullargs.Append(" " + rtsp);
            // Append other args
            (from a in args select fullargs.Append(" " + a)).ToList();
            // Lastly, append output arg
            fullargs.Append(" " + snap_file);
            // child = subprocess.Popen(process, stdout=subprocess.PIPE)
            var child = new Process { StartInfo = new ProcessStartInfo(app, fullargs.ToString()) };
            child.Start();
            child.WaitForExit();
            return child.ExitCode == 0;
        }

        public static object ExternalRecord(string video_file, string exe, string rtsp = RTSP_STREAM_DEFAULT, string[] args = null, int time_limit = 5)
        {
            if (args == null)
            {
                args = new string[] { "-vcodec", "copy", "-f", "mp4", "-y", "-an" };
            }
            if (!File.Exists(exe))
            {
                return false;
            }
            // Add executable
            var fullargs = new StringBuilder();

            // Make silent except errors
            fullargs.Append(" -loglevel");
            fullargs.Append(" panic");
            // Append input arg
            fullargs.Append(" -i");
            fullargs.Append(" " + rtsp);
            // Append other args
            (from a in args
             select fullargs.Append(a)).ToList();
            // Append record time limit in secs
            fullargs.Append(" -t ");
            fullargs.Append(time_limit > 0 ? time_limit.ToString() : "5");
            // Append output arg
            fullargs.Append(video_file);
            // child = subprocess.Popen(process, stdout=subprocess.PIPE)
            var child = new Process();
            child.StartInfo = new ProcessStartInfo(exe, fullargs.ToString());
            child.Start();
            child.WaitForExit();
            return child.ExitCode == 0;
        }

        public string TalkClaim()
        {
            Debug.Assert(IsSubConnection, "cmd_talk_claim need run on a sub connection");
            var audioformat = new Dictionary<string, object>
            {
                { "BitRate", 0 },
                { "EncodeType", "G711_ALAW" },
                { "SampleBit", 8 },
                { "SampleRate", 8 }
            };
            var optalk = new Dictionary<string, object>
            {
                { "Action", "Claim" },
                { "AudioFormat", audioformat }
            };

            var pkt = new Dictionary<string, object>
            {
                { "Name", "OPTalk" },
                { "OPTalk", optalk }
            };
            var response = _getResponse(XMConstants.PlayBack.TALK_CLAIM, pkt);
            return response;
        }

        public Dictionary<string, object> SendTalkVoice(byte[] data)
        {
            //assert type(data) == bytes, 'Data should be a PCM bytes type'
            // final_data = bytes.fromhex('000001fa0e024001') + data
            List<byte> final_data = new List<byte> { 0x00, 0x00, 0x01, 0xfa, 0x0e, 0x02, 0x40, 0x01 };
            final_data.AddRange(data);
            var sent = SendCommand(XMConstants.PlayBack.TALK_CU_PU_DATA, final_data.ToArray());
            return sent;
        }

        public string StartTalk()
        {
            var audioformat = new Dictionary<string, object>
            {
                { "BitRate", 128 },
                { "EncodeType", "G711_ALAW" },
                { "SampleBit", 8 },
                { "SampleRate", 8000 }
            };
            var optalk = new Dictionary<string, object>
            {
                { "Action", "Start" },
                { "AudioFormat", audioformat }
            };
            var pkt = new Dictionary<string, object>
            {
                { "Name", "OPTalk" },
                { "OPTalk", optalk }
            };
            var response = _getResponse(XMConstants.PlayBack.TALK_REQ, pkt);
            return response;
        }

        public string StopTalk()
        {
            Dictionary<string, object> audioformat = new Dictionary<string, object>
            {
                { "BitRate", 128 },
                { "EncodeType", "G711_ALAW" },
                { "SampleBit", 8 },
                { "SampleRate", 8000 }
            };
            Dictionary<string, object> optalk = new Dictionary<string, object>
            {
                { "Action", "Stop" },
                { "AudioFormat", audioformat }
            };
            var pkt = new Dictionary<string, object>
            {
                { "Name", "OPTalk" },
                { "OPTalk", optalk }
            };
            var response = _getResponse(XMConstants.PlayBack.TALK_REQ, pkt);
            return response;
        }

        public static object TalkConvert(string src, double volume = 1.0, string app = "/usr/bin/avconv", List<string> args = null)
        {
            if (args == null)
            {
                args = new List<string>() { "-y", "-f", "alaw", "-ar", "8000", "-ac", "1" };
            }
            if (!File.Exists(app))
            {
                return false;
            }
            if (!File.Exists(src))
            {
                return false;
            }
            var dst_final = src + ".pcm";
            var fullargs = new StringBuilder();
            fullargs.Append(" -loglevel");
            fullargs.Append(" panic");
            fullargs.Append(" -i");
            fullargs.Append(src);
            (from a in args select fullargs.Append(" " + a)).ToList();
            if (volume != 1.0)
            {
                fullargs.Append("-filter:a");
                fullargs.Append(string.Format("volume={0}", volume));
            }
            fullargs.Append(dst_final);
            Process proc = Process.Start(new ProcessStartInfo(app, fullargs.ToString()));
            proc.Start();
            proc.WaitForExit();
            return proc.ExitCode == 0;
        }

        public static byte[] GetTalkTrunks(string pcmfile)
        {
            byte[] retdata = null;
            try
            {
                var voiceStream = File.OpenRead(pcmfile);
                Span<byte> buffer = new Span<byte>();
                int count = voiceStream.Read(buffer);
                retdata = buffer.ToArray();
            }
            catch
            {
                Console.WriteLine("Got an exception on talk_get_chunks");
            }
            return retdata;
        }
    }
}