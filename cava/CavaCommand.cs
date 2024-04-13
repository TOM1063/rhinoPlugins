using Rhino;
using Rhino.Commands;
using Rhino.DocObjects;
using Rhino.Geometry;
using Rhino.Input;
using Rhino.Input.Custom;
using System;
using System.Collections.Generic;

namespace cava
{
    public class CavaCommand : Command
    {
        public CavaCommand()
        {
            // Rhino only creates one instance of each command class defined in a
            // plug-in, so it is safe to store a refence in a static property.
            Instance = this;
        }

        ///<summary>The only instance of this command.</summary>
        public static CavaCommand Instance { get; private set; }

        ///<returns>The command name as it appears on the Rhino command line.</returns>
        public override string EnglishName => "CavaCommand";

        protected override Result RunCommand(RhinoDoc doc, RunMode mode)
        {
            RhinoApp.WriteLine("The {0} command will transform your geometries into cavaliere projection", EnglishName);

            var go = new GetObject();
            go.SetCommandPrompt("Select geometry to apply cava transformation");
            go.GeometryFilter = Rhino.DocObjects.ObjectType.AnyObject;
            go.GetMultiple(1, 0);

            if (go.CommandResult() != Result.Success)
                return go.CommandResult();

            var layerName = "cava";
            var layer = doc.Layers.FindName(layerName);

            if (layer == null)
            {
                layer = new Layer();
                layer.Name = layerName;
                layer.Color = System.Drawing.Color.Red;
                doc.Layers.Add(layer);

                layer = doc.Layers.FindName(layerName);
            }

            foreach (var objRef in go.Objects())
            {
                var geometry = objRef.Geometry();

                if (geometry != null)
                {
                    //var transformedGeometry = geometry.Duplicate();

                    var inv_sqr_tw = 1 / Math.Sqrt(2);

                    Vector3d vec_x = new Vector3d(1.00, 0.00, 0.00);
                    Vector3d vec_y = new Vector3d(inv_sqr_tw, inv_sqr_tw, inv_sqr_tw);
                    Vector3d vec_z = new Vector3d(0.00, 0.00, 1.00);

                    geometry.Transform(Transform.Shear(Plane.WorldXY, vec_x, vec_y, vec_z));

                    var guid = doc.Objects.Add(geometry);

                    doc.Objects.Select(objRef,false);
                    doc.Objects.Select(guid);

                    var transformed = doc.Objects.FindId(guid);

                    if (transformed != null)
                    {
                        transformed.Attributes.LayerIndex = layer.Index;
                        transformed.CommitChanges();
                    }
                }
            }

            doc.Views.Redraw();

            return Result.Success;

           
        }
    }
}
