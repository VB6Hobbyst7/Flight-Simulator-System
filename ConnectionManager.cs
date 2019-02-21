using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Configuration;
using ESRI.ArcGIS.esriSystem;
using ESRI.ArcGIS.Geodatabase;

namespace AirPlane
{
    static class ConnectionManager
    {
  
        private static  string _message;
        private static IWorkspace _workspace;

        public static string Message
        {
            get
            {
                return _message;
            }
        }

        public static  IWorkspace Worksapce
        {
            get
            {
                _workspace = ConnectToTransactionalVersion(@".", @"sde:sqlserver:MGHPC", "sa", "1234", "AirPlane", "DBO.DEFAULT", "DBMS");
                return _workspace;
            }
        }

        private static  IWorkspace ConnectToTransactionalVersion(String server, String instance, String user, String password, String database, String version, string authenticationMode)
        {
            IWorkspace workspace = null;
            try
            {
                IPropertySet propertySet = new PropertySetClass();
                propertySet.SetProperty("SERVER", server);
                propertySet.SetProperty("INSTANCE", instance);
                propertySet.SetProperty("DATABASE", database);
                propertySet.SetProperty("USER", user);
                propertySet.SetProperty("PASSWORD", password);
                propertySet.SetProperty("AUTHENTICATION_MODE", authenticationMode);
                propertySet.SetProperty("VERSION", version);


                Type factoryType = Type.GetTypeFromProgID("esriDataSourcesGDB.SdeWorkspaceFactory");
                IWorkspaceFactory workspaceFactory = (IWorkspaceFactory)Activator.CreateInstance
                    (factoryType);

                workspace = workspaceFactory.Open(propertySet, 0);
            }
            catch
            {
                _message = "عدم توانایی در برقراری ارتباط با پایگاه داده مکانی";
                workspace = null;
            }
            return workspace;
        }
    }
}
