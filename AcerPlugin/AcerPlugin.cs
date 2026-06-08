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

            var (success, speedSensors, ctrlSensors) = ConfigLoader.load(logger);
            if (success == false) return;

            logger.Log($"[Acer-PO3-640][Debug] Loaded config file: {speedSensors.Length} speed sensors, {ctrlSensors.Length} control sensors");

            container.FanSensors.AddRange(speedSensors);
            container.ControlSensors.AddRange(ctrlSensors);

            logger.Log("[Acer-PO3-640][Debug] Registered sensors with Fan Control");
        }

        public void Close()
        {
            if (failed == true) return;
            logger.Log("[Acer-PO3-640][Debug] Shutting Down");
            Shutdown();
        }

    }
}
