using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ESRI.ArcGIS.Carto;
using ESRI.ArcGIS.Geometry;
using ESRI.ArcGIS.Geodatabase;

namespace AirPlane
{
    static  class Path
    {
        private static IFeature _selectedFeature;
        private static ICurve _path;
        private static IMap _map;
        private static ILayer _layer;
        private static IPoint _ToPoint;
        private static IPoint _FromPoint;

        public static IMap  Map
        {
            get
            {
                return _map;
            }
             set
            {
                _map = value;
            }
        }
        public static ICurve SelectedPath
        {
            get
            {
                return _path;
            }
            private set
            {
                _path = value;
            }
        }

        public static IFeature SelectedFeature
        {
            get
            {
                return  _selectedFeature ;
            }
            private set
            {
                _selectedFeature = value;
            }
        }

        public static ILayer Layer
        {
            get
            {
                return _layer;
            }
            set
            {
                _layer = value;
            }
        }

        public static IPoint ToPoint
        {
            get
            {
                return _ToPoint;
            }
            set
            {
                _ToPoint = value;
            }
        }

        public static IPoint FromPoint
        {
            get
            {
                return _FromPoint;
            }
            set
            {
                _FromPoint = value;
            }
        }

      
        public static  void   Get_Path()
        {
            IEnumFeature enumFeature;
            IFeature selectedFeature = null;

            enumFeature = Map.FeatureSelection as IEnumFeature;
            enumFeature.Reset();

            selectedFeature = enumFeature.Next();

            while (selectedFeature != null)
            {
                if (selectedFeature.ShapeCopy.GeometryType == esriGeometryType.esriGeometryPolyline)
                {
                    _layer = GetLayer(selectedFeature);
                    break;
                }
            }

            if (selectedFeature == null) return ;
            _selectedFeature = selectedFeature;
            _path  = selectedFeature.ShapeCopy as ICurve;

        }

        public static  ILayer GetLayer(IFeature selectedFeature)
        {
            ILayer layer =null;
            IObjectClass objClass = selectedFeature.Class;
            IFeatureClass featureClass = objClass as IFeatureClass;
            if (featureClass != null)
            {
                IFeatureLayer featureLayer = new FeatureLayerClass();
                featureLayer.FeatureClass = featureClass;
                layer = (ILayer)featureLayer; 
            }
            return layer;
        }

        public static double DistanceAlongPath(double velocity, int inputGum)
        {
            if (_path == null) return 0.0;

            double lengthForSecond = velocity / 3.6;
            return (lengthForSecond / 1000) * inputGum;
        }

        public static  IPoint   GetToPoint()
        {
            IPoint toPoint = null;
            if (_path == null) return null ;
             toPoint  = _path.ToPoint;
             return toPoint;

        }

        public static  IPoint GetFromPoint()
        {
            IPoint fromPoint = null;
            if (_path == null) return null;
            fromPoint = _path.FromPoint ;
            return fromPoint;

        }
    }
}
