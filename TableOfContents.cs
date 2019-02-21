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

using ESRI.ArcGIS.GlobeCore;
using ESRI.ArcGIS.Analyst3D;
using ESRI.ArcGIS.Carto;

namespace AirPlane
{
    public class TableOfContents
    {
        IScene _scene;

        public TableOfContents(IGlobe globe)
        {
            _scene = GetScene(globe);
        }

        private IScene GetScene(IGlobe globe)
        {
            IScene scene;

            scene = globe as IScene;

            return scene;
        }

        public bool LayerExists(string name)
        {
            bool exists = false;

            for (int i = 0; i < _scene.LayerCount; i++)
            {
                ILayer layer = _scene.get_Layer(i);

                if (layer.Name == name)
                {
                    exists = true;
                    break;
                }
            }

            return exists;
        }

        public void ConstructLayer(string name)
        {
            ILayer globeGraphicsLayer = new GlobeGraphicsLayer();

            ILayer layer = globeGraphicsLayer as ILayer;

            layer.Name = name;

            _scene.AddLayer(layer, true);
        }

        public ILayer this[string name]
        {
            get
            {
                return GetLayer(name);
            }
        }

        private ILayer GetLayer(string name)
        {
            ILayer layer = null;

            for (int i = 0; i < _scene.LayerCount; i++)
            {
                ILayer currentLayer = _scene.get_Layer(i);

                if (currentLayer.Name == name)
                {
                    layer = currentLayer;
                    break;
                }
            }

            return layer;
        }
     }
}