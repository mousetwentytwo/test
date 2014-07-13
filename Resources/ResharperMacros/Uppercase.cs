using System.Collections.Generic;
using JetBrains.DocumentModel;
using JetBrains.ReSharper.Feature.Services.LiveTemplates.Hotspots;
using JetBrains.ReSharper.Feature.Services.LiveTemplates.Macros;

namespace ResharperMacros
{
	[Macro("CustomMacros.UppercaseAll",
	  ShortDescription = "Uppercase value of {#0:another variable}",
	  LongDescription = "Take the value of {#0:another variable} and uppercase all characters")]
    public class Uppercase : IMacro
    {
	    public string GetPlaceholder(IDocument document)
	    {
		    return "a";
	    }

	    public bool HandleExpansion(IHotspotContext context, IList<string> arguments)
	    {
		    return false;
	    }

	    public HotspotItems GetLookupItems(IHotspotContext context, IList<string> arguments)
	    {
		    return null;
	    }

	    public string EvaluateQuickResult(IHotspotContext context, IList<string> arguments)
	    {
			if (arguments.Count != 1 || arguments[0] == null) return null;

		    return arguments[0].ToUpper();
	    }

		public ParameterInfo[] Parameters { get { return new[] { new ParameterInfo(ParameterType.VariableReference) }; } }
    }
}