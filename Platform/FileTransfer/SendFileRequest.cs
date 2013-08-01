﻿using System;
using Pipefish;

namespace Octopus.Shared.Platform.FileTransfer
{
    public class SendFileRequest : IMessage
    {
        public string LocalFilename { get; private set; }
        public string RemoteSquid { get; private set; }
        public long ExpectedSize { get; private set; }
        public string Hash { get; private set; }

        public SendFileRequest(string localFilename, string remoteSquid, long expectedSize, string hash)
        {
            LocalFilename = localFilename;
            RemoteSquid = remoteSquid;
            ExpectedSize = expectedSize;
            Hash = hash;
        }
    }
}
