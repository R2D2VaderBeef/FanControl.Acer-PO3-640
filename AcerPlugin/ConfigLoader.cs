using FanControl.Plugins;
using System;
using Tommy;

namespace FanControl.Acer_PO3_640
{
    public class ConfigLoader
    {
        // The required structure of a fan in the config file
        private static Dictionary<string, Type> fanStructure = new Dictionary<string, Type> 
        {
            {"id", typeof(TomlString)},
            {"name", typeof(TomlString)},
            {"read", typeof(TomlInteger)},
            {"write", typeof(TomlInteger)},
            {"min", typeof(TomlInteger)},
            {"max", typeof(TomlInteger)}
        };

        public static (bool success, FanSpeedSensor[] speedSensors, FanControlSensor[] ctrlSensors) load(IPluginLogger logger)
        {
            List<FanSpeedSensor> speedSensorList = new List<FanSpeedSensor>();
            List<FanControlSensor> ctrlSensorList = new List<FanControlSensor>();

            TomlTable table;

            using (StreamReader reader = File.OpenText(Path.Combine(Directory.GetCurrentDirectory(), "Plugins\\Acer-PO3-640\\config.toml")))
            {
                try
                {
                    table = TOML.Parse(reader);
                }
                catch (TomlParseException ex)
                {
                    logger.Log("[Acer-PO3-640][Error] Your config.toml file contains syntax errors:");
                    foreach (TomlSyntaxException syntaxEx in ex.SyntaxErrors)
                    {
                        logger.Log($"[Acer-PO3-640][Error] Line {syntaxEx.Line} Column {syntaxEx.Column}: {syntaxEx.Message}");
                    }
                    return (false, new FanSpeedSensor[0], new FanControlSensor[0]);
                }
            }

            int counter = 0;
            foreach (TomlNode node in table["fan"])
            {
                counter++;
                bool valid = true;
                foreach (var item in fanStructure)
                {
                    if (!item.Value.IsInstanceOfType(node[item.Key]))
                    {
                        valid = false;
                    }
                }

                if (valid == false)
                {
                    logger.Log($"[Acer-PO3-640][Warning] Fan {counter} in your config.toml file is invalid, so will not appear in Fan Control.");
                }
                else
                {
                    string idBase = "acer-po3-640-" + node["id"];
                    speedSensorList.Add(new FanSpeedSensor(idBase + "-speed", node["name"] + " Speed", (byte)node["read"]));
                    ctrlSensorList.Add(new FanControlSensor(idBase + "-control", node["name"], (byte)node["write"], node["min"], node["max"]));
                }
            }

            FanSpeedSensor[] speedSensors = new FanSpeedSensor[speedSensorList.Count];
            FanControlSensor[] ctrlSensors = new FanControlSensor[ctrlSensorList.Count];

            speedSensorList.CopyTo(speedSensors);
            ctrlSensorList.CopyTo(ctrlSensors);

            return (true, speedSensors, ctrlSensors);
        }
    }
}
