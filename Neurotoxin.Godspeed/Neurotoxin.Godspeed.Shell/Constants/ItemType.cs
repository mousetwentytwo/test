using System;

namespace Neurotoxin.Godspeed.Shell.Constants
{
    [Flags]
    public enum ItemType
    {
        File = 0x1,
        Directory = 0x10,
        Drive = 0x100
    }
}