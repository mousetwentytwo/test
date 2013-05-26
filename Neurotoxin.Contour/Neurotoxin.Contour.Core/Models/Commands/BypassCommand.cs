using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Neurotoxin.Contour.Core.Models.Commands
{
    public class BypassCommand
    {
        public static BypassCommand Instance { get; private set; }

        static BypassCommand()
        {
            Instance = new BypassCommand();
        }

        protected BypassCommand() {}
    }
}