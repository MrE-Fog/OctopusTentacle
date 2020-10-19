using Autofac;
using Octopus.Configuration;
using Octopus.Shared.Configuration.Instances;
using Octopus.Shared.Services;
using Octopus.Shared.Startup;
using Octopus.Shared.Util;

namespace Octopus.Shared.Configuration
{
    public class ConfigurationModule : Module
    {
        readonly ApplicationName applicationName;
        readonly string instanceName;

        public ConfigurationModule(ApplicationName applicationName, string instanceName)
        {
            this.applicationName = applicationName;
            this.instanceName = instanceName;
        }

        protected override void Load(ContainerBuilder builder)
        {
            base.Load(builder);

            if (PlatformDetection.IsRunningOnWindows)
            {
                builder.RegisterType<RegistryApplicationInstanceStore>()
                    .WithParameter("applicationName", applicationName)
                    .As<IRegistryApplicationInstanceStore>();
                builder.RegisterType<WindowsServiceConfigurator>().As<IServiceConfigurator>();
            }
            else
            {
                builder.RegisterType<NullRegistryApplicationInstanceStore>().As<IRegistryApplicationInstanceStore>();
                builder.RegisterType<LinuxServiceConfigurator>().As<IServiceConfigurator>();
            }

            builder.RegisterType<ApplicationInstanceStore>()
                .WithParameter("applicationName", applicationName)
                .As<IApplicationInstanceStore>();

            builder.RegisterType<ApplicationInstanceSelector>()
                .WithParameter("applicationName", applicationName)
                .WithParameter("currentInstanceName", instanceName)
                .As<IApplicationInstanceSelector>()
                .SingleInstance();

            builder.Register(c =>
            {
                var selector = c.Resolve<IApplicationInstanceSelector>();
                return selector.GetCurrentConfiguration();
            }).As<IKeyValueStore>().SingleInstance();
            builder.Register(c =>
            {
                var selector = c.Resolve<IApplicationInstanceSelector>();
                return selector.GetWritableCurrentConfiguration();
            }).As<IWritableKeyValueStore>().SingleInstance();


            builder.RegisterType<HomeConfiguration>()
                .WithParameter("application", applicationName)
                .As<IHomeConfiguration>()
                .SingleInstance();
            builder.RegisterType<WritableHomeConfiguration>()
                .WithParameter("application", applicationName)
                .As<IWritableHomeConfiguration>()
                .SingleInstance();

            builder.RegisterType<LoggingConfiguration>().As<ILoggingConfiguration>().SingleInstance();
            builder.RegisterType<ProxyConfigParser>().As<IProxyConfigParser>();
            builder.RegisterType<ProxyConfiguration>().As<IProxyConfiguration>();
            builder.RegisterType<WritableProxyConfiguration>().As<IWritableProxyConfiguration>();
            builder.RegisterType<ProxyInitializer>().As<IProxyInitializer>().SingleInstance();
            RegisterWatchdog(builder);
        }

        private void RegisterWatchdog(ContainerBuilder builder)
        {
            if (PlatformDetection.IsRunningOnWindows)
            {
                builder.RegisterType<Watchdog>().As<IWatchdog>()
                    .WithParameter("applicationName", applicationName);
            }
            else
            {
                builder.RegisterType<NullWatchdog>().As<IWatchdog>();
            }
        }
    }
}