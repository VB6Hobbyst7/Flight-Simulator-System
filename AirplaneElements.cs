using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ESRI.ArcGIS.Carto;
using ESRI.ArcGIS.Geometry;
using ESRI.ArcGIS.Analyst3D;
using ESRI.ArcGIS.Display;
using ESRI.ArcGIS.GlobeCore;
using ESRI.ArcGIS.esriSystem;
using ESRI.ArcGIS.EngineCore;

namespace AirPlane
{
    public class AirplaneElements
    {
        private IElement _element;
        private IGlobeGraphicsElementProperties _elementProperties;
        private esriIPictureType _pictureType = esriIPictureType.esriIPicturePNG;
        public string fileName2DElement { get; set; }
        public string fileName3DElement { get; set; }

        public enum ElementMode
        {
            Element2D,
            Element3D
        }

        public AirplaneElements(IElement element, IGeometry elementGeometry, ICurve curvePath, ElementMode elementMode, double markerSize, double height)
        {
            this.fileName2DElement = @"C:\Users\mgh\Desktop\AirPlane2\Graphics\1.png";
            this.fileName3DElement = @"C:\Users\mgh\Desktop\AirPlane2\Graphics\11_2\13.3ds";

            if (elementGeometry.GeometryType == esriGeometryType.esriGeometryPoint)
            {
                if (elementMode == ElementMode.Element2D)
                    _element = Get2DElementAirplane(element, elementGeometry, curvePath, markerSize);
                else
                {
                    _element = Get3DElementAirplane(element, elementGeometry, curvePath, markerSize);
                    _elementProperties = Get3DElementProperties(height);
                }
            }

        }

        private IElement Get2DElementAirplane(IElement element, IGeometry elementGeometry, ICurve curvePath, double markerSize)
        {
            if (string.IsNullOrEmpty(fileName2DElement)) return null;

            IMarkerElement markerElement;
            // IElement element = new MarkerElement();
            markerElement = element as IMarkerElement;


            IColor rgbColor = GetRGBColor(255, 0, 0);

            IPictureMarkerSymbol pictureMarkerSymbol = new ESRI.ArcGIS.Display.PictureMarkerSymbol();
            pictureMarkerSymbol.CreateMarkerSymbolFromFile(_pictureType, fileName2DElement);
            pictureMarkerSymbol.Angle = GetAngle(curvePath);
            pictureMarkerSymbol.BitmapTransparencyColor = rgbColor;
            pictureMarkerSymbol.Size = markerSize;
            pictureMarkerSymbol.XOffset = 0;
            pictureMarkerSymbol.YOffset = 0;

        
            IGraphicTrackerSymbol gts = Elements2D.m_graphicTracker2D.CreateSymbol(pictureMarkerSymbol as ISymbol, null);
            GlobalValues.id2D = Elements2D.m_graphicTracker2D.Add(elementGeometry, gts);

            markerElement.Symbol = pictureMarkerSymbol;
            element.Geometry = (IPoint)elementGeometry;
            return element;
        }

        private IGlobeGraphicsElementProperties Get3DElementProperties(double height)
        {
            IGlobeGraphicsElementProperties globeGraphicsElementProperties;

            globeGraphicsElementProperties = new GlobeGraphicsElementProperties();
            globeGraphicsElementProperties.DrapeElement = true;
            globeGraphicsElementProperties.OrientationMode = esriGlobeGraphicsOrientation.esriGlobeGraphicsOrientationLocal;
            globeGraphicsElementProperties.DrapeZOffset = height;
            globeGraphicsElementProperties.FixedScreenSize = false;
            globeGraphicsElementProperties.Illuminate = false;

            return globeGraphicsElementProperties;
        }

        private IElement Get3DElementAirplane(IElement element, IGeometry elementGeometry, ICurve curvePath, double markerSize)
        {
            if (string.IsNullOrEmpty(fileName3DElement)) return null;

            // IElement element = new MarkerElement();

            IMarker3DSymbol pMarker3DSymbol = new Marker3DSymbol();

            pMarker3DSymbol.CreateFromFile(fileName3DElement);
            pMarker3DSymbol.UseMaterialDraping = true;
            //pMarker3DSymbol.
            IMarker3DPlacement pM3DP = (IMarker3DPlacement)pMarker3DSymbol;

            // pM3DP.Width =  markerSize ;
            pM3DP.Units = esriUnits.esriMeters;
            pM3DP.Angle = GetAngle(curvePath);
            pM3DP.MaintainAspectRatio = true;
            pM3DP.Size = markerSize;
            //pM3DP.Width = 83;
            //pM3DP.Depth = 73;


            IGraphicTrackerSymbol gts = Elements2D.m_graphicTracker3D.CreateSymbol(null , pMarker3DSymbol as ISymbol);
            GlobalValues.id3D = Elements2D.m_graphicTracker3D.Add(elementGeometry, gts);


            ((IMarkerElement)element).Symbol = (IMarkerSymbol)pMarker3DSymbol;

            element.Geometry = (IPoint)elementGeometry;
            return element;

        }

        private double GetAngle(ICurve curvePath)
        {
            IPolyline pPoly = (IPolyline)curvePath;
            ISegmentCollection pSG = pPoly as ISegmentCollection;
            ILine pLine = pSG.get_Segment(0) as ILine;
            return Math.Round((180 * pLine.Angle) / Math.PI, 3);
        }

        private double GetResolutionQuality()
        {
            const double HighQuality = 1.0;

            return HighQuality;
        }

        private void SetMarker3DPlacement(IMarker3DPlacement marker3DPlacement, double size)
        {
            const double XOffset = 0;
            const double YOffset = 0;

            marker3DPlacement.XOffset = XOffset;
            marker3DPlacement.YOffset = YOffset;
            marker3DPlacement.ZOffset = size / 2;
        }


        private IRgbColor GetRGBColor(int red, int green, int blue)
        {
            //Create rgb color and grab hold of the IRGBColor interface
            IColor pColor = new RgbColor();
            IRgbColor pRGB = pColor as IRgbColor;
            //Set rgb color properties
            pRGB.Red = red;
            pRGB.Green = green;
            pRGB.Blue = blue;
            pRGB.UseWindowsDithering = true;
            return pRGB;
        }

        public IElement Element
        {
            get
            {
                return _element;
            }

        }
        public IGlobeGraphicsElementProperties GlobeGraphicsElementProperties
        {
            get
            {
                return _elementProperties;
            }
        }

        public esriIPictureType PictureType
        {

            set
            {
                _pictureType = value;
            }
        }

    }
}
