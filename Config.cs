
using System;
using System.IO;
using Newtonsoft.Json;

namespace TimeCurrency
{
    public class TimeCurrencyConfig
    {
        
      
//variables go here
        public string DeadGroupPrefix = "|Dead|";


        public static TimeCurrencyConfig Read(string path)
        {
            if (!File.Exists(path))
                return new TimeCurrencyConfig();
            using (var fs = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read))
            {
                return Read(fs);
            }
        }

        public static TimeCurrencyConfig Read(Stream stream)
        {
            using (var sr = new StreamReader(stream))
            {
                var cf = JsonConvert.DeserializeObject<TimeCurrencyConfig>(sr.ReadToEnd());
                if (ConfigRead != null)
                    ConfigRead(cf);
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

        public static Action<TimeCurrencyConfig> ConfigRead;
    }
}