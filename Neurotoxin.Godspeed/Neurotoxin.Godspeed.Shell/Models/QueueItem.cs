using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Neurotoxin.Godspeed.Shell.Constants;

namespace Neurotoxin.Godspeed.Shell.Models
{
    public class QueueItem
    {
        public FileSystemItem FileSystemItem { get; private set; }
        public TransferType TransferType { get; private set; }

        public QueueItem(FileSystemItem fileSystemItem, TransferType transferType)
        {
            FileSystemItem = fileSystemItem;
            TransferType = transferType;
        }
    }
}
