using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ESRI.ArcGIS.Carto;
using ESRI.ArcGIS.GlobeCore;

namespace AirPlane
{
    class Synchronous
    {
        public IMap m_Mapglobe { get; set; }
        public IGlobeDisplay m_GlobeDisplay;
        public IGlobe m_Globe { get; set; }

        public void RefreshMaps(IMap pMap)
        {
            if (m_Mapglobe == null) return;
            m_GlobeDisplay = m_Globe.GlobeDisplay;
            for (int i = 0; i < pMap.LayerCount; i++)
            {
                //  m_Mapglobe.AddLayer(pMap.get_Layer(i));
                if (pMap.get_Layer(i) is IFeatureLayer)
                    AddVectorDataToGlobe(m_Globe, esriGlobeLayerType.esriGlobeLayerTypeDraped, pMap.get_Layer(i));
                //AddDrapeLayerToGlobeElevationSurface(m_GlobeDisplay, pMap.get_Layer(i), @"C:\Users\mgh\Desktop\AirPlane2\AirPlane\AirPlane\bin\Debug\GlobeData\wsiearth.tif");
                //Hieght(pMap.get_Layer(i));

            }

            // ChangeRasterize();
            // ChangeHeightProperties();

        }

        public void AddVectorDataToGlobe(ESRI.ArcGIS.GlobeCore.IGlobe globe, ESRI.ArcGIS.GlobeCore.esriGlobeLayerType globeLayerType, ESRI.ArcGIS.Carto.ILayer layer)
        {
            if (globe == null || layer == null || globeLayerType == ESRI.ArcGIS.GlobeCore.esriGlobeLayerType.esriGlobeLayerTypeElevation || globeLayerType == ESRI.ArcGIS.GlobeCore.esriGlobeLayerType.esriGlobeLayerTypeUnknown)
            {
                return;
            }

            if (!layer.Valid) return;

            ESRI.ArcGIS.GlobeCore.IGlobeDisplay globeDisplay = globe.GlobeDisplay;
            ESRI.ArcGIS.GlobeCore.IGlobeDisplay2 globeDisplay2 = globeDisplay as ESRI.ArcGIS.GlobeCore.IGlobeDisplay2; // Reference or Boxing Conversion
            globeDisplay2.PauseCaching = true;
            globe.AddLayerType(layer, globeLayerType, true);
            ESRI.ArcGIS.GlobeCore.IGlobeDisplayLayers globeDisplayLayers = globeDisplay as ESRI.ArcGIS.GlobeCore.IGlobeDisplayLayers; // Reference or Boxing Conversion
            ESRI.ArcGIS.GlobeCore.IGlobeLayerProperties globeLayerProperties = globeDisplayLayers.FindGlobeProperties(layer);
            globeLayerProperties.IsDynamicallyRasterized = false;
            globeDisplay2.PauseCaching = false;
        }

        public void RefreshLayers()
        {

        }

        public void AddDrapeLayerToGlobeElevationSurface(IGlobeDisplay globeDisplay, ILayer layer, System.String elevationRasterFilePath)
        {

            ChangeProp(layer);

            IGlobeDisplayLayers globeDisplayLayers = (IGlobeDisplayLayers)globeDisplay; // Explicit cast

            IFeatureLayer pFeatureLayer = (IFeatureLayer)layer;
            pFeatureLayer.Cached = true;

            // Create elevation raster layer
            IRasterLayer elevationRasterLayer = new RasterLayer();
            elevationRasterLayer.CreateFromFilePath(elevationRasterFilePath);

            // Create and add the GlobeLayerProperties extension with the Type set to ElevationData
            IGlobeLayerProperties globeLayerProperties = new GlobeLayerProperties();
            globeLayerProperties.Type = esriGlobeDataType.esriGlobeDataElevation;
            ILayerExtensions layerExtension = (ILayerExtensions)elevationRasterLayer; // Explicit cast
            layerExtension.AddExtension(globeLayerProperties);

            // Set the base option for layer to be esriGlobeLayerBaseLayer and its base layer to be ElevationLayer
            IGlobeLayerProperties drapeLayerGlobeLayerProperties = globeDisplayLayers.FindGlobeProperties(layer);
            IGlobeHeightProperties drapeLayerGlobeHeightProperties = drapeLayerGlobeLayerProperties.HeightProperties;
            //
            drapeLayerGlobeHeightProperties.BaseLayer = elevationRasterLayer;
            drapeLayerGlobeHeightProperties.BaseOption = esriGlobeLayerBaseOption.esriGlobeLayerBaseLayer;
            drapeLayerGlobeHeightProperties.ElevationExpressionString = "[Elevation] * 1000";
            // drapeLayerGlobeHeightProperties.ExtrusionType =  ESRI.ArcGIS.Analyst3D.esriExtrusionType.esriExtrusionBase;
            //drapeLayerGlobeHeightProperties.ExtrusionExpressionString = "Elevation * 1000";
            // Apply the height properties of the layer
            globeDisplayLayers.ApplyHeightProperties(layer);
            drapeLayerGlobeHeightProperties.Apply(m_Globe, layer);
            // globeDisplay.RefreshViewers();

            globeDisplayLayers.RefreshLayer(layer);
            IActiveView pacv = (IActiveView)m_Globe;
            pacv.PartialRefresh(esriViewDrawPhase.esriViewGeography, pFeatureLayer, null);
        }



        private void Hieght(ILayer pLayer)
        {

            ChangeProp(pLayer);

            IFeatureLayer pFL = (IFeatureLayer)pLayer;
            pFL.Cached = true;
            IGlobeDisplayLayers globeDisplayLayers = (IGlobeDisplayLayers)m_Globe.GlobeDisplay;
            IGlobeLayerProperties drapeLayerGlobeLayerProperties = globeDisplayLayers.FindGlobeProperties(pLayer);

            IGlobeHeightProperties drapeLayerGlobeHeightProperties = drapeLayerGlobeLayerProperties.HeightProperties;
            // drapeLayerGlobeHeightProperties.BaseLayer = elevationRasterLayer;
            // drapeLayerGlobeHeightProperties.BaseOption = esriGlobeLayerBaseOption.esriGlobeLayerBaseLayer;
            drapeLayerGlobeHeightProperties.ElevationExpressionString = "[Elevation] * 1000";
            //  drapeLayerGlobeHeightProperties.HasElevationValues = true;
            //drapeLayerGlobeHeightProperties.ElevationExpression.Expression = "Elevation * 1000";
            globeDisplayLayers.ApplyHeightProperties(pLayer);

            drapeLayerGlobeHeightProperties.Apply(m_Globe, pLayer);
            // m_globe.GlobeDisplay.RefreshViewers();

            // IViewers3D v3d = (IViewers3D)m_globe.GlobeDisplay;
            // v3d.RefreshViewers();
            globeDisplayLayers.RefreshLayer(pLayer);
            IActiveView pacv = (IActiveView)m_Globe;
            pacv.PartialRefresh(esriViewDrawPhase.esriViewGeography, pFL, null);

        }

        private void ChangeProp(ILayer pLayer)
        {

            IGlobeDisplayLayers globeDisplayLayers = (IGlobeDisplayLayers)m_Globe.GlobeDisplay;
            IGlobeLayerProperties drapeLayerGlobeLayerProperties = globeDisplayLayers.FindGlobeProperties(pLayer);
            // IGlobeLayerProperties drapeLayerGlobeLayerProperties = globeDisplayLayers.FindGlobeProperties(pLayer );
            //  IGlobeLayerProperties drapeLayerGlobeLayerProperties = GetGlobeLayerProperties((ILayer)featureLayer);
            ESRI.ArcGIS.GlobeCore.IGlobeDisplay2 globeDisplay2 = globeDisplayLayers as ESRI.ArcGIS.GlobeCore.IGlobeDisplay2;
            globeDisplay2.PauseCaching = true;
            drapeLayerGlobeLayerProperties.Scale3DSymbols = true;
            drapeLayerGlobeLayerProperties.IsDynamicallyRasterized = false;
            drapeLayerGlobeLayerProperties.Type = esriGlobeDataType.esriGlobeDataVector;
            globeDisplay2.PauseCaching = false;

        }

        private void ChangeHeightProperties()
        {
            try
            {
                ILayer pLayer = ((IBasicMap)m_Globe).get_Layer(1);
                IFeatureLayer pFL = pLayer as IFeatureLayer;
                pFL.Cached = true;
                IGlobeDisplayLayers globeDisplayLayers = (IGlobeDisplayLayers)m_Globe.GlobeDisplay;
                IGlobeLayerProperties drapeLayerGlobeLayerProperties = globeDisplayLayers.FindGlobeProperties(pLayer);

                IGlobeHeightProperties drapeLayerGlobeHeightProperties = drapeLayerGlobeLayerProperties.HeightProperties;
                // drapeLayerGlobeHeightProperties.BaseLayer = elevationRasterLayer;
                // drapeLayerGlobeHeightProperties.BaseOption = esriGlobeLayerBaseOption.esriGlobeLayerBaseLayer;
                drapeLayerGlobeHeightProperties.ElevationExpressionString = "[Elevation] * 10";
                //  drapeLayerGlobeHeightProperties.HasElevationValues = true;
                //drapeLayerGlobeHeightProperties.ElevationExpression.Expression = "Elevation * 1000";
                globeDisplayLayers.ApplyHeightProperties(pLayer);

                drapeLayerGlobeHeightProperties.Apply(m_Globe, pLayer);
                // m_globe.GlobeDisplay.RefreshViewers();


                // IViewers3D v3d = (IViewers3D)m_globe.GlobeDisplay;
                // v3d.RefreshViewers();
                globeDisplayLayers.RefreshLayer(pLayer);
                IActiveView pacv = (IActiveView)m_Globe;
                pacv.PartialRefresh(esriViewDrawPhase.esriViewGeography, pFL, null);
            }
            catch (Exception ex)
            {
                // MessageBox.Show(ex.Message);
            }
        }

        private void ChangeRasterize()
        {
            ILayer pLayer = ((IBasicMap)m_Globe).get_Layer(1);
            IGlobeDisplayLayers globeDisplayLayers = (IGlobeDisplayLayers)m_Globe.GlobeDisplay;
            IGlobeLayerProperties drapeLayerGlobeLayerProperties = globeDisplayLayers.FindGlobeProperties(pLayer);
            // IGlobeLayerProperties drapeLayerGlobeLayerProperties = globeDisplayLayers.FindGlobeProperties(pLayer );
            // IGlobeLayerProperties drapeLayerGlobeLayerProperties = GetGlobeLayerProperties((ILayer)featureLayer);
            ESRI.ArcGIS.GlobeCore.IGlobeDisplay2 globeDisplay2 = globeDisplayLayers as ESRI.ArcGIS.GlobeCore.IGlobeDisplay2;
            globeDisplay2.PauseCaching = true;
            drapeLayerGlobeLayerProperties.Scale3DSymbols = true;
            drapeLayerGlobeLayerProperties.IsDynamicallyRasterized = false;
            drapeLayerGlobeLayerProperties.Type = esriGlobeDataType.esriGlobeDataVector;
            globeDisplay2.PauseCaching = false;
        }

    }
}
