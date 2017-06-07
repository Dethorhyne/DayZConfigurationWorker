using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ShardTools.DayZConfigurationWorker
{
   
    public enum LineType
    {
        Text,
        NumberInt,
        NumberDouble,
        Boolean,
        KeyBinding,
        SpecialText,
        Misc
    }
    /// <summary>
    /// Contains the entire dayz configuration data structured into two lists that can be altered via [] operator by their keys (Similar to key usage in Dictionary class).
    /// </summary>
    public class DayZConfig
    {
        /// <summary>
        /// Contains entire structure of the selected .DayZProfile file. </summary>
        public List<ConfigEntry> ProfileConfig { get; set; }
        /// <summary>
        /// Contains entire structure of the DayZ.cfg file. </summary>
        public List<ConfigEntry> CfgConfig { get; set; }

        private string ProfilePath;
        private string CfgPath;

        /// <summary>
        /// DayZConfig constructor which determines configuration file paths and loads all of the data from the configuration files. </summary>
        /// <param name="profilePath"> Full path to the .DayZProfile file.</param>
        public DayZConfig(string profilePath)
        {
            ProfilePath = profilePath;
            var files = Directory.GetFiles(Path.GetDirectoryName(profilePath), "*.cfg").ToList();
            CfgPath = files.Where(x => Path.GetFileNameWithoutExtension(x).ToLower() == "dayz").FirstOrDefault();

            ProfileConfig = LoadConfig(ProfilePath);
            CfgConfig = LoadConfig(CfgPath);

        }

        /// <summary>
        /// Saves all configuration entries of the selected .DayZProfile configuration file to that file replacing all changed values. </summary>
        /// <param name="CreateBackup">If true a original configuration file will be saved under a backup name and new configuration file will be created with altered settings.</param>
        public void UpdateProfile(bool CreateBackup)
        {
            using (StreamWriter SW = new StreamWriter(ProfilePath, false))
            {
                foreach (var x in ProfileConfig)
                {
                    SW.WriteLine(x);
                }
            }
        }

        /// <summary>
        /// Saves all configuration entries of the DayZ.cfg configuration file to that file replacing all changed values. </summary>
        /// <param name="CreateBackup">If true a original configuration file will be saved under a backup name and new configuration file will be created with altered settings.</param>
        public void UpdateConfig(bool CreateBackup)
        {
            using (StreamWriter SW = new StreamWriter(CfgPath, false))
            {
                foreach (var x in CfgConfig)
                {
                    SW.WriteLine(x);
                }
            }
        }

        /// <summary>
        /// Opens a file and reads all of the configuration entries and parses the data. </summary>
        /// <param name="FilePath"> Full path to the .DayZProfile configuration file.</param>
        /// <returns>
        /// Returns a List of ConfigEntry objects that contain parsed data of the configuration entries.</returns>
        private List<ConfigEntry> LoadConfig(string FilePath)
        {
            List<ConfigEntry> ConfigLines = new List<ConfigEntry>();

            StreamReader sr = new StreamReader(FilePath);
            string line = "";
            line = sr.ReadLine();

            while(line!=null)
            {
                switch(DetermineLineType(line.ToLower().TrimStart()))
                {
                    case LineType.Boolean:
                        ConfigLines.Add(new BoolValue(LineType.Boolean, line));
                        break;
                    case LineType.KeyBinding:
                        ConfigLines.Add(new BindingValue(LineType.KeyBinding, line));
                        break;
                    case LineType.NumberDouble:
                        ConfigLines.Add(new DoubleValue(LineType.NumberDouble, line));
                        break;
                    case LineType.NumberInt:
                        ConfigLines.Add(new IntValue(LineType.NumberInt, line));
                        break;
                    case LineType.SpecialText:
                        ConfigLines.Add(new TextValue(LineType.SpecialText, line));
                        break;
                    case LineType.Text:
                        ConfigLines.Add(new TextValue(LineType.Text, line));
                        break;
                    default:
                        ConfigLines.Add(new ConfigEntry(LineType.Misc, line));
                        break;

                }

                line = sr.ReadLine();

            }

            return ConfigLines;
        }

        /// <summary>
        /// </summary>
        /// <param name="line"></param>
        /// <returns>
        /// </returns>
        private LineType DetermineLineType(string line)
        {
            if (line.StartsWith("playername") || line.StartsWith("lastmpservername"))
            {
                return LineType.SpecialText;
            }
            if (line.Contains("[") && line.Contains("]") && line.Contains("{") && line.Contains("}") && line.Contains(";"))
            {
                return LineType.KeyBinding;
            }
            if (line.Contains("\"") && line.Contains(";") && line.Contains("="))
            {
                return LineType.Text;
            }
            if (line.Contains(".") && line.Contains(";") && line.Contains("="))
            {
                return LineType.NumberDouble;
            }
            if (line.Contains(";") && line.Contains("="))
            {
                if (line.StartsWith("windowed") || line.StartsWith("ssaoenabled") || line.StartsWith("vsync") || line.StartsWith("perspective") || line.StartsWith("trackir") || line.StartsWith("freetrack") || line.StartsWith("triplehead") || line.StartsWith("showtitles") || line.StartsWith("useimperialsystem") || line.StartsWith("vehiclefreelook") || line.StartsWith("showradio") || line.StartsWith("battleyelicense"))
                {
                    return LineType.Boolean;
                }
                if (line.StartsWith("headbob") || line.StartsWith("gamma") || line.StartsWith("bloom") || line.StartsWith("fov=") || line.StartsWith("mousesmoothing"))
                {
                    return LineType.NumberDouble;
                }
                return LineType.NumberInt;
            }

            return LineType.Misc;
        }

        /// <summary>
        /// Finds and retrieves a ConfigEntry object inside the Config and Profile entries. </summary>
        /// <param name="key"> Key name of the config entry. eg. "playerName". This key is NOT case sensitive.</param>
        /// <returns>
        /// Returns a single ConfigEntry object that matches the key provided.</returns>
        public ConfigEntry this[string key]
       {
            get
            {
                key = key.ToLower();
                if (CfgConfig.Any(x => x.key == key))
                    return CfgConfig.Where(x => x.key == key).FirstOrDefault();
                if (ProfileConfig.Any(x => x.key == key))
                    return ProfileConfig.Where(x => x.key == key).FirstOrDefault();
                
                return null;
            }
        }


        public class ConfigEntry
        {
            public LineType Type { get; set; }
            public string Line { get; private set; }
            public string line { get { return Line.ToLower(); } }

            public virtual string Key { get; internal set; }
            public virtual dynamic Value { get; set; }
            public virtual string key { get { return null; } }

            internal int Indentations = 0;

            public ConfigEntry(LineType type, string line)
            {
                Type = type;

                Indentations = line.TakeWhile(x => x == '\t').Count();

                Line = line.TrimStart('\t');
                Key = null;
                Value = null;
            }

            public override string ToString()
            {
                return IndentationBuilder() + Line;
            }

            internal string IndentationBuilder()
            {
                string indents = "";
                for(int i=0;i<Indentations;i++)
                {
                    indents += "\t";
                }
                return indents;
            }


        }

        public class TextValue : ConfigEntry
        {
            internal string Quote = "\"";
            public override string Key { get; internal set; }
            public override dynamic Value { get; set; }
            public override string key { get { return Key.ToLower(); } }
            public string value { get { return Value.ToLower(); } }
            

            public TextValue(LineType type, string line) : base(type,line)
            {
                line = line.TrimStart('\t').TrimEnd(';');
                Key = line.Substring(0, line.IndexOf('='));
                line = line.Remove(0, line.IndexOf('=') + 1);
                Value = line.TrimStart('"').TrimEnd('"');

            }

            public override string ToString()
            {
                return $@"{IndentationBuilder()}{Key}={Quote}{Value}{Quote};";
            }
        }

        public class BindingValue : ConfigEntry
        {
            public override string Key { get; internal set; }
            public override dynamic Value { get; set; }
            public override string key { get { return Key.ToLower(); } }
            public string value { get { return Value.ToLower(); } }


            public BindingValue(LineType type, string line) : base(type, line)
            {
                line = line.TrimStart('\t').TrimEnd(';');
                Key = line.Substring(0, line.IndexOf('='));
                line = line.Remove(0, line.IndexOf('=') + 1);
                Value = line;

            }

            public override string ToString()
            {
                return $@"{IndentationBuilder()}{Key}={Value};";
            }
        }

        public class DoubleValue : ConfigEntry
        {
            public override string Key { get; internal set; }
            public override dynamic Value { get; set; }
            public override string key { get { return Key.ToLower(); } }
            public dynamic value { get { return Value; } }

            private string Delimiter { get { return Thread.CurrentThread.CurrentCulture.NumberFormat.CurrencyDecimalSeparator; } }


            public DoubleValue(LineType type, string line) : base(type, line)
            {
                line = line.TrimStart('\t').TrimEnd(';');
                Key = line.Substring(0, line.IndexOf('='));
                line = line.Remove(0, line.IndexOf('=') + 1);
                Value = Convert.ToDouble(line.Replace(".",Delimiter));

            }

            public override string ToString()
            {
                return $@"{IndentationBuilder()}{Key}={Value.ToString().Replace(Delimiter,".")};";
            }
        }

        public class IntValue : ConfigEntry
        {
            public override string Key { get; internal set; }
            public override dynamic Value { get; set; }
            public override string key { get { return Key.ToLower(); } }
            public dynamic value { get { return Value; } }
            


            public IntValue(LineType type, string line) : base(type, line)
            {
                line = line.TrimStart('\t').TrimEnd(';');
                Key = line.Substring(0, line.IndexOf('='));
                line = line.Remove(0, line.IndexOf('=') + 1);
                Value = Convert.ToInt32(line);

            }

            public override string ToString()
            {
                return $@"{IndentationBuilder()}{Key}={Value};";
            }
        }

        public class BoolValue : ConfigEntry
        {
            public override string Key { get; internal set; }
            public override dynamic Value { get; set; }
            public override string key { get { return Key.ToLower(); } }
            public dynamic value { get { return Value; } }



            public BoolValue(LineType type, string line) : base(type, line)
            {
                line = line.TrimStart('\t').TrimEnd(';');
                Key = line.Substring(0, line.IndexOf('='));
                if (line.Remove(0, line.IndexOf('=') + 1) == "1")
                    Value = true;
                else
                    Value = false;

            }

            public override string ToString()
            {
                int val = Value ? 1 : 0;

                return $@"{IndentationBuilder()}{Key}={val};";
            }
        }

    }
}
