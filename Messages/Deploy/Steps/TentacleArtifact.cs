﻿using System;

namespace Octopus.Shared.Messages.Deploy.Steps
{
    public class TentacleArtifact
    {
        public TentacleArtifact(string path, string originalFilename)
        {
            Path = path;
            OriginalFilename = originalFilename;
        }

        public string Path { get; private set; }
        public string OriginalFilename { get; private set; }
    }
}