using FanControl.Plugins;

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
