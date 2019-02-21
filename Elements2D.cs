using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ESRI.ArcGIS.Geometry;
using ESRI.ArcGIS.Carto;
using ESRI.ArcGIS.EngineCore;

namespace AirPlane
{
    public class Elements2D
    {

        #region "Fields"

        private IElement _element2D;
        private IElement _element3D;
        private string _name;
        private double _distanceAlongCurve;
        private double _distanceFromCurve;
        private IPoint _startPosition;
        private double _elevation;
        private double _velocity;
        private ICurve _Path;
        private States state = States.Off;
        private static List<Elements2D> _elements2Ds = new List<Elements2D>();
        public static IGraphicTracker m_graphicTracker2D;
        public static IGraphicTracker m_graphicTracker3D;
        private double _angle;
        private bool _showInformation = false;

        #endregion

        public static void InitializeGraphicTracker(object map , object globe)
        {
            m_graphicTracker2D = new GraphicTrackerClass();
            m_graphicTracker2D.Initialize(map);

            m_graphicTracker3D = new GraphicTrackerClass();
            m_graphicTracker3D.Initialize(globe);
        }

        #region "Enums"
        public enum States
        {
            On,
            Off
        }
        #endregion

        #region "Properties"

        public IElement Element3D
        {
            get { return _element3D; }
            set { _element3D = value; }
        }

        public IElement Element2D
        {
            get { return _element2D; }
            set { _element2D = value; }
        }

        public string Name
        {
            get { return _name; }
            set { _name = value; }
        }

        public ICurve Path
        {
            get { return _Path; }
            set { _Path = value; }
        }

        public double Elevation
        {
            get { return _elevation; }
            set { _elevation = value; }
        }

        public double Velocity
        {
            get { return _velocity; }
            set { _velocity = value; }
        }

        public IPoint StartPosition
        {
            get { return _startPosition; }
            set { _startPosition = value; }
        }

        public double DistanceFromCurve
        {
            get { return _distanceFromCurve; }
            set { _distanceFromCurve = value; }
        }

        public double DistanceAlongCurve
        {
            get
            {
                return _distanceAlongCurve;
            }
            set
            {
                _distanceAlongCurve = value;
            }
        }

        public bool ShowInformation
        {
            get { return _showInformation; }
            set { _showInformation = value; }
        }

        public double Angle
        {
            get { return _angle; }
            set { _angle = value; }
        }

        public States State
        {
            get { return state; }
            set { state = value; }
        }

        public static List<Elements2D> Elements2Ds
        {
            get { return Elements2D._elements2Ds; }
            set { Elements2D._elements2Ds = value; }
        }
        #endregion


    }
}
