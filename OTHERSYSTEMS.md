# Adapting the plugin for other systems
I believe that this plugin could be adapted to support any Acer Predator desktop, or any other machine which controls the fans through the motherboard's Embedded Controller (EC), including the many laptops supported by [NoteBook FanControl](https://github.com/hirschmann/nbfc). 

The initial release of the plugin is hardcoded to the config for my machine, meaning to enable a custom config you'll need to build your own version of FanControl.Acer-PO3-640.dll - the C# .NET project contained within the [AcerPlugin](AcerPlugin/) directory. Notes on building from source are [in the README](/README.md#building-from-source). I may add config file loading in the future.

My version may support other Acer Predator Orion 3000 desktops out-of-the-box, but you must confirm this first using the first step detailed below.

## 1. Finding the EC registers
You're looking for the EC registers which set each fan's Target Speed (which we want to write to), and those which report back on its Actual Speed (which we want to read from). To do this, use NoteBook FanControl's [ec-probe tool](https://github.com/hirschmann/nbfc/wiki/EC-probing-tool), or another tool of your choice. In future, I may make a tool of my own to make this easier. 

Download and install NoteBook FanControl from [the latest release](https://github.com/hirschmann/nbfc/wiki/EC-probing-tool). Open the install location (default `C:\Program Files (x86)\NoteBook FanControl`) in your terminal. 

Follow the steps detailed [in their guide](https://github.com/hirschmann/nbfc/wiki/Probe-the-EC%27s-registers) to find the correct registers. You're looking for registers which change in value at the same time as your fan speed goes up or down. A basic OEM tool which can manually change your fan speed e.g. PredatorSense will make it easier to find the correct registers. It may be tricky to spot the registers containing Actual Speed, as they may change subtly while the fan is at a seemingly steady speed - a tool like PredatorSense or HWMonitor which can display the current fan speed will be useful to compare the values with.

On the Acer Predator PO3-640, the fan speed values are the Target RPM and Actual RPM, stored as 16-bit words over two registers. The ec-probe tool will therefore display the hex-encoded values across two rows. 

These are the correct EC registers on my machine:
| Fan         | Target Speed | Actual Speed |
| ----------- | ------------ | ------------ |
| CPU         | 0xF0         | 0x14         |
| Front Case  | 0xF2         | 0x16         |
| Back Case   | 0xF6         | 0x1A         |

If your registers are the same as mine, your motherboard is compatible with my build of the plugin. You should still continue with the second step if your fans aren't similar to mine.

## 2. Finding the minimum and maximum speed

Now you have enough information to add Speed Sensors to the AcerPlugin.cs file. 
```cs
private FanSpeedSensor[] speedSensors =
{                      
    new FanSpeedSensor( string id, // Give it a unique ID
                        string name, // Give it a name
                        byte readAddress // The Actual Speed address from above
                      ),
    new FanSpeedSensor("acer-po3-640-cpu-speed", "CPU Fan Speed", 0x14),
    new FanSpeedSensor("acer-po3-640-front-speed", "Front Case Fan Speed", 0x16),
    new FanSpeedSensor("acer-po3-640-back-speed", "Back Case Fan Speed", 0x1A)
};
```
You'll have to change further logic in the FanSpeedSensor.cs file (and potentially in ECLibrary.dll) if your EC uses bytes instead of words and/or if you want to convert arbitrary values into the true RPM. 

Defining only the Speed Sensors (and clearing my existing Control Sensors from the array below), building the plugin and installing it in Fan Control may be useful. Create the folder `Plugins\Acer-PO3-640` within your Fan Control installation, and copy your new FanControl.Acer-PO3-640.dll file there along with the provided (or your modified) ECLibrary.dll and WinRing0x64.dll files. You can then view the live value of the Actual Speed registers on the Home tab by clicking "View options" towards the top right and unchecking "Hide fan speed cards" if it is checked.

To find the maximum and minimum speeds, use the ec-probe tool or a custom build of my TestApp program to set increasingly high (and later low) values to the Target Speed register(s): 
```cs
ec-probe.exe write 0xF2 2500 
// Or...
TestApp.exe set front 2500 // assuming you have set up the correct register addresses in the switch statement in Program.cs in your build
```
Until the value of the Actual Speed register stops increasing / decreasing, or you reach the high / low RPM limit from the fan's data sheet. View the Actual Speed in Fan Control or by querying the correct register with ec-probe. 
```cs
ec-probe.exe read 0xF2
```
Record the final values you **set the Target Speed register to** before the fan reaches its maximum and minimum speeds - these are the values you'll need.

When I tried to set the speed too low, I found my fans behaved erratically, with the speed fluctuating and the Stock fans stopping and starting up again. Because of this, for my minimum value I tried to achieve the lowest stable speed. Those who know more about hardware than me may understand why this happens. 

My expanded data table, with the maximum and minimum values of Target Speed based on my testing, looks like this:
| Fan         | Target Speed | Actual Speed | Minimum Value | Maximum Value |
| ----------- | ------------ | ------------ | ------------- | ------------- |
| CPU         | 0xF0         | 0x14         | 500           | 2000          |
| Front Case  | 0xF2         | 0x16         | 600           | 3400          |
| Back Case   | 0xF6         | 0x1A         | 800           | 3400          |

Once you have this information, you can complete your configuration by adding Control Sensors to the AcerPlugin.cs file: 
```cs
private FanControlSensor[] ctrlSensors =
{
    new FanControlSensor( string id, // Give it a unique ID
                          string name, // Give it a name
                          byte writeAddress, // The Target Speed address from above
                          int minSpeed, // The Minimum Value from above
                          int maxSpeed // The Maximum Value from above
                        )
    new FanControlSensor("acer-po3-640-cpu-control", "CPU Fan", 0xF0, 500, 2000),
    new FanControlSensor("acer-po3-640-front-control", "Front Case Fan", 0xF2, 600, 3400),
    new FanControlSensor("acer-po3-640-back-control", "Back Case Fan", 0xF6, 800, 3400)
};
```
After changing any logic in the FanControlSensor.cs file relating to the size of the data on the EC or transforming the values at all, build the plugin and copy the FanControl.Acer-PO3-640.dll file 