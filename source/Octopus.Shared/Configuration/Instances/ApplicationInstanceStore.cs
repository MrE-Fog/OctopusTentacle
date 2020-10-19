using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using Newtonsoft.Json;
using Octopus.Diagnostics;
using Octopus.Shared.Util;

namespace Octopus.Shared.Configuration.Instances
{
    class ApplicationInstanceStore : IApplicationInstanceStore
    {
        readonly ApplicationName applicationName;
        readonly ILog log;
        readonly IOctopusFileSystem fileSystem;
        readonly IRegistryApplicationInstanceStore registryApplicationInstanceStore;
        readonly string machineConfigurationHomeDirectory;

        public ApplicationInstanceStore(ApplicationName applicationName,
            ILog log,
            IOctopusFileSystem fileSystem,
            IRegistryApplicationInstanceStore registryApplicationInstanceStore)
        {
            this.applicationName = applicationName;
            this.log = log;
            this.fileSystem = fileSystem;
            this.registryApplicationInstanceStore = registryApplicationInstanceStore;
            machineConfigurationHomeDirectory = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData), "Octopus");

            if (!PlatformDetection.IsRunningOnWindows)
            {
                machineConfigurationHomeDirectory = "/etc/octopus";
            }
        }

        public class Instance
        {
            public Instance(string name, string configurationFilePath)
            {
                Name = name;
                ConfigurationFilePath = configurationFilePath;
            }

            public string Name { get; }
            public string ConfigurationFilePath { get; set; }
        }

        internal string InstancesFolder()
        {
            return Path.Combine(machineConfigurationHomeDirectory, applicationName.ToString(), "Instances");
        }

        public bool AnyInstancesConfigured()
        {
            var instancesFolder = InstancesFolder();
            if (fileSystem.DirectoryExists(instancesFolder))
            {
                if (fileSystem.EnumerateFiles(instancesFolder).Any())
                    return true;
            }
            var listFromRegistry = registryApplicationInstanceStore.GetListFromRegistry();
            return listFromRegistry.Any();
        }

        public IList<ApplicationInstanceRecord> ListInstances()
        {
            var instancesFolder = InstancesFolder();

            var listFromRegistry = registryApplicationInstanceStore.GetListFromRegistry();
            var listFromFileSystem = new List<ApplicationInstanceRecord>();
            if (fileSystem.DirectoryExists(instancesFolder))
            {
                listFromFileSystem = fileSystem.EnumerateFiles(instancesFolder)
                    .Select(LoadInstanceConfiguration)
                    .Select(instance => new ApplicationInstanceRecord(instance.Name, instance.ConfigurationFilePath))
                    .ToList();
            }

            // for customers running multiple instances on a machine, they may have a version that only understood
            // using the registry. We need to list those too.
            var combinedInstanceList = listFromFileSystem
                .Concat(listFromRegistry.Where(x => listFromFileSystem.All(y => y.InstanceName != x.InstanceName)))
                .OrderBy(i => i.InstanceName);
            return combinedInstanceList.ToList();
        }

        Instance LoadInstanceConfiguration(string path)
        {
            var result = TryLoadInstanceConfiguration(path);
            if (result == null)
                throw new ArgumentException($"Could not load instance at path {path}");
            return result;
        }

        Instance? TryLoadInstanceConfiguration(string path)
        {
            if (!fileSystem.FileExists(path))
                return null;

            var data = fileSystem.ReadFile(path);
            var instance = JsonConvert.DeserializeObject<Instance>(data);
            return instance;
        }

        void WriteInstanceConfiguration(Instance instance, string path)
        {
            var data = JsonConvert.SerializeObject(instance, Formatting.Indented);
            fileSystem.OverwriteFile(path, data);
        }

        public ApplicationInstanceRecord? GetInstance(string instanceName)
        {
            var instancesFolder = InstancesFolder();
            if (fileSystem.DirectoryExists(instancesFolder))
            {
                var instanceConfiguration = Path.Combine(instancesFolder, InstanceFileName(instanceName) + ".config");
                var instance = TryLoadInstanceConfiguration(instanceConfiguration);
                if (instance != null)
                {
                    return new ApplicationInstanceRecord(instance.Name, instance.ConfigurationFilePath);
                }
            }

            // for customers running multiple instances on a machine, they may have a version that only understood
            // using the registry. We need to fall back to there if it doesn't exist in the folder yet.
            var listFromRegistry = registryApplicationInstanceStore.GetListFromRegistry();
            return listFromRegistry.FirstOrDefault(x => x.InstanceName == instanceName);
        }

        string InstanceFileName(string instanceName)
        {
            return instanceName.Replace(' ', '-').ToLower();
        }

        public void SaveInstance(ApplicationInstanceRecord instanceRecord)
        {
            var instancesFolder = InstancesFolder();
            if (!fileSystem.DirectoryExists(instancesFolder))
            {
                fileSystem.CreateDirectory(instancesFolder);
            }
            var instanceConfiguration = Path.Combine(instancesFolder, InstanceFileName(instanceRecord.InstanceName) + ".config");
            var instance = TryLoadInstanceConfiguration(instanceConfiguration) ?? new Instance(instanceRecord.InstanceName, instanceRecord.ConfigurationFilePath);

            instance.ConfigurationFilePath = instanceRecord.ConfigurationFilePath;

            WriteInstanceConfiguration(instance, instanceConfiguration);
        }

        public void DeleteInstance(ApplicationInstanceRecord instanceRecord)
        {
            var instancesFolder = InstancesFolder();
            var instanceConfiguration = Path.Combine(instancesFolder, InstanceFileName(instanceRecord.InstanceName) + ".config");

            fileSystem.DeleteFile(instanceConfiguration);
        }

        public void MigrateInstance(ApplicationInstanceRecord instanceRecord)
        {
            var instanceName = instanceRecord.InstanceName;
            var instancesFolder = InstancesFolder();
            if (File.Exists(Path.Combine(instancesFolder, InstanceFileName(instanceName) + ".config")))
            {
                return;
            }

            using (var cts = new CancellationTokenSource(TimeSpan.FromSeconds(5)))
            {
                var registryInstance = registryApplicationInstanceStore.GetInstanceFromRegistry(instanceName);
                if (registryInstance != null )
                {
                    log.Info($"Migrating {applicationName} instance from registry - {instanceName}");
                    try
                    {
                        SaveInstance(instanceRecord);
                    }
                    catch (Exception ex)
                    {
                        log.Error(ex, "Error migrating instance data");
                        throw;
                    }
                }
            }
        }
    }
}