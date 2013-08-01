﻿using System;

namespace Octopus.Shared.Platform.FileTransfer
{
    public class SendFileResult : ResultMessage
    {
        public string DestinationPath { get; private set; }

        public SendFileResult(bool wasSuccessful, string details, string destinationPath)
            : base(wasSuccessful, details)
        {
            DestinationPath = destinationPath;
        }
    }
}
