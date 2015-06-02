using System;
using System.Collections.Generic;
using System.Linq;
using NuGet;
using Octopus.Shared.Util;
using SemanticVersion = Octopus.Client.Model.SemanticVersion;

namespace Octopus.Shared.Packages
{
    public class ExternalNuGetPackageAdapter : IExternalPackage
    {
        readonly IPackage wrapped;

        public ExternalNuGetPackageAdapter(IPackage wrapped)
        {
            this.wrapped = wrapped;
        }

        public string PackageId
        {
            get { return wrapped.Id; }
        }

        public SemanticVersion Version
        {
            get { return SemanticVersion.Parse(wrapped.Version.ToString()); }
        }

        public string Description
        {
            get { return wrapped.Description; }
        }

        public string ReleaseNotes
        {
            get { return wrapped.ReleaseNotes; }
        }

        public DateTimeOffset? Published
        {
            get { return wrapped.Published; }
        }

        public string Title
        {
            get { return wrapped.Title; }
        }

        public string Summary
        {
            get { return wrapped.Summary; }
        }

        public bool IsReleaseVersion()
        {
            return wrapped.IsReleaseVersion();
        }

        public long GetSize()
        {
            using (var stream = wrapped.GetStream())
            {
                return stream.Length;
            }
        }

        public List<string> GetDependencies()
        {
            return wrapped.DependencySets.SelectMany(ds => ds.Dependencies).Select(dependency => dependency.ToString()).ToList();
        }

        public string CalculateHash()
        {
            using (var stream = wrapped.GetStream())
            {
                return HashCalculator.Hash(stream);
            }
        }

        public IPackage GetRealPackage()
        {
            return wrapped;
        }
    }
}