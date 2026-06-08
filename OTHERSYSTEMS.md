# Configuring the plugin for other systems
This plugin reads from a config file located at `<your FanControl installation>\Plugins\Acer-PO3-640\config.toml` after installation.

The plugin should be configurable to support other Acer Predator desktops and many other machines which control the fans through the motherboard's Embedded Controller (EC), including many laptops supported by [NoteBook FanControl](https://github.com/hirschmann/nbfc), given that they have the following properties:
- They store fan speeds as 16-bit words (across two bytes), big endian
- They store a value for target/set and actual/reported speed
- At least the actual/reported speed value is stored as RPM

My default configuration may support other Acer Predator Orion 3000 desktops out-of-the-box, but you must confirm this first using the first step detailed below.

If you have a Predator PO3-640 with different fans to me, complete the second step below (or use the speeds provided by the manufacturer) to ensure the plugin uses the correct speed range for your fans.

## 1. Finding the EC registers
You're looking for the EC registers which set each fan's Target Speed (which we want to write to), and those which report back on its Actual Speed (which we want to read from). To do this, use NoteBook FanControl's [ec-probe tool](https://github.com/hirschmann/nbfc/wiki/EC-probing-tool), or another tool of your choice. In future, I may make a tool of my own to make this easier. 

Download and install NoteBook FanControl from [the latest release](https://github.com/hirschmann/nbfc/releases/tag/1.6.3). Open the install location (default `C:\Program Files (x86)\NoteBook FanControl`) in your terminal. 

Follow the steps detailed [in their guide](https://github.com/hirschmann/nbfc/wiki/Probe-the-EC%27s-registers) to find the correct registers. You're looking for registers which change in value at the same time as your fan speed goes up or down. A basic OEM tool which can manually change your fan speed e.g. PredatorSense will make it easier to find the correct registers. It may be tricky to spot the registers containing Actual Speed, as they may change subtly while the fan is at a seemingly steady speed - a tool like PredatorSense or HWMonitor which can display the current fan speed will be useful to compare the values with.

On the Acer Predator PO3-640, the fan speed values are the Target RPM and Actual RPM, stored as 16-bit words over two registers. The ec-probe tool will therefore display the hex-encoded values across two rows. 

These are the correct EC registers on my machine:
| Fan         | Target Speed | Actual Speed |
| ----------- | ------------ | ------------ |
| CPU         | 0xF0         | 0x14         |
| Front Case  | 0xF2         | 0x16         |
| Back Case   | 0xF6         | 0x1A         |

If your registers are the same as mine, your motherboard is compatible with the default configuration of the plugin. If your motherboard / PC model is not already listed in the README, please open an issue on GitHub to let me know! You should still continue with the second step if your fans aren't similar to mine.

If your registers are different to mine, you'll need to edit the config file (located at `<your FanControl installation>\Plugins\Acer-PO3-640\config.toml` after installation). Edit the ID/name of the existing fans as necessary, then set the `read` property to the Actual Speed register address you found, and the `write` property to the Target Speed register address you found. Here is my CPU fan configured with the values from above:
```toml
[[fan]]
id = "cpu"             # A unique ID
name = "CPU Fan"       # The name which will appear in Fan Control
read = 0x14            # EC register address for the first byte of the actual / reported speed
write = 0xF0           # EC register address for the first byte of the target / set speed
#min = ???             
#max = ???             
```
You can freely add new fans or remove ones you don't need, provided that each fan begins with `[[fan]]` and includes all of the properties. We will find the correct values for `min` and `max` next.

## 2. Finding the minimum and maximum speed
As the Predator PO3-640 stores RPM values (rather than percentage or some other value) to its EC registers, that is what we will discuss here, though in principle you could use any arbitrary minimum/maximum values you discover. 

The easiest way to do this is to use the speeds provided by the manufacturer and call it a day.

To find the 'true' maximum and minimum speeds, you can use the ec-probe tool again. If you skipped the first step above, you'll need to install [NoteBook FanControl](https://github.com/hirschmann/nbfc/releases/tag/1.6.3), then open the install location (default `C:\Program Files (x86)\NoteBook FanControl`) in your terminal. 

Use ec-probe to to set increasingly high (and later low) values to the Target Speed register(s): 
```cs
ec-probe.exe write 0xF2 2500 
```
Until the value of the Actual Speed register stops increasing / decreasing, or you reach the high / low RPM limit from the fan's data sheet. View the Actual Speed by querying the correct register with ec-probe. 
```cs
ec-probe.exe read 0x16
```
Record the final values you **set the Target Speed register to** before the fan reaches its maximum and minimum speeds - these are the values you'll need.

When I tried to set the speed too low, I found my fans behaved erratically, with the speed fluctuating and the Stock fans stopping and starting up again. Because of this, for my minimum value I tried to achieve the lowest stable speed. Those who know more about hardware than me may understand why this happens. 

My expanded data table, with the maximum and minimum values of Target Speed based on my testing, looks like this:
| Fan         | Target Speed | Actual Speed | Minimum Value | Maximum Value |
| ----------- | ------------ | ------------ | ------------- | ------------- |
| CPU         | 0xF0         | 0x14         | 500           | 2000          |
| Front Case  | 0xF2         | 0x16         | 600           | 3400          |
| Back Case   | 0xF6         | 0x1A         | 800           | 3400          |

Once you have this information, you can complete your configuration by editing the `min` and `max` values in the config file (located at `<your FanControl installation>\Plugins\Acer-PO3-640\config.toml` after installation) to match the Minimum and Maximum values you found. Here is the complete configuration for my CPU fan based on the data above:   
```toml
[[fan]]
id = "cpu"             # A unique ID
name = "CPU Fan"       # The name which will appear in Fan Control
read = 0x14            # EC register address for the first byte of the actual / reported speed
write = 0xF0           # EC register address for the first byte of the target / set speed
min = 500              # The minimum speed, in RPM, the fan will spin at
max = 2000             # The maximum speed, in RPM, the fan will spin at
```