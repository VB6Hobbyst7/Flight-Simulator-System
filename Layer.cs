// Copyright 2012 ESRI
// 
// All rights reserved under the copyright laws of the United States
// and applicable international laws, treaties, and conventions.
// 
// You may freely redistribute and use this sample code, with or
// without modification, provided you include the original copyright
// notice and use restrictions.
// 
// See the use restrictions at <your ArcGIS install location>/DeveloperKit10.1/userestrictions.txt.
// 

using ESRI.ArcGIS.Analyst3D;
using ESRI.ArcGIS.Carto;
using ESRI.ArcGIS.GlobeCore;

namespace AirPlane
{
    public class Layer
    {
        private ILayer _layer;

        public Layer(ILayer layer)
        {
            _layer = layer;
        }

        public void AddElement(IElement element, IGlobeGraphicsElementProperties elementProperties)
        {
            int elementIndex;

            IGlobeGraphicsLayer globeGraphicsLayer = _layer as IGlobeGraphicsLayer;
            globeGraphicsLayer.AddElement(element, elementProperties, out elementIndex);
        }

        public void RemoveElement(int index)
        {
            IGraphicsContainer3D graphicsContainer3D = _layer as IGraphicsContainer3D;
            graphicsContainer3D.DeleteElement(this[index]);
        }


        public void UpdateAllElement()
        {
            IGlobeGraphicsLayer graphicsContainer3D = _layer as IGlobeGraphicsLayer;
            graphicsContainer3D.UpdateAllElements ();
        }

      
        public IElement this[int i]
        {
            get
            {
                IGraphicsContainer3D graphicsContainer3D = _layer as IGraphicsContainer3D;
                return graphicsContainer3D.get_Element(i);
            }
        }

        public int ElementCount
        {
            get
            {
                IGraphicsContainer3D graphicsContainer3D = _layer as IGraphicsContainer3D;
                return graphicsContainer3D.ElementCount;
            }
        }
    }
}