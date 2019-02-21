using System;
using ESRI.ArcGIS;
using ESRI.ArcGIS.esriSystem;

namespace AirPlane
{
    internal partial class LicenseInitializer
    {
        public LicenseInitializer()
        {
            ResolveBindingEvent += new EventHandler(BindingArcGISRuntime);


            //if (!InitializeEngineLicense())
            //{
            //    System.Windows.Forms.MessageBox.Show("ArcGIS Engine License or Globe Extension could not be initialized.  Closing...");
            //    System.Environment.Exit(0);
            //}
        }

        private bool InitializeEngineLicense()
        {
            AoInitialize aoi = new AoInitializeClass();

            //more license choices could be included here
            esriLicenseProductCode productCode = esriLicenseProductCode.esriLicenseProductCodeEngine;
            esriLicenseExtensionCode extensionCode = esriLicenseExtensionCode.esriLicenseExtensionCode3DAnalyst;

            if (aoi.IsProductCodeAvailable(productCode) == esriLicenseStatus.esriLicenseAvailable && aoi.IsExtensionCodeAvailable(productCode, extensionCode) == esriLicenseStatus.esriLicenseAvailable)
            {
                aoi.Initialize(productCode);
                aoi.CheckOutExtension(extensionCode);
                return true;
            }
            else
                return false;
        }

        void BindingArcGISRuntime(object sender, EventArgs e)
        {
            //
            // TODO: Modify ArcGIS runtime binding code as needed
            //
            if (!RuntimeManager.Bind(ProductCode.Engine))
            {
                // Failed to bind, announce and force exit
                System.Windows.Forms.MessageBox.Show("Invalid ArcGIS runtime binding. Application will shut down.");
                System.Environment.Exit(0);
            }
        }
    }
}