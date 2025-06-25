using FanControl.Plugins;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Runtime.Versioning;
using System.Security.Principal;

namespace FanControl.Acer_PO3_640
{
    [SupportedOSPlatform("windows")]
    public partial class AcerPlugin : IPlugin2
    {
        [LibraryImport("ECLibrary.dll")]
        private static partial ushort ReadWord(byte _register);
        
        [LibraryImport("ECLibrary.dll")]
        private static partial byte WriteWord(byte _register, ushort _value);

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
                Marshal.PrelinkAll(typeof(AcerPlugin));
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
            container.FanSensors.Add(new FakeSensor("po3-640-cpu", "CPU Fan", 2500));
            container.TempSensors.Add(new FakeSensor("po3-640-faketemp", "Fake Temperature", 2500));
            logger.Log("[Acer-PO3-640][Debug] Loaded");
        }

        public void Close()
        {
            if (failed == true) return;
            logger.Log("[Acer-PO3-640][Debug] Shutting Down");
        }

        public void Update()
        {
            if (failed == true) return;
        }
    }
}
