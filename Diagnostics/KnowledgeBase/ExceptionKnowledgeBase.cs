﻿using System;
using System.Collections.Generic;
using System.Data.Services.Client;
using System.IO;
using System.Net;
using System.Net.Sockets;
using Octopus.Shared.Util;

namespace Octopus.Shared.Diagnostics.KnowledgeBase
{
    public static class ExceptionKnowledgeBase
    {
        static readonly List<ExceptionKnowledge> Rules = new List<ExceptionKnowledge>();

        static ExceptionKnowledgeBase()
        {
            AddRule(r => r.ExceptionIs<InvalidOperationException>(
                ex => ex.Message.StartsWith("Could not connect to the feed"),
                (ex, s) => s["Generic"] = ex.Message)
                .HasInnerException<DataServiceClientException>(
                    (ex, s) => s["Client"] = ex.Message)
                .EntrySummaryIs(
                    s => s["Generic"] + " The feed responded with: " + s["Client"]));

            AddRule(r => r.ExceptionIs<InvalidOperationException>(
                ex => (ex.Message.Contains("Cannot load Counter Name data because an invalid index") ||
                    ex.Message.Contains("The configuration registry key is invalid")) &&
                    ex.StackTrace.Contains("System.Diagnostics.PerformanceCounterCategory") &&
                    ex.StackTrace.Contains("Raven."))
                .EntrySummaryIs("Storage initialization failed because Windows Performance Counters are corrupted.")
                .EntryHelpTextIs("From time to time the Windows Performance Counters used by RavenDB become corrupted. " +
                    "These can be fixed by rebuilding them with the Windows `lodctr /R` command.")
                .EntryHelpLinkIs("http://g.octopushq.com/RebuildingPerfCounters"));

            AddRule(r => r.ExceptionIs<FileNotFoundException>(
                ex => ex.Message.Contains("Could not load file or assembly") &&
                    ex.Message.Contains("XmlSerializers") &&
                    ex.StackTrace.Contains("System.DirectoryServices.AccountManagement"))
                .EntrySummaryIs("Active Directory integration failed because of a bug in this Windows version.")
                .EntryHelpTextIs("Windows 7 and Windows Server 2008 R2 cause the .NET directory services provider to throw random exceptions. " +
                    "The error detected here is commonly associated with that bug. " +
                    "Please refer to this Microsoft Support article with links to a hotfix: http://g.octopushq.com/ADHotfix")
                .EntryHelpLinkIs("http://g.octopushq.com/WhyADSetupFails"));

            AddRule(r => r.ExceptionIs<HttpListenerException>(
                ex => // ex.ErrorCode == 0x80004005 && // Not sure now where this value came from, comparison always false because unsigned is too big for an int, value is -2147467259
                    ex.Message.StartsWith("The process cannot access the file because it is being used by another process") &&
                        ex.StackTrace.Contains("System.Net.HttpListener.Start()"))
                .EntrySummaryIs("The HTTP server failed to start because the port is in use.")
                .EntryHelpTextIs("The required port or URL prefix is being used by another process. The Windows `netstat -o -n -a` command " +
                    "can be used to show which process this is (compare PIDs with those shown in Task Manager).")
                .EntryHelpLinkIs("http://g.octopushq.com/HttpPortInUse"));

            AddRule(r => r.ExceptionIs<SocketException>(
                ex => ex.Message.Contains("Only one usage of each socket address (protocol/network address/port) is normally permitted"))
                .EntrySummaryIs("A required communications port is already in use.")
                .EntryHelpTextIs("The required port is being used by another process. The Windows `netstat -o -n -a` command " +
                    "can be used to show which process this is (compare PIDs with those shown in Task Manager).")
                .EntryHelpLinkIs("http://g.octopushq.com/HttpPortInUse"));

            AddRule(r => r.ExceptionIs<FileNotFoundException>(
                ex => ex.Message.Contains("Could not load file or assembly") &&
                    ex.Message.Contains("XmlSerializers") &&
                    ex.StackTrace.Contains(".CertificateGeneration.CryptContext.Open()"))
                .EntrySummaryIs("Crypto functions require the Windows User Profile")
                .EntryHelpTextIs("Various cryptographic functions used by Octopus Deploy require the Windows " +
                    "user profile to have been loaded. Some remote administration scenarios run commmands " +
                    "in processes without user profile information; to successfully run the problem command, " +
                    "invoke it from the command-line using RUNAS, e.g.: `runas /profile /user:<username> \"C:\\...\\Tentacle.exe new-certificate\"`.")
                .EntryHelpLinkIs("http://g.octopushq.com/CryptoRequiresUserProfile"));

            AddRule(r => r.ExceptionIs<UnauthorizedAccessException>(
                ex => ex.StackTrace.Contains(".CertificateGenerator.Generate("))
                .EntrySummaryIs("Crypto functions require the Windows User Profile")
                .EntryHelpTextIs("Various cryptographic functions used by Octopus Deploy require the Windows " +
                    "user profile to have been loaded. Some remote administration scenarios run commmands " +
                    "in processes without user profile information; to successfully run the problem command, " +
                    "invoke it from the command-line using RUNAS, e.g.: `runas /profile /user:<username> \"C:\\...\\Tentacle.exe new-certificate\"`.")
                .EntryHelpLinkIs("http://g.octopushq.com/CryptoRequiresUserProfile"));

            AddRule(r => r.ExceptionIs<ArgumentException>(
                ex => ex.Message.Contains("The user name (UPN) could not be determined for principal: Administrator"))
                .EntrySummaryIs("Some systems fail to correctly identify the 'Administrator' account")
                .EntryHelpTextIs("Using a domain account called 'Administrator' can resolve to the wrong principal" +
                    "on some systems; first, try qualifying the username with DOMAIN\\Administrator. If this" +
                    "issue persists, you may need to use an administrative account with a different name.")
                .EntryHelpLinkIs("http://g.octopushq.com/AdministratorAccountName"));
        }

        static void AddRule(Action<ExceptionKnowledgeBuilder> buildRule)
        {
            var builder = new ExceptionKnowledgeBuilder();
            buildRule(builder);
            Rules.Add(builder.Build());
        }

        public static bool TryInterpret(Exception exception, out ExceptionKnowledgeBaseEntry entry)
        {
            var unpacked = exception.UnpackFromContainers();
            try
            {
                foreach (var rule in Rules)
                {
                    if (rule.TryMatch(unpacked, out entry))
                        return true;
                }
            }
                // ReSharper disable once EmptyGeneralCatchClause
            catch
            {
            }

            entry = null;
            return false;
        }
    }
}