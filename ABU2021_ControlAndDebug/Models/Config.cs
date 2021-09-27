using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using MVVMLib;

namespace ABU2021_ControlAndDebug.Models
{
    /// <summary>
    /// 設定周り
    /// </summary>
    class Config : NotifyPropertyChanged
    {
        private OutputLog _log;
        private Regex _ipRegex = new Regex(@"\d+[.]\d+[.]\d+[.]\d+");


        #region Singleton instance
        private static Config _instance;
        public static Config GetInstance
        {
            get
            {
                return _instance ?? (_instance = new Config());
            }
        }
        private Config()
        {
            _log = OutputLog.GetInstance;
            ReadConfig();
        }
        ~Config()
        {
        }
        #endregion


        #region Property
        private string _ip;

        public string IP
        {
            get => _ip;
            set { SetProperty(ref _ip, value); }
        }
        #endregion



        #region Method
        private void ReadConfig()
        {
            try
            {
                using (StreamReader file = new StreamReader(Core.ControlType.CONFIG_FILE_NAME))
                {

                    _log.WiteLine("configファイルを読み出し...");
                    string line;
                    var head = new Regex(@"^" + nameof(IP));
                    while ((line = file.ReadLine()) != null)
                    {
                        if (head.IsMatch(line) && _ipRegex.IsMatch(line)) IP = _ipRegex.Match(line).Value;
                    }
                }
            }
            catch(Exception ex)
            {
                Trace.WriteLine("config file open filed. : " + ex.Message);
                _log.WiteErrorMsg("configファイルが見つかりませんでした。");
            }


            //設定の無かった項目をデフォルト化
            if (IP == null)
            {
                IP = Core.ControlType.TCP_IP_ADDRESS;
                _log.WiteErrorMsg("configファイルにIPが無かったためデフォルト値を使用。");
            }


            using(StreamWriter file = new StreamWriter(Core.ControlType.CONFIG_FILE_NAME))
            {
                file.WriteLine(nameof(IP) + " : " + IP);
            }

            _log.WiteLine("config上書き完了。");
        }
        #endregion
    }
}
