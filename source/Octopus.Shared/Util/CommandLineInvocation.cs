﻿using System;

namespace Octopus.Shared.Util
{
    public class CommandLineInvocation
    {
        readonly string executable;
        readonly string arguments;
        readonly string? systemArguments;

        public CommandLineInvocation(string executable, string arguments, string? systemArguments = null)
        {
            this.executable = executable;
            this.arguments = arguments;
            this.systemArguments = systemArguments;
        }

        public string Executable
        {
            get { return executable; }
        }

        public string Arguments
        {
            get { return arguments; }
        }

        // Arguments only used when we are invoking this directly from within the tools - not used when 
        // exporting the script for use later.
        public string? SystemArguments => systemArguments;

        public bool IgnoreFailedExitCode { get; set; }

        public override string ToString()
        {
            return "\"" + executable + "\" " + arguments;
        }
    }
}