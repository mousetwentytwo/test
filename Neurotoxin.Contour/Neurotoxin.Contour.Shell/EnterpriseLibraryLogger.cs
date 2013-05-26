using System;
using Microsoft.Practices.Composite.Logging;
using Microsoft.Practices.EnterpriseLibrary.Logging;

namespace Mti.Mnp.Client.Wpf.Shell
{
  public class EnterpriseLibraryLogger : ILoggerFacade
  {
    public void Log(string message, Category category, Priority priority)
    {
      Logger.Write(message, category.ToString(), (int)priority);
    }
  }
}
