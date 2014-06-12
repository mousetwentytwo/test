﻿using System;

namespace Neurotoxin.Godspeed.Shell.Constants
{
    [Flags]
    public enum CopyAction
    {
        CreateNew = 0,
        Overwrite = 1,
        OverwriteOlder = 2,
        Resume = 4,
        Rename = 8
    }
}