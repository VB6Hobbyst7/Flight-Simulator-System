using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ESRI.ArcGIS.Carto;
using ESRI.ArcGIS.Geometry;

namespace AirPlane
{
    public static class ElementsPropertise
    {

        static IElement _element2D;
        static IElement _element3D;

        static double _lengthFromStart2D;
        static double _lengthToEnd2D;

       // public static List<

       // public static IPoint StartPosition { get; set; }
        public static IPoint EndPosition { get; set; }

        public static IPoint CurrnetPosition { get; set; }

        public static double Elevation { get; set; }

        public static double Velocity { get; set; }

        public static IElement Element2D
        {
            get
            {
                return _element2D;
            }
            set
            {
                _element2D = value;
            }
        }

        public static IElement Elemnt3D
        {
            get
            {
                return _element3D;
            }
            set
            {
                _element3D = value;
            }
        }

        public static double LengthFromStart2D
        {
            get
            {
                return _lengthFromStart2D;
            }
            set
            {
                _lengthFromStart2D = value;
            }
        }

        public static double LengthToEnd2D
        {
            get
            {
                return _lengthToEnd2D;
            }
            set
            {
                _lengthToEnd2D = GetLengthtoEnd();
            }
        }

        private static double GetLengthtoEnd()
        {
            return 0.0;
        }
    }
}
