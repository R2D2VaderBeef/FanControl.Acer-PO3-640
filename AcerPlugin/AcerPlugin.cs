using FanControl.Plugins;
using System.Runtime.InteropServices;
using System.Runtime.Versioning;
using System.Security.Principal;

namespace FanControl.Acer_PO3_640
{
    [SupportedOSPlatform("windows")]
    public partial class AcerPlugin : IPlugin
    {
        [LibraryImport("ECLibrary.dll")]
        private static partial void Setup();

        [LibraryImport("ECLibrary.dll")]
        private static partial void Shutdown();

        public string Name => "Acer-PO3-640";
        private readonly IPluginDialog dialog;
        private readonly IPluginLogger logger;
        private bool failed = false;

        private FanSpeedSensor[] speedSensors =
        {
            new FanSpeedSensor("acer-po3-640-cpu-speed", "CPU Fan Speed", 0x14),
            new FanSpeedSensor("acer-po3-640-front-speed", "Front Case Fan Speed", 0x16),
            new FanSpeedSensor("acer-po3-640-back-speed", "Back Case Fan Speed", 0x1A)
        };
        private FanControlSensor[] ctrlSensors =
        {
            new FanControlSensor("acer-po3-640-cpu-control", "CPU Fan", 0xF0, 500, 2000),
            new FanControlSensor("acer-po3-640-front-control", "Front Case Fan", 0xF2, 600, 3400),
            new FanControlSensor("acer-po3-640-back-control", "Back Case Fan", 0xF6, 800, 3400)
        };

        public AcerPlugin(IPluginLogger logger, IPluginDialog dialog)
        {
            this.logger = logger;
            this.dialog = dialog;
        }

        public void Initialize()
        {
            logger.Log("[Acer-PO3-640][Debug] Initialising...");
            var identity = WindowsIdentity.GetCurrent();
            var principal = new WindowsPrincipal(identity);
            if (principal.IsInRole(WindowsBuiltInRole.Administrator))
            {
                logger.Log("[Acer-PO3-640][Debug] Administrator permission confirmed");
                NativeLibrary.Load(Path.Combine(Directory.GetCurrentDirectory(), "Plugins\\Acer-PO3-640\\ECLibrary.dll"));
                Setup();
            }
            else
            {
                logger.Log("[Acer-PO3-640] Error: Not Run as Administrator.");
                dialog.ShowMessageDialog("Error - Acer-PO3-640 requires you to run FanControl as Administrator.");
                failed = true;
            }
        }
        public void Load(IPluginSensorsContainer container)
        {
            if (failed == true) return;

            container.FanSensors.AddRange(speedSensors);
            container.ControlSensors.AddRange(ctrlSensors);

            logger.Log("[Acer-PO3-640][Debug] Loaded");

        }

        public void Close()
        {
            if (failed == true) return;
            logger.Log("[Acer-PO3-640][Debug] Shutting Down");
            Shutdown();
        }

    }
}
