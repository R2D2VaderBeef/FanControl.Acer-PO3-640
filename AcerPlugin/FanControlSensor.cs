using FanControl.Plugins;
using System.Runtime.InteropServices;

namespace FanControl.Acer_PO3_640
{
    public partial class FanControlSensor : IPluginControlSensor
    {
        [LibraryImport("ECLibrary.dll")]
        private static partial byte WriteWord(byte _register, ushort _value);

        public FanControlSensor(string id, string name, byte writeAddress, int minSpeed, int maxSpeed)
        {
            Id = id;
            Name = name;
            Value = 0;
            this.writeAddress = writeAddress;
            min = minSpeed;
            max = maxSpeed;

            float difference = max - min;
            percent = difference / (float) 100;
        }

        public string Id { get; }
        public string Name { get; }
        
        private byte writeAddress;

        private int min;
        private int max;
        // One percent of the difference between min and max, mapped to a change of one percent in FanControl
        private float percent;

        // I guess Value is meant to show the value the Control is successfully set to.
        // This pattern is copied from the author's example DellPlugin
        private float newValue;
        public float? Value { get; internal set; } 
        public void Update() {
            Value = newValue;
        }

        public void Set(float val)
        {
            int offset = (int) Math.Round(percent * val, 0);
            int target = min + offset;
            if (target > max) target = max;
            WriteWord(writeAddress, (ushort) target);
            newValue = val;
        }

        public void Reset()
        {
            Set(50);
        }
    }
}
