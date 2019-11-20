using System;
using System.Collections.Generic;
using System.Fabric;
using System.Fabric.Description;
using System.Linq;
using System.Threading.Tasks;


namespace NCacheDiscovery
{
    public class ConfigSettings
    {
        public int ReverseProxyPort { get; private set; }
        public string NCacheServiceName { get; private set; }

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
            NCacheServiceName = section.Parameters["NCacheServiceName"].Value;
            ReverseProxyPort = int.Parse(section.Parameters["ReverseProxyPort"].Value);
        }
    }
}
