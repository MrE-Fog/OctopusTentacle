using System;
using System.IO;
using System.Security.Cryptography;
using Octopus.Shared.Diagnostics;
using Octopus.Shared.Security.MasterKey;

namespace Octopus.Shared.Configuration
{
    public class OctopusServerStorageConfiguration : IOctopusServerStorageConfiguration
    {
        readonly IKeyValueStore settings;
        readonly IHomeConfiguration home;
        readonly ILog log = Log.Octopus();

        public OctopusServerStorageConfiguration(IKeyValueStore settings, IHomeConfiguration home)
        {
            this.settings = settings;
            this.home = home;

            if (MasterKey == null)
            {
                log.Info("Generating a new Master Key for this Octopus Server...");
                MasterKey = MasterKeyEncryption.GenerateKey();
                Save();
                log.Info("Master Key saved; use the Octopus Administration tool to back the key up.");
            }
        }

        public string UniqueControllerName
        {
            get { return settings.Get("Octopus.Server.UniqueControllerName", Environment.MachineName); }
            set { settings.Set("Octopus.Server.UniqueControllerName", value); }
        }

        public string ExternalDatabaseConnectionString
        {
            get { return settings.Get("Octopus.Storage.ExternalDatabaseConnectionString"); }
            set { settings.Set("Octopus.Storage.ExternalDatabaseConnectionString", value); }
        }

        public bool BackupsEnabledByDefault
        {
            get { return settings.Get("Octopus.Storage.BackupsEnabledByDefault", true); }
            set { settings.Set("Octopus.Storage.BackupsEnabledByDefault", value); }
        }

        public StorageMode StorageMode
        {
            get { return settings.Get("Octopus.Storage.Mode", StorageMode.Embedded); }
            set { settings.Set("Octopus.Storage.Mode", value); }
        }

        public string EmbeddedDatabaseStoragePath
        {
            get { return Path.Combine(home.HomeDirectory, "RavenDB"); }
        }

        public int EmbeddedDatabaseListenPort
        {
            get { return settings.Get("Octopus.Storage.EmbeddedDatabaseListenPort", 10931); }
            set { settings.Set("Octopus.Storage.EmbeddedDatabaseListenPort", value); }
        }

        public string EmbeddedDatabaseListenHostname
        {
            get { return settings.Get("Octopus.Storage.EmbeddedDatabaseListenHostname", "localhost"); }
            set { settings.Set("Octopus.Storage.EmbeddedDatabaseListenHostname", value); }
        }

        public byte[] MasterKey
        {
            get
            {
                return settings.Get<byte[]>("Octopus.Storage.MasterKey", protectionScope: DataProtectionScope.LocalMachine);
            }
            set
            {
                settings.Set("Octopus.Storage.MasterKey", value, DataProtectionScope.LocalMachine);
            }
        }

        public string ActivityLogDirectory
        {
            get { return Path.Combine(home.ApplicationSpecificHomeDirectory, "ActivityLogs"); }
        }

        public void Save()
        {
            settings.Save();
        }
    }
}