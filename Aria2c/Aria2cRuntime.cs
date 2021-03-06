﻿
namespace FlyVR.Aria2
{
    using System;
    using System.Diagnostics;
    using System.IO;
    using System.Text;

    public sealed class Aria2cRuntime
    {
        #region 私有变量
        private static int aria2cPort = 6800;
        private static string aria2cHost = "localhost";
        private static string downLoadDirectory;
        private static Process aria2cProcess;
        #endregion

        #region 属性
        /// <summary>
        /// Aria2c IP地址
        /// </summary>
        public static string Aria2cHost
        {
            get
            {
                return aria2cHost;
            }
            set
            {
                aria2cHost = value;
                SetAria2cHost(Aria2cHost, Aria2cPort);
            }
        }

        /// <summary>
        /// Aria2c端口
        /// </summary>
        public static int Aria2cPort
        {
            get
            {
                return aria2cPort;
            }
            set
            {
                aria2cPort = value;
                SetAria2cHost(Aria2cHost, aria2cPort);
            }
        }

        /// <summary>
        /// 是否正在运行
        /// </summary>
        public static bool IsLoaded
        {
            get
            {
                try
                {
                    string version = Aria2cWarpper.GetVersion();
                    return true;
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex);
                    return false;
                }
            }
        }


        /// <summary>
        /// 全局下载目录
        /// </summary>
        public static string DownLoadDirectory
        {
            get
            {
                try
                {
                    downLoadDirectory = Aria2cWarpper.DownLoadDirectory;
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex);
                }
                return downLoadDirectory;
            }
            set
            {
                downLoadDirectory = value;
                try
                {
                    Aria2cWarpper.DownLoadDirectory = downLoadDirectory;
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex);
                }
            }
        }

        #endregion

        #region 公共方法
        private static Aria2cSettings _Aria2cSettings;
        public static Aria2cSettings Settings
        {
            get
            {
                return _Aria2cSettings ?? new Aria2cSettings();
            }
            set
            {
                _Aria2cSettings = value;
                SetAria2cHost(value.Aria2Host, value.Aria2Port);
            }
        }

        /// <summary>
        /// 启动服务进程
        /// </summary>
        /// <returns></returns>
        public static void Start(Aria2cSettings settings)
        {
            Settings = settings;
            Start();
        }

        /// <summary>
        /// 启动服务进程
        /// </summary>
        /// <returns></returns>
        public static void Start()
        {
            if (/*!IsLoaded && 外界已经做了检查所以不再检测一遍提升启动速度。*/File.Exists(Settings.Aria2Path))
            {
                StartProcess(Settings.Aria2Path, Settings.Aria2Args);
            }
        }

        /// <summary>
        /// 关闭服务进程
        /// </summary>
        public static void ShutDown()
        {
            if (aria2cProcess == null)
                return;
            if (!aria2cProcess.HasExited)
            {
                aria2cProcess.Kill();
                aria2cProcess.Dispose();
            }
            aria2cProcess = null;
        }
        #endregion

        #region 实现
        /// <summary>
        /// 设置host地址
        /// </summary>
        /// <param name="host"></param>
        /// <param name="port"></param>
        private static void SetAria2cHost(string host, int port)
        {
            StringBuilder builder = new StringBuilder();
            builder.Append("http://");
            builder.Append(host);
            builder.Append(":");
            builder.Append(port);
            builder.Append("/rpc");

            Aria2cWarpper.Aria2cHost = builder.ToString();
        }


        private static void StartProcess(string path, string args = "")
        {
            try
            {
                var aria2Dir = Aria2cTools.GetDirectoryName(path);
                var aria2cName = Aria2cTools.GetFileNameWithoutExtension(path);
                string config = aria2cName + ".conf";

                if (string.IsNullOrWhiteSpace(args))
                {
                    if (File.Exists(Path.Combine(aria2Dir, config)))
                    {
                        args = " --conf-path=" + config;
                    }
                    else
                    {
                        args = " --enable-rpc --rpc-listen-all=true --rpc-allow-origin-all --rpc-listen-port=6800 -c -D";
                    }
                }

                var psi = new ProcessStartInfo(path, args);
                psi.WorkingDirectory = aria2Dir;
                psi.WindowStyle = ProcessWindowStyle.Hidden;
                psi.CreateNoWindow = true;
                psi.UseShellExecute = false;
                psi.RedirectStandardOutput = false;
                psi.RedirectStandardError = false;
                aria2cProcess = Process.Start(psi);
            }
            catch (Exception ex)
            {
                throw ex;
                // Logger.Debug(ex);
            }
        }

        #endregion

    }
}
