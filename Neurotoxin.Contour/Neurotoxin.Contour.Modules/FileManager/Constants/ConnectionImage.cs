using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Neurotoxin.Contour.Core.Attributes;

namespace Neurotoxin.Contour.Modules.FileManager.Constants
{
    public enum ConnectionImage
    {
        [StringValue("Xbox360 (Fat/White)")]
        Fat,
        [StringValue("Xbox360 Elite (Fat/Black)")]
        FatElite,
        [StringValue("Xbox360 S (Slim)")]
        Slim,
        [StringValue("Xbox360 E (Slim E)")]
        SlimE
    }
}