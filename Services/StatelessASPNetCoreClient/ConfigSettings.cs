using System;
using System.Collections.Generic;
using System.Fabric;
using System.Fabric.Description;
using System.Linq;
using System.Threading.Tasks;

namespace StatelessASPNetCoreClient
{
    public class ConfigSettings
    {
        public string NCacheDiscoveryService { get; private set; }

        public ConfigSettings(StatelessServiceContext context)
        {
            context.CodePackageActivationContext.ConfigurationPackageModifiedEvent +=
                CodePackageActivationContext_ConfigurationPackageModifiedEvent;

            this.UpdateConfigSettings(context.CodePackageActivationContext.GetConfigurationPackageObject("Config").Settings);
        }

        private void CodePackageActivationContext_ConfigurationPackageModifiedEvent(object sender, PackageModifiedEventArgs<ConfigurationPackage> e)
        {
            UpdateConfigSettings(e.NewPackage.Settings);
        }

        private void UpdateConfigSettings(ConfigurationSettings settings)
        {
            ConfigurationSection section = settings.Sections["MyConfigSection"];
            NCacheDiscoveryService = section.Parameters["NCacheDiscoveryService"].Value;
        }
    }
}
