using ESRI.ArcGIS.DataSourcesRaster;
using ESRI.ArcGIS.Analyst3D;
using ESRI.ArcGIS.esriSystem;
using ESRI.ArcGIS.Display;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using DevComponents.DotNetBar;
using ESRI.ArcGIS.Controls;
using ESRI.ArcGIS.Carto;
using ESRI.ArcGIS.Geometry;
using ESRI.ArcGIS.Geodatabase;
using ESRI.ArcGIS.GlobeCore;
using DevComponents.Instrumentation;
using ESRI.ArcGIS.DataSourcesFile;
using AirPlane.Tools.Elements;
using AirPlane.Tools.AddRasterData;
using GlobeDynamicObjectTracking;


namespace AirPlane
{
    public partial class frmMain : DevComponents.DotNetBar.OfficeForm
    {
        #region "Global Variables"

        private Synchronous m_Synchronous;
        private IMap m_MapMapControl;
        private IMap m_MapGlobeControl;
        private IActiveView m_ActiveViewMapControl;
        private IActiveView m_ActiveViewGlobeControl;

        private IElement element;

        private const string GraphicsAirPalnesLayerName = "Globe Graphics AirPalnes";
        private const string GraphicsAirlinesLayerName = "Globe Graphics Airlines";
        private ICurve curvePath;
        private IGraphicsContainer GCon_MapControl;

        private Layer graphicsAirplanesLayer = null;
        private Layer graphicsAirlinesLayer = null;
        private List<Timers> timers;

        private double distanceAlongPath = 0;
        private double lengthForGum;

        #endregion

        #region "Form"

        public frmMain()
        {
            InitializeComponent();
        }

        private void frmMain_Load(object sender, EventArgs e)
        {
            try
            {

                Initialize();


            }
            catch (Exception ex)
            {

                lblStatus.Text = "اشکال در بارگذاری سیستم";
            }


        }

        private void Initialize()
        {
            TOCControl.SetBuddyControl(MapControl.Object);
            m_Synchronous = new Synchronous();
            m_MapGlobeControl = GlobeControl.Globe as IMap;
            if (m_MapGlobeControl == null) return;

            // Add Raster Layer
            IRasterLayer pRas = new ESRI.ArcGIS.Carto.RasterLayer();
            pRas.CreateFromFilePath(@"C:\Users\mgh\Desktop\AirPlane2\AirPlane\AirPlane\bin\Debug\GlobeData\wsiearth.tif");

            GlobeControl.Globe.AddLayerType(pRas, esriGlobeLayerType.esriGlobeLayerTypeDraped, true);

            // Add Graphic Layer
            graphicsAirplanesLayer = AddGraphicLayers(GraphicsAirPalnesLayerName);
            graphicsAirlinesLayer = AddGraphicLayers(GraphicsAirlinesLayerName);

            m_Synchronous.m_Mapglobe = m_MapGlobeControl;
            m_Synchronous.m_Globe = GlobeControl.Globe;
            GCon_MapControl = m_MapMapControl as IGraphicsContainer;
            m_ActiveViewGlobeControl = m_MapGlobeControl as IActiveView;
            m_ActiveViewGlobeControl.Extent = m_ActiveViewGlobeControl.FullExtent;
            m_ActiveViewGlobeControl.Refresh();
        }

        private Layer AddGraphicLayers(string graphicLayerName)
        {
            TableOfContents tableOfContents = new TableOfContents(GlobeControl.Globe);

            if (!tableOfContents.LayerExists(graphicLayerName))
            {
                tableOfContents.ConstructLayer(graphicLayerName);
            }

            return new Layer(tableOfContents[graphicLayerName]);
        }

        private void frmMain_FormClosing(object sender, FormClosingEventArgs e)
        {
            GlobeControl.Dispose();
            MapControl.Dispose();
            TOCControl.Dispose();
            ESRI.ArcGIS.ADF.COMSupport.AOUninitialize.Shutdown();
        }
        #endregion

        #region "MapControl"

        private void MapControl_OnMouseMove(object sender, IMapControlEvents2_OnMouseMoveEvent e)
        {
            lblCoordinate.Text = String.Format("{0}     {1}  {2}", e.mapX.ToString("#######.###"), e.mapY.ToString("#######.###"), MapControl.MapUnits.ToString().Substring(4));
            lblSpatialReference.Text = "WGS_1984_UTM_Zone_39N";
        }

        private void MapControl_OnExtentUpdated(object sender, IMapControlEvents2_OnExtentUpdatedEvent e)
        {

            Update_ActiveView();
        }

        private void MapControl_OnMapReplaced(object sender, IMapControlEvents2_OnMapReplacedEvent e)
        {
            m_MapMapControl = MapControl.Map;
            if (m_MapMapControl == null) return;

            m_ActiveViewMapControl = MapControl.ActiveView;

            m_Synchronous.RefreshMaps(m_MapMapControl);

            // Add Graphic Layer
            graphicsAirplanesLayer = AddGraphicLayers(GraphicsAirPalnesLayerName);
            graphicsAirlinesLayer = AddGraphicLayers(GraphicsAirlinesLayerName);

            Update_ActiveView();

        }

        private void Update_ActiveView()
        {
            try
            {

                m_ActiveViewGlobeControl = GlobeControl.Globe as IActiveView;
                IEnvelope pEnvelope = MapControl.Extent;
                pEnvelope.Expand(0.5, 0.5, true);
                m_ActiveViewGlobeControl.Extent = pEnvelope;
                m_ActiveViewGlobeControl.Refresh();

            }
            catch
            {

            }

        }

        #endregion

        EnumPlayStopAirplane enumAiplaneState = EnumPlayStopAirplane.StopAirPlane;
        private enum EnumPlayStopAirplane
        {
            PlayAirPlane,
            StopAirPlane
        }

        private void btnPlayStopAirplane_Click(object sender, EventArgs e)
        {
            double velocity;

            lblStatus.Text = "";
            velocity = Set_GaugeControl();

            if (velocity == 0)
            {
                ShowMessages("ابتدا سرعت مد نظر را وارد نمایید");
                return;
            }

            ElementsPropertise.Velocity = velocity;

            element = new MarkerElement();

            if (Path.SelectedPath == null)
            {
                ShowMessages("لطفاً ابتدا مسیر مورد نظر را انتخاب نمایید");
                return;
            }
            curvePath = Path.SelectedPath;
            lengthForGum = Path.DistanceAlongPath(velocity, intInputGum.Value);

            CalcStaticsAirplanes(velocity);
            ChangeAirplaneState();

            timer_Airlpanes.Interval = intInputGum.Value;
            // timer_Airlpanes.Enabled = true;
            // AddTimer();
        }

        private void CalcStaticsAirplanes(double velocity)
        {
            foreach (var element in Elements2D.Elements2Ds)
            {
                double distanceFromCurve = -1;
                double distanceAlogCurve = -1;
                ICurve pathElement;

                bool isRightSide = false;

                QueryPointAndDistance(element.StartPosition, ref distanceAlogCurve, ref distanceFromCurve, ref isRightSide);

                pathElement = ConstructOffset(curvePath as IPolyline, distanceFromCurve, isRightSide, element.Elevation);

                element.Velocity = velocity;
                element.DistanceAlongCurve = distanceAlogCurve;
                element.DistanceFromCurve = distanceFromCurve;
                element.Path = pathElement;
                element.State = Elements2D.States.On;

            }
        }

        private void ChangeAirplaneState()
        {
            if (enumAiplaneState == EnumPlayStopAirplane.PlayAirPlane)
            {
                enumAiplaneState = EnumPlayStopAirplane.StopAirPlane;
                btnPlayStopAirplane.Text = "شروع";
                btnPlayStopAirplane.Image = Properties.Resources.GenericBluePlayCircleTime16;
                timer_Airlpanes.Enabled = false;
            }
            else
            {
                enumAiplaneState = EnumPlayStopAirplane.PlayAirPlane;
                btnPlayStopAirplane.Text = "مکث";
                btnPlayStopAirplane.Image = Properties.Resources.GenericBluePauseCircle16;
                timer_Airlpanes.Enabled = true;
            }
        }

        //private void AddTimer()
        //{
        //    if (timers == null)
        //        timers = new List<Timers>();
        //    Timers timer = new Timers();
        //    timer.interval = intInputGum.Value;
        //    timer.Enabled = true;
        //    timers.Add(timer);
        //    timer.Tick += new EventHandler(timer_Tick);
        //}

        void timer_Tick(object sender, EventArgs e)
        {

            foreach (var airplaneElement in Elements2D.Elements2Ds)
            {

                IPoint newPosition = new ESRI.ArcGIS.Geometry.Point();

                ICurve path = airplaneElement.Path;
                distanceAlongPath = airplaneElement.DistanceAlongCurve;

                distanceAlongPath += lengthForGum;

                path.QueryPoint(esriSegmentExtension.esriNoExtension, distanceAlongPath, false, newPosition);

                if (distanceAlongPath > path.Length)
                {
                    airplaneElement.State = Elements2D.States.Off;
                    if (!CheckAirplanesStates())
                        timer_Airlpanes.Enabled = false;
                    else
                        continue;
                }

                double newAngle = GetAngle(path, distanceAlongPath);

               // MoveElements(airplaneElement.Element2D, airplaneElement.Element3D, newPosition);
                Elements2D.m_graphicTracker2D.MoveTo(GlobalValues.id2D, newPosition.X, newPosition.Y, 0);
                Elements2D.m_graphicTracker3D.MoveTo(GlobalValues.id3D, newPosition.X, newPosition.Y, 0);
                if (airplaneElement.Angle != newAngle)
                {

                    ReAngleElements(airplaneElement.Element2D, airplaneElement.Element3D, newAngle);
                    airplaneElement.Angle = newAngle;
                }
                UpdateViewers(airplaneElement.Velocity, airplaneElement.Elevation, path, distanceAlongPath, airplaneElement.StartPosition, newPosition);

                airplaneElement.DistanceAlongCurve = distanceAlongPath;

            }

            m_ActiveViewMapControl.PartialRefresh(esriViewDrawPhase.esriViewGraphics, null, null);

            graphicsAirplanesLayer.UpdateAllElement();

            GlobeControl.Globe.GlobeDisplay.RefreshViewers();
            
        }

        private bool CheckAirplanesStates()
        {
            bool states = false;
            foreach (var airplane in Elements2D.Elements2Ds)
            {
                if (airplane.State == Elements2D.States.On)
                {
                    states = true;
                    break;
                }
            }
            return states;
        }

        private void ReAngleElements(IElement element2D, IElement element3D, double newAngle)
        {

            if (element2D != null)
            {
                IMarkerElement markerElement;
                markerElement = element2D as IMarkerElement;
                IPictureMarkerSymbol pictureMarkerSymbol = (IPictureMarkerSymbol)markerElement.Symbol;

                pictureMarkerSymbol.Angle = newAngle;

                markerElement.Symbol = pictureMarkerSymbol;
            }

            if (element3D != null)
            {
                IMarkerElement markerElement;
                markerElement = element3D as IMarkerElement;
                IMarker3DSymbol pMarker3DSymbol = (IMarker3DSymbol)markerElement.Symbol;
                IMarker3DPlacement pM3DP = (IMarker3DPlacement)pMarker3DSymbol;
                pM3DP.Angle = newAngle;
                markerElement.Symbol = (ESRI.ArcGIS.Display.IMarkerSymbol)pMarker3DSymbol;
            }

        }

        private double GetAngle(ICurve curvePath, double distanceAlongPath)
        {
            ILine pline = new Line();
            curvePath.QueryTangent(esriSegmentExtension.esriNoExtension, distanceAlongPath, false, distanceAlongPath, pline);
            double angle = (180 * pline.Angle) / Math.PI;
            return angle;
        }



        private ICurve ConstructOffset(IPolyline inPolyline, double distanceFromCurve, bool isRightSide, double elevation)
        {
            if (inPolyline == null || inPolyline.IsEmpty)
            {
                return null;
            }
            object Missing = Type.Missing;
            IPolyline pPoly = new ESRI.ArcGIS.Geometry.Polyline() as IPolyline;
            IConstructCurve constructCurve = (IConstructCurve)pPoly;

            if (!isRightSide)
                distanceFromCurve = -distanceFromCurve;
            constructCurve.ConstructOffset(inPolyline, distanceFromCurve, ref Missing, ref Missing);


            IRgbColor rgbColor = GetColor(0, 0, 0);
            AddTempolralPathToMap(m_MapMapControl, constructCurve as IGeometry, rgbColor);
            AddTempolralPathToGlobe(constructCurve as IGeometry, elevation);
            return constructCurve as ICurve;
        }

        private void AddTempolralPathToGlobe(IGeometry elementGeometry, double elevation)
        {
            IElement element = null;
            double elevationElement = elevation;
            element = new LineElementClass();
            PathElements pathElement = new PathElements(element, elementGeometry, PathElements.ElementMode.Element3D, elevationElement);
            element = pathElement.Element;
            if (element == null) return;

            graphicsAirlinesLayer.AddElement(pathElement.Element, pathElement.GlobeGraphicsElementProperties);
            GlobeControl.Globe.GlobeDisplay.RefreshViewers();
        }

        private IRgbColor GetColor(int red, int green, int blue)
        {
            IRgbColor rgb = new RgbColor();
            rgb.Red = red;
            rgb.Green = green;
            rgb.Blue = blue;
            return rgb;
        }

        public void AddTempolralPathToMap(IMap map, IGeometry geometry, IRgbColor rgbColor)
        {
            IGraphicsContainer graphicsContainer = (IGraphicsContainer)map; // Explicit Cast
            IElement element = null;
            if ((geometry.GeometryType) == esriGeometryType.esriGeometryPolyline)
            {
                //  Line elements
                ISimpleLineSymbol simpleLineSymbol = new SimpleLineSymbolClass();
                simpleLineSymbol.Color = rgbColor;
                simpleLineSymbol.Style = esriSimpleLineStyle.esriSLSDash;
                simpleLineSymbol.Width = 1;

                ILineElement lineElement = new LineElementClass();
                lineElement.Symbol = simpleLineSymbol;
                element = (IElement)lineElement; // Explicit Cast

            }
            if (!(element == null))
            {
                element.Geometry = geometry;
                graphicsContainer.AddElement(element, 0);
            }
        }


        //private ICurve ExtendCurveToEnd(ICurve curve)
        //{
        //    if (curve == null || curve.IsEmpty)
        //        return null;
        //    ISegmentCollection sCollection = (ISegmentCollection)curve;
        //    ISegment segmentEnd = sCollection.get_Segment(sCollection.SegmentCount - 1);
        //    ILine inLine = ( ILine ) segmentEnd ;
        //    ILine line = new  ESRI .ArcGIS.Geometry.Line ();
        //    IConstructLine contructLine = ( IConstructLine) line ;
        //    contructLine.ConstructExtended( inLine ,  esriSegmentExtension.esriExtendEmbeddedAtTo);
        //    line.ConstructExtended(
        //}

        private bool QueryPointAndDistance(IPoint startPosition, ref double distanceAlogCurve, ref double distanceFromCurve, ref bool isRightSide)
        {
            bool result = false;
            try
            {
                IPoint fromPoint = startPosition;
                IPoint nearestPoint = new ESRI.ArcGIS.Geometry.Point();

                curvePath.QueryPointAndDistance(esriSegmentExtension.esriNoExtension, fromPoint, false, nearestPoint, ref distanceAlogCurve, ref distanceFromCurve, ref  isRightSide);
                result = true;
            }
            catch
            {
                result = false;

            }
            return result;
        }

        private void UpdateViewers(double velocity, double elevation, ICurve path, double distanceAlongPath, IPoint startPosition, IPoint CurrentPosition)
        {
            if (string.IsNullOrEmpty(lblAirplaneName.Text))
                lblAirplaneName.Text = "AirPlane";
            lblAirplaneVelocity.Text = velocity.ToString();
            lblAirplaneElevation.Text = elevation.ToString();
            lblAirplaneOrigin.Text = string.Format("X: {0} , Y: {1}", Math.Round(startPosition.X, 3), Math.Round(startPosition.Y, 3));
            lblAirplaneDestination.Text = string.Format("X: {0} , Y: {1}", Math.Round(path.ToPoint.X, 3), Math.Round(path.ToPoint.Y, 3));

            lblAirplaneXCurrent.Text = Math.Round(CurrentPosition.X, 4).ToString();
            lblAirplaneYCurrent.Text = Math.Round(CurrentPosition.Y, 4).ToString();

            lblLengthPath.Text = Math.Round(path.Length, 4).ToString();
            lblLengthFromPath.Text = Math.Round(distanceAlongPath, 4).ToString();
            lblLengthToPath.Text = Math.Round((path.Length - distanceAlongPath), 4).ToString();
        }

        private void MoveElements(IElement element2D, IElement element3D, IPoint newPosition)
        {
            ITransform2D transform2D;
            IPoint currentPosition;
            if (element2D != null)
            {
                transform2D = element2D as ITransform2D;
                currentPosition = element2D.Geometry as IPoint;

                transform2D.Move(newPosition.X - currentPosition.X, newPosition.Y - currentPosition.Y);
            }

            if (element3D != null)
            {
                element3D.Geometry = newPosition;

                //  transform2D = element3D as ITransform2D;
                //currentPosition = element3D.Geometry as IPoint;

                //transform2D.Move(newPosition.X - currentPosition.X, newPosition.Y - currentPosition.Y);
            }

        }



        private double Set_GaugeControl()
        {
            double velocity = 0.0;

            velocity = dblInputVelocity.Value;

            // gaugeControl1.SetParameters(0, 500, velocity, 100);
            GaugeCircularScale oScale = gaugeControl1.CircularScales[0];

            oScale.Labels.Interval = 50;
            oScale.MinValue = 0;
            oScale.MaxValue = 500;

            GaugePointer GPointer = oScale.Pointers[0];
            GPointer.Value = velocity;

            return velocity;
        }

        //public ILayer AddShapefileUsingOpenFileDialog(IActiveView activeView)
        //{
        //    //parameter check
        //    if (activeView == null)
        //    {
        //        return null;
        //    }

        //    // Use the OpenFileDialog Class to choose which shapefile to load.
        //    System.Windows.Forms.OpenFileDialog openFileDialog = new System.Windows.Forms.OpenFileDialog();
        //    openFileDialog.InitialDirectory = @"C:\Users\mgh\Desktop\AirPlane2";
        //    openFileDialog.Filter = "Shapefiles (*.shp)|*.shp";
        //    openFileDialog.FilterIndex = 2;
        //    openFileDialog.RestoreDirectory = true;
        //    openFileDialog.Multiselect = false;

        //    IFeatureLayer featureLayer = null;

        //    if (openFileDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
        //    {
        //        // The user chose a particular shapefile.

        //        // The returned string will be the full path, filename and file-extension for the chosen shapefile. Example: "C:\test\cities.shp"
        //        string shapefileLocation = openFileDialog.FileName;

        //        if (shapefileLocation != "")
        //        {
        //            // Ensure the user chooses a shapefile

        //            // Create a new ShapefileWorkspaceFactory CoClass to create a new workspace
        //            IWorkspaceFactory workspaceFactory = new ShapefileWorkspaceFactoryClass();

        //            // IO.Path.GetDirectoryName(shapefileLocation) returns the directory part of the string. Example: "C:\test\"
        //            IFeatureWorkspace featureWorkspace = (IFeatureWorkspace)workspaceFactory.OpenFromFile(System.IO.Path.GetDirectoryName(shapefileLocation), 0); // Explicit Cast

        //            // IO.Path.GetFileNameWithoutExtension(shapefileLocation) returns the base filename (without extension). Example: "cities"
        //            IFeatureClass featureClass = featureWorkspace.OpenFeatureClass(System.IO.Path.GetFileNameWithoutExtension(shapefileLocation));

        //            featureLayer = new FeatureLayer();
        //            featureLayer.FeatureClass = featureClass;
        //            featureLayer.Name = featureClass.AliasName;
        //            featureLayer.Visible = true;

        //            AddVectorDataToGlobe(GlobeControl.Globe, esriGlobeLayerType.esriGlobeLayerTypeDraped, (ILayer)featureLayer);

        //            // Zoom the display to the full extent of all layers in the map
        //            activeView.Extent = featureLayer.AreaOfInterest;
        //            activeView.PartialRefresh(esriViewDrawPhase.esriViewGeography, null, null);

        //            ChangeRasterize();
        //            ChangeHeightProperties(featureLayer);

        //            // ChangeProp();
        //            //AddDrapeLayerToGlobeElevationSurface(m_globe.GlobeDisplay, (ILayer)featureLayer, @"C:\Users\mgh\Desktop\AirPlane2\AirPlane\AirPlane\bin\Debug\GlobeData\wsiearth.tif");
        //        }
        //        else
        //        {

        //        }
        //    }
        //    else
        //    {

        //    }

        //    return (ILayer)featureLayer;
        //}

        public void AddVectorDataToGlobe(ESRI.ArcGIS.GlobeCore.IGlobe globe, ESRI.ArcGIS.GlobeCore.esriGlobeLayerType globeLayerType, ESRI.ArcGIS.Carto.ILayer layer)
        {
            if (globe == null || layer == null || globeLayerType == ESRI.ArcGIS.GlobeCore.esriGlobeLayerType.esriGlobeLayerTypeElevation || globeLayerType == ESRI.ArcGIS.GlobeCore.esriGlobeLayerType.esriGlobeLayerTypeUnknown)
            {
                return;
            }

            ESRI.ArcGIS.GlobeCore.IGlobeDisplay globeDisplay = globe.GlobeDisplay;
            ESRI.ArcGIS.GlobeCore.IGlobeDisplay2 globeDisplay2 = globeDisplay as ESRI.ArcGIS.GlobeCore.IGlobeDisplay2; // Reference or Boxing Conversion
            globeDisplay2.PauseCaching = true;
            globe.AddLayerType(layer, globeLayerType, true);
            ESRI.ArcGIS.GlobeCore.IGlobeDisplayLayers globeDisplayLayers = globeDisplay as ESRI.ArcGIS.GlobeCore.IGlobeDisplayLayers; // Reference or Boxing Conversion
            ESRI.ArcGIS.GlobeCore.IGlobeLayerProperties globeLayerProperties = globeDisplayLayers.FindGlobeProperties(layer);
            globeLayerProperties.Type = esriGlobeDataType.esriGlobeDataVector;
            globeLayerProperties.IsDynamicallyRasterized = false;
            globeDisplay2.PauseCaching = false;
        }

        private void ShowMessages(string message)
        {
            lblStatus.Text = "";
        }


        #region "GlobeControl Tools"


        private void btnZoomInOut_GlobeControl_Click(object sender, EventArgs e)
        {
            ControlsGlobeZoomInOutTool GlobeZoomInOutTool = new ESRI.ArcGIS.Controls.ControlsGlobeZoomInOutTool();

            GlobeZoomInOutTool.OnCreate(GlobeControl.Object);
            GlobeZoomInOutTool.OnClick();
            GlobeControl.CurrentTool = (ESRI.ArcGIS.SystemUI.ITool)GlobeZoomInOutTool;
        }


        private void btnFixZoomIn_GlobeControl_Click(object sender, EventArgs e)
        {
            ControlsGlobeFixedZoomInCommand pCommand = new ESRI.ArcGIS.Controls.ControlsGlobeFixedZoomInCommand();
            pCommand.OnCreate(GlobeControl.Object);
            pCommand.OnClick();
        }

        private void btnFixZoomOut_GlobeControl_Click(object sender, EventArgs e)
        {

            ControlsGlobeFixedZoomOutCommand pCommand = new ESRI.ArcGIS.Controls.ControlsGlobeFixedZoomOutCommand();
            pCommand.OnCreate(GlobeControl.Object);
            pCommand.OnClick();
        }

        private void btnFeatureSelection_GlobeControl_Click(object sender, EventArgs e)
        {
            ControlsGlobeSelectFeaturesTool GlobePanTool = new ESRI.ArcGIS.Controls.ControlsGlobeSelectFeaturesTool();

            GlobePanTool.OnCreate(GlobeControl.Object);
            GlobePanTool.OnClick();
            GlobeControl.CurrentTool = (ESRI.ArcGIS.SystemUI.ITool)GlobePanTool;
        }

        private void btnSelectAll_GlobeControl_Click(object sender, EventArgs e)
        {
            ControlsSelectFeaturesTool GlobePanTool = new ESRI.ArcGIS.Controls.ControlsSelectFeaturesTool();

            GlobePanTool.OnCreate(GlobeControl.Object);
            GlobePanTool.OnClick();
            GlobeControl.CurrentTool = (ESRI.ArcGIS.SystemUI.ITool)GlobePanTool;
        }



        private void btnZoomToSelectedFeatures_GlobeControl_Click(object sender, EventArgs e)
        {
            ControlsZoomToSelectedCommand pCommand = new ESRI.ArcGIS.Controls.ControlsZoomToSelectedCommand();
            pCommand.OnCreate(GlobeControl.Object);
            pCommand.OnClick();
        }

        private void btnNavigate_GlobeControl_Click(object sender, EventArgs e)
        {
            ControlsGlobeNavigateTool globeNavigateTool = new ESRI.ArcGIS.Controls.ControlsGlobeNavigateTool();

            globeNavigateTool.OnCreate(GlobeControl.Object);
            globeNavigateTool.OnClick();
            GlobeControl.CurrentTool = (ESRI.ArcGIS.SystemUI.ITool)globeNavigateTool;
        }

        private void btnNavigationMode_GlobeControl_Click(object sender, EventArgs e)
        {
            ControlsGlobeNavigationModeCommand globeNavigationModeCommand = new ESRI.ArcGIS.Controls.ControlsGlobeNavigationModeCommand();

            globeNavigationModeCommand.OnCreate(GlobeControl.Object);
            globeNavigationModeCommand.OnClick();
        }

        private void btnFly_GlobeControl_Click(object sender, EventArgs e)
        {
            ControlsGlobeFlyTool globeFlyTool = new ESRI.ArcGIS.Controls.ControlsGlobeFlyTool();

            globeFlyTool.OnCreate(GlobeControl.Object);
            globeFlyTool.OnClick();
            GlobeControl.CurrentTool = (ESRI.ArcGIS.SystemUI.ITool)globeFlyTool;
        }

        private void btnLookAround_GlobeControl_Click(object sender, EventArgs e)
        {
            ControlsGlobeLookAroundTool globeLookAroundTool = new ESRI.ArcGIS.Controls.ControlsGlobeLookAroundTool();

            globeLookAroundTool.OnCreate(GlobeControl.Object);
            globeLookAroundTool.OnClick();
            GlobeControl.CurrentTool = (ESRI.ArcGIS.SystemUI.ITool)globeLookAroundTool;
        }

        private void btnLookNorth_GlobeControl_Click(object sender, EventArgs e)
        {
            ControlsGlobeNorthCommand globeNorthCommand = new ESRI.ArcGIS.Controls.ControlsGlobeNorthCommand();

            globeNorthCommand.OnCreate(GlobeControl.Object);
            globeNorthCommand.OnClick();
        }

        private void btnNavigationToggleDraftMode_GlobeControl_Click(object sender, EventArgs e)
        {
            ControlsGlobeNavigationModeCommand globeNavigationModeCommand = new ESRI.ArcGIS.Controls.ControlsGlobeNavigationModeCommand();

            globeNavigationModeCommand.OnCreate(GlobeControl.Object);
            globeNavigationModeCommand.OnClick();
        }

        private void btnPanTarget_GlobeControl_Click(object sender, EventArgs e)
        {
            ControlsGlobeTargetPanTool globeTargetPanTool = new ESRI.ArcGIS.Controls.ControlsGlobeTargetPanTool();

            globeTargetPanTool.OnCreate(GlobeControl.Object);
            globeTargetPanTool.OnClick();
            GlobeControl.CurrentTool = (ESRI.ArcGIS.SystemUI.ITool)globeTargetPanTool;
        }

        private void btnPivotTool_GlobeControl_Click(object sender, EventArgs e)
        {
            // ControlsGlobePanDragTool globePanDragTool = new ESRI.ArcGIS.Controls.controlsglobe

            //globePanDragTool.OnCreate(GlobeControl.Object);
            //globePanDragTool.OnClick();
            //GlobeControl.CurrentTool = (ESRI.ArcGIS.SystemUI.ITool)globePanDragTool;
        }

        private void btnSetObserver_GlobeControl_Click(object sender, EventArgs e)
        {
            // ControlsGlobePanDragTool globePanDragTool = new ESRI.ArcGIS.Controls.controlsglobe

            //globePanDragTool.OnCreate(GlobeControl.Object);
            //globePanDragTool.OnClick();
            //GlobeControl.CurrentTool = (ESRI.ArcGIS.SystemUI.ITool)globePanDragTool;
        }

        private void btnSpinStop_GlobeControl_Click(object sender, EventArgs e)
        {
            ControlsGlobeSpinStopCommand globeSpinStopCommand = new ESRI.ArcGIS.Controls.ControlsGlobeSpinStopCommand();

            globeSpinStopCommand.OnCreate(GlobeControl.Object);
            globeSpinStopCommand.OnClick();
        }

        private void btnSpinToLeft_GlobeControl_Click(object sender, EventArgs e)
        {
            ControlsGlobeSpinClockwiseCommand globeSpinClockwiseCommand = new ESRI.ArcGIS.Controls.ControlsGlobeSpinClockwiseCommand();

            globeSpinClockwiseCommand.OnCreate(GlobeControl.Object);
            globeSpinClockwiseCommand.OnClick();
        }

        private void btnSpinToRight_GlobeControl_Click(object sender, EventArgs e)
        {
            ControlsGlobeSpinCounterClockwiseCommand globeSpinCounterClockwiseCommand = new ESRI.ArcGIS.Controls.ControlsGlobeSpinCounterClockwiseCommand();

            globeSpinCounterClockwiseCommand.OnCreate(GlobeControl.Object);
            globeSpinCounterClockwiseCommand.OnClick();
        }

        private void btnCenterOnTarget_GlobeControl_Click(object sender, EventArgs e)
        {
            ControlsGlobeTargetCenterTool globeTargetCenterTool = new ESRI.ArcGIS.Controls.ControlsGlobeTargetCenterTool();

            globeTargetCenterTool.OnCreate(GlobeControl.Object);
            globeTargetCenterTool.OnClick();
            GlobeControl.CurrentTool = (ESRI.ArcGIS.SystemUI.ITool)globeTargetCenterTool;
        }

        private void btnFlyOrbitalTool_GlobeControl_Click(object sender, EventArgs e)
        {
            ControlsGlobeOrbitalFlyTool globeOrbitalFlyTool = new ESRI.ArcGIS.Controls.ControlsGlobeOrbitalFlyTool();

            globeOrbitalFlyTool.OnCreate(GlobeControl.Object);
            globeOrbitalFlyTool.OnClick();
            GlobeControl.CurrentTool = (ESRI.ArcGIS.SystemUI.ITool)globeOrbitalFlyTool;
        }

        private void btnFlyTo_GlobeControl_Click(object sender, EventArgs e)
        {
            //ControlsGlobeOrbitalFlyTool globeOrbitalFlyTool = new ESRI.ArcGIS.Controls.con

            //globeOrbitalFlyTool.OnCreate(GlobeControl.Object);
            //globeOrbitalFlyTool.OnClick();
            //GlobeControl.CurrentTool = (ESRI.ArcGIS.SystemUI.ITool)globeOrbitalFlyTool;
        }

        private void btn2DDisplay_GlobeControl_Click(object sender, EventArgs e)
        {
            //ControlsGlobeOrbitalFlyTool globeOrbitalFlyTool = new ESRI.ArcGIS.Controls.controlsglobe

            //globeOrbitalFlyTool.OnCreate(GlobeControl.Object);
            //globeOrbitalFlyTool.OnClick();
            //GlobeControl.CurrentTool = (ESRI.ArcGIS.SystemUI.ITool)globeOrbitalFlyTool;
        }

        private void btnAnalystCreateLineOfSight_GlobeControl_Click(object sender, EventArgs e)
        {
            ControlsGlobeFixedLineOfSightTool globeFixedLineOfSightTool = new ESRI.ArcGIS.Controls.ControlsGlobeFixedLineOfSightTool();

            globeFixedLineOfSightTool.OnCreate(GlobeControl.Object);
            globeFixedLineOfSightTool.OnClick();
            GlobeControl.CurrentTool = (ESRI.ArcGIS.SystemUI.ITool)globeFixedLineOfSightTool;
        }

        private void btnWalk_GlobeControl_Click(object sender, EventArgs e)
        {
            ControlsGlobeWalkTool globeWalkTool = new ESRI.ArcGIS.Controls.ControlsGlobeWalkTool();

            globeWalkTool.OnCreate(GlobeControl.Object);
            globeWalkTool.OnClick();
            GlobeControl.CurrentTool = (ESRI.ArcGIS.SystemUI.ITool)globeWalkTool;
        }

        private void btnZoomToTarget_GlobeControl_Click(object sender, EventArgs e)
        {
            ControlsGlobeTargetZoomTool globeTargetZoomTool = new ESRI.ArcGIS.Controls.ControlsGlobeTargetZoomTool();

            globeTargetZoomTool.OnCreate(GlobeControl.Object);
            globeTargetZoomTool.OnClick();
            GlobeControl.CurrentTool = (ESRI.ArcGIS.SystemUI.ITool)globeTargetZoomTool;
        }

        private void btnZoomInOut_Globe_Click(object sender, EventArgs e)
        {
            ControlsGlobeZoomInOutTool globeZoomInOutTool = new ESRI.ArcGIS.Controls.ControlsGlobeZoomInOutTool();

            globeZoomInOutTool.OnCreate(GlobeControl.Object);
            globeZoomInOutTool.OnClick();
            GlobeControl.CurrentTool = (ESRI.ArcGIS.SystemUI.ITool)globeZoomInOutTool;
        }

        private void btnFullExtent_Globe_Click(object sender, EventArgs e)
        {
            ControlsGlobeFullExtentCommand globeFullExtentCommand = new ESRI.ArcGIS.Controls.ControlsGlobeFullExtentCommand();

            globeFullExtentCommand.OnCreate(GlobeControl.Object);
            globeFullExtentCommand.OnClick();
        }

        private void btnPan_Globe_Click(object sender, EventArgs e)
        {
            ControlsGlobePanDragTool globePanTool = new ESRI.ArcGIS.Controls.ControlsGlobePanDragTool();

            globePanTool.OnCreate(GlobeControl.Object);
            globePanTool.OnClick();
            GlobeControl.CurrentTool = (ESRI.ArcGIS.SystemUI.ITool)globePanTool;
        }

        private void btnFlyAlong_Globecontrol_Click(object sender, EventArgs e)
        {

        }

        #endregion

        #region "MainToolbar"

        private void btnOpenMXD_Click(object sender, EventArgs e)
        {
            ControlsOpenDocCommand pCommand = new ESRI.ArcGIS.Controls.ControlsOpenDocCommand();
            pCommand.OnCreate(MapControl.Object);
            pCommand.OnClick();
        }

        private void btnGraphics_Click(object sender, EventArgs e)
        {
            // Get Selected Path
            Path.Map = m_MapMapControl;
            Path.Get_Path();

            Tool_Elements createElements = new Tool_Elements();
            createElements.OnCreate(MapControl.Object);
            createElements.Globe = GlobeControl.Globe;
            createElements.OnClick();
            MapControl.CurrentTool = createElements;
        }

        private void btnAddRasterData_Click(object sender, EventArgs e)
        {
            Command_AddRasterData addRasterData = new Command_AddRasterData();
            addRasterData.OnCreate(MapControl.Object);
            addRasterData.Globe = GlobeControl.Globe;
            addRasterData.OnClick();
        }

        private void btn3DLayers_Click(object sender, EventArgs e)
        {
            Tools._3DLayers.Command_3DLayers Command_3DLayers = new Tools._3DLayers.Command_3DLayers();
            Command_3DLayers.OnCreate(MapControl.Object);
            Command_3DLayers.Globe = GlobeControl.Globe;
            Command_3DLayers.OnClick();
        }

        private void btnAbout_Click(object sender, EventArgs e)
        {
            Tools.About.Command_About Command_About = new Tools.About.Command_About();
            Command_About.OnCreate(MapControl.Object);
            Command_About.OnClick();
        }

        #endregion

        #region "MapControl Tools"


        private void btnZoomIn_MapControl_Click(object sender, EventArgs e)
        {
            ControlsMapZoomInTool MapZoomInTool = new ESRI.ArcGIS.Controls.ControlsMapZoomInTool();

            MapZoomInTool.OnCreate(MapControl.Object);
            MapZoomInTool.OnClick();
            MapControl.CurrentTool = (ESRI.ArcGIS.SystemUI.ITool)MapZoomInTool;
        }


        private void btnZoomOut_MapControl_Click(object sender, EventArgs e)
        {
            ControlsMapZoomOutTool MapZoomOutTool = new ESRI.ArcGIS.Controls.ControlsMapZoomOutTool();

            MapZoomOutTool.OnCreate(MapControl.Object);
            MapZoomOutTool.OnClick();
            MapControl.CurrentTool = (ESRI.ArcGIS.SystemUI.ITool)MapZoomOutTool;
        }

        private void btnFullExtent_MapControl_Click(object sender, EventArgs e)
        {
            ControlsMapFullExtentCommand pCommand = new ESRI.ArcGIS.Controls.ControlsMapFullExtentCommand();
            pCommand.OnCreate(MapControl.Object);
            pCommand.OnClick();
        }

        private void btnPan_MapControl_Click(object sender, EventArgs e)
        {
            ControlsMapPanTool MapPanTool = new ESRI.ArcGIS.Controls.ControlsMapPanTool();

            MapPanTool.OnCreate(MapControl.Object);
            MapPanTool.OnClick();
            MapControl.CurrentTool = (ESRI.ArcGIS.SystemUI.ITool)MapPanTool;
        }

        private void btnFixZoomIn_MapControl_Click(object sender, EventArgs e)
        {
            ControlsMapZoomInFixedCommand pCommand = new ESRI.ArcGIS.Controls.ControlsMapZoomInFixedCommand();
            pCommand.OnCreate(MapControl.Object);
            pCommand.OnClick();
        }

        private void btnFixZoomOut_MapControl_Click(object sender, EventArgs e)
        {
            ControlsMapZoomOutFixedCommand pCommand = new ESRI.ArcGIS.Controls.ControlsMapZoomOutFixedCommand();
            pCommand.OnCreate(MapControl.Object);
            pCommand.OnClick();
        }

        private void btnSelectFeatures_MapControl_Click(object sender, EventArgs e)
        {
            ControlsSelectFeaturesTool MapPanTool = new ESRI.ArcGIS.Controls.ControlsSelectFeaturesTool();

            MapPanTool.OnCreate(MapControl.Object);
            MapPanTool.OnClick();
            MapControl.CurrentTool = (ESRI.ArcGIS.SystemUI.ITool)MapPanTool;
        }

        private void btnSelectAll_MapControl_Click(object sender, EventArgs e)
        {
            ControlsSelectAllCommand pCommand = new ESRI.ArcGIS.Controls.ControlsSelectAllCommand();
            pCommand.OnCreate(MapControl.Object);
            pCommand.OnClick();
        }

        private void BtnUnSelect_MapControl_Click(object sender, EventArgs e)
        {
            ControlsClearSelectionCommand pCommand = new ESRI.ArcGIS.Controls.ControlsClearSelectionCommand();
            pCommand.OnCreate(MapControl.Object);
            pCommand.OnClick();
        }

        private void btnSwitchSelection_MapControl_Click(object sender, EventArgs e)
        {
            ControlsSwitchSelectionCommand pCommand = new ESRI.ArcGIS.Controls.ControlsSwitchSelectionCommand();
            pCommand.OnCreate(MapControl.Object);
            pCommand.OnClick();
        }

        private void btnZoomToSelectedFeatures_MapControl_Click(object sender, EventArgs e)
        {
            ControlsZoomToSelectedCommand pCommand = new ESRI.ArcGIS.Controls.ControlsZoomToSelectedCommand();
            pCommand.OnCreate(MapControl.Object);
            pCommand.OnClick();
        }

        private void btnIdentify_Click(object sender, EventArgs e)
        {
            //ControlsSwitchSelectionCommand pCommand = new ESRI.ArcGIS.Controls.con();
            //pCommand.OnCreate(MapControl.Object);
            //pCommand.OnClick();
        }

        private void btnSearch_Click(object sender, EventArgs e)
        {

        }

        #endregion

        private void buttonX1_Click(object sender, EventArgs e)
        {
            GlobeDynamicObjectTracking.TrackDynamicObject t = new TrackDynamicObject();
            t.OnCreate(GlobeControl.Object);
            t.OnClick();
        }

        private void btnStopAirPlane_Click(object sender, EventArgs e)
        {
            timer_Airlpanes.Enabled = false;
        }

        private void btnDeleteGraphics_MapControl_Click(object sender, EventArgs e)
        {
            IGraphicsContainer graphicsContainer = m_MapMapControl as IGraphicsContainer;
            graphicsContainer.DeleteAllElements();
            timer_Airlpanes.Enabled = false;
            Elements2D.Elements2Ds.Clear();
            ClearVelocityGauge();
            ClearViewrs();
            IActiveView activeView = m_MapMapControl as IActiveView;
            activeView.PartialRefresh(esriViewDrawPhase.esriViewAll, null, null);
        }

        private void ClearVelocityGauge()
        {
            GaugeCircularScale oScale = gaugeControl1.CircularScales[0];
            GaugePointer GPointer = oScale.Pointers[0];
            GPointer.Value = 0;
        }

        private void ClearViewrs()
        {
            lblAirplaneName.Text = string.Empty;
            lblAirplaneVelocity.Text = string.Empty;
            lblAirplaneElevation.Text = string.Empty;
            lblAirplaneOrigin.Text = string.Empty;
            lblAirplaneDestination.Text = string.Empty;

            lblAirplaneXCurrent.Text = string.Empty;
            lblAirplaneYCurrent.Text = string.Empty;

            lblLengthPath.Text = string.Empty;
            lblLengthFromPath.Text = string.Empty;
            lblLengthToPath.Text = string.Empty;
        }

        private void buttonX2_Click(object sender, EventArgs e)
        {
            ESRI.ArcGIS.Analyst3D.IScene scene = (ESRI.ArcGIS.Analyst3D.IScene)GlobeControl.Globe;
            // Explicit cast.

            ESRI.ArcGIS.Geometry.IEnvelope envelope = GlobalValues.element3D.Geometry.Envelope;

            ESRI.ArcGIS.Analyst3D.ICamera camera = GlobeControl.Globe.GlobeDisplay.ActiveViewer.Camera;
            ESRI.ArcGIS.GlobeCore.IGlobeCamera globeCamera = (ESRI.ArcGIS.GlobeCore.IGlobeCamera)
                camera; // Explicit cast.


            ESRI.ArcGIS.Analyst3D.ISceneViewer sceneViewer = GlobeControl.Globe.GlobeDisplay.ActiveViewer;
            globeCamera.SetToZoomToExtents(envelope, GlobeControl.Globe, sceneViewer);

        }

        // Way 1
        //public IPoint ConstructPointParallelToLine(ICurve pCurve, double distanceAlongPath)
        //{
        //    IPoint outPutPoint = null;
        //    try
        //    {
        //        IPoint pPoint = new ESRI.ArcGIS.Geometry.Point();
        //        IConstructPoint constructionPoint = (IConstructPoint)pPoint;

        //        IGeometryCollection geometryCollection = pCurve as IGeometryCollection;
        //        ISegmentCollection segmentCollection = geometryCollection.get_Geometry(0) as ISegmentCollection;

        //        ISegment segment = segmentCollection.get_Segment(0);

        //        IPoint fromPoint = ElementsPropertise.StartPosition;
        //        IProximityOperator ro = (IProximityOperator)fromPoint;

        //        constructionPoint.ConstructOffset(pCurve, esriSegmentExtension.esriNoExtension, distanceAlongPath, false, -ro.ReturnDistance((IGeometry)pCurve));
        //        // constructionPoint.ConstructParallel(segment, esriSegmentExtension.esriNoExtension, fromPoint, distanceAlongPath);
        //        outPutPoint = constructionPoint as IPoint;
        //    }
        //    catch
        //    {
        //        outPutPoint = null;
        //    }

        //    return outPutPoint;
        //}

        // Way 2
    }
}