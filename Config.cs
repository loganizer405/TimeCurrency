using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Newtonsoft.Json;
using TShockAPI;
using Terraria;

namespace TimeCurrency
{
    public class TimeConfig
    {
        //public variables go here not static
        public string DeadGroupPrefix = "(Dead)";
        public string DeadGroupSuffix = "";
        public string DeadGroupColor = "255,255,255";
        public bool ShowBalanceOnLogin = false;
        public string ShowBalanceTemplate = "Welcome, you have {days}, {hours}, {minutes}, and {seconds} of time in your account.";

        public static TimeConfig Read(string path)
        {
            if (!File.Exists(path))
                return new TimeConfig();
            using (var fs = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read))
            {
                return Read(fs);
            }
        }
        public static TimeConfig Read(Stream stream)
        {
            using (var sr = new StreamReader(stream))
            {
                var cf = JsonConvert.DeserializeObject<TimeConfig>(sr.ReadToEnd());
                if (TimeConfigRead != null)
                    TimeConfigRead(cf);
                return cf;
            }
        }
        public void Write(string path)
        {
            using (var fs = new FileStream(path, FileMode.Create, FileAccess.Write, FileShare.Write))
            {
                Write(fs);
            }
        }
        public void Write(Stream stream)
        {
            var str = JsonConvert.SerializeObject(this, Formatting.Indented);
            using (var sw = new StreamWriter(stream))
            {
                sw.Write(str);
            }
        }
        public static Action<TimeConfig> TimeConfigRead;
    }
}