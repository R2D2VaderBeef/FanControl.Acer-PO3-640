using FanControl.Plugins;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace FanControl.Acer_PO3_640
{
    public class FakeSensor : IPluginSensor
    {
        internal FakeSensor(string id, string name, float value)
        {
            Id = id;
            Name = name;
            Value = value;
        }

        public string Id { get; }
        public string Name { get; }

        public float? Value { get; internal set; }
        public void Update() { }
    }
}
