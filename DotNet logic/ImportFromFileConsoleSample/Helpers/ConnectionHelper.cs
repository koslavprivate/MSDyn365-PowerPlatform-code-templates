using Microsoft.Xrm.Sdk;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UDS.ImportFromFileConsoleSample.Helpers
{
    public static class ConnectionHelper
    {
        public static IOrganizationService InitializeService()
        {
            XrmToolingConnection xrmToolingConnection = new XrmToolingConnection();
            return xrmToolingConnection.Connect();
        }
    }
}
