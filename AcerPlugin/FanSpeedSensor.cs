using FanControl.Plugins;
using System.Runtime.InteropServices;

namespace FanControl.Acer_PO3_640
{
    public partial class FanSpeedSensor : IPluginSensor
    {
        [LibraryImport("ECLibrary.dll")]
        private static partial ushort ReadWord(byte _register);

        public FanSpeedSensor(string id, string name, byte readAddress)
        {
            Id = id;
            Name = name;
            Value = 0;
            this.readAddress = readAddress;
        }

        public string Id { get; }
        public string Name { get; }

        private byte readAddress;

        public float? Value { get; internal set; }
        public void Update() {
            ushort newValue = ReadWord(readAddress);
            Value = newValue;
        }
    }
}
