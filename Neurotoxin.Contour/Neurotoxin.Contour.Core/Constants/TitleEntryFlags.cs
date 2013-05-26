using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Neurotoxin.Contour.Core.Constants
{
    public enum TitleEntryFlags
    {
        NeedsToBeSynced = 0x1,
        ImageNeedsToBeDownloaded = 0x2,
        AvatarAwardNeedsToBeDownloaded = 0x10
    }
}
