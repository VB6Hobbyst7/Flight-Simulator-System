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

namespace AirPlane
{
    public class PathElements
    {
        private IElement _element;
        private IGlobeGraphicsElementProperties _elementProperties;

        public enum ElementMode
        {
            Element2D,
            Element3D
        }

        public PathElements(IElement element, IGeometry elementGeometry, ElementMode elementMode, double height)
        {

            if (elementGeometry.GeometryType == esriGeometryType.esriGeometryPolyline)
            {
                if (elementMode == ElementMode.Element2D)
                    _element = Get2DElementTemporaryPath(element, elementGeometry);
                else
                {
                    _element = Get3DElementTemporaryPath(element, elementGeometry);
                    _elementProperties = Get3DElementProperties(height);
                }
            }
        }

        private IElement Get2DElementTemporaryPath(IElement element, IGeometry elementGeometry)
        {

            ISimpleLineSymbol simpleLineSymbol = new SimpleLineSymbol();
            simpleLineSymbol.Style = esriSimpleLineStyle.esriSLSDash;

            ILineSymbol linesymbol = (ILineSymbol)simpleLineSymbol;

            IRgbColor colorElement = GetRGBColor(0, 0, 0);
            linesymbol.Color = colorElement;
            linesymbol.Width = 1;


            ((ILineElement)element).Symbol = (ILineSymbol)simpleLineSymbol;

            element.Geometry = elementGeometry;
            return element;

        }

           private IGlobeGraphicsElementProperties Get3DElementProperties(double height)
        {
            IGlobeGraphicsElementProperties globeGraphicsElementProperties;

            globeGraphicsElementProperties = new GlobeGraphicsElementProperties();
            globeGraphicsElementProperties.DrapeElement = true;
            globeGraphicsElementProperties.OrientationMode = esriGlobeGraphicsOrientation.esriGlobeGraphicsOrientationDefault;
            globeGraphicsElementProperties.DrapeZOffset = height;

            return globeGraphicsElementProperties;
        }

        private IElement Get3DElementTemporaryPath(IElement element, IGeometry elementGeometry)
        {
           
            ISimpleLine3DSymbol simpleLine3DSymbol = new SimpleLine3DSymbol();
            simpleLine3DSymbol.Style = esriSimple3DLineStyle.esriS3DLSTube;

            ILineSymbol linesymbol = (ILineSymbol)simpleLine3DSymbol;

            IRgbColor colorElement = GetRGBColor(0, 0, 0);
            linesymbol.Color = colorElement;
            linesymbol.Width = 1;


            ((ILineElement)element).Symbol = (ILineSymbol)simpleLine3DSymbol;

            element.Geometry = elementGeometry;
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

     }
}
