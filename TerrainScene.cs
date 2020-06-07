//////////////////////////////////////////////////
// Externals.

using MartinVasina;     // Terrain

//////////////////////////////////////////////////
// Rendering params.

Debug.Assert(scene != null);
Debug.Assert(context != null);

// Override image resolution and supersampling.
context[PropertyName.CTX_WIDTH]         = 640;
context[PropertyName.CTX_HEIGHT]        = 480;
context[PropertyName.CTX_SUPERSAMPLING] =  4;

//////////////////////////////////////////////////
// Preprocessing stage support.

// Uncomment the block if you need preprocessing.
/*
if (Util.TryParseBool(context, PropertyName.CTX_PREPROCESSING))
{
  double time = 0.0;
  bool single = Util.TryParse(context, PropertyName.CTX_TIME, ref time);
  // if (single) simulate only for a single frame with the given 'time'

  // TODO: put your preprocessing code here!
  //
  // It will be run only this time.
  // Store preprocessing results to arbitrary (non-reserved) context item,
  //  subsequent script calls will find it there...
}
*/

//////////////////////////////////////////////////
// Param string from UI.

// Params dictionary.
Dictionary<string, string> p = Util.ParseKeyValueList(param);

//////////////////////////////////////////////////
// CSG scene.

CSGInnerNode root = new CSGInnerNode(SetOperation.Union);
root.SetAttribute(PropertyName.REFLECTANCE_MODEL, new PhongModel());
root.SetAttribute(PropertyName.MATERIAL, new PhongMaterial(new double[] {1.0, 0.7, 0.1}, 0.1, 0.85, 0.05, 64));
scene.Intersectable = root;

// Background color.
scene.BackgroundColor = new double[] {0.0, 0.01, 0.03};

// Camera.
scene.Camera = new StaticCamera(new Vector3d(0, 25, -20), new Vector3d(0.0, -1, 1), 50.0);

// Light sources.
scene.Sources = new System.Collections.Generic.LinkedList<ILightSource>();
scene.Sources.Add(new AmbientLightSource(0.8));
scene.Sources.Add(new PointLightSource(new Vector3d(-10, 15, 0), 1.2));


// --- NODE DEFINITIONS ----------------------------------------------------

Terrain terrain = new Terrain(-10,10,-10,10,0,10,1,true,true,5);
double height = terrain.GetTerrainHeight(-3, -2);
terrain.SetAttribute(PropertyName.COLOR, new double[] {0.37, 0.39, 0.145});
root.InsertChild(terrain, Matrix4d.Identity);
