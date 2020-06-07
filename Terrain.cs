using MathSupport;
using OpenTK;
using Rendering;
using System;
using System.Collections.Generic;

namespace MartinVasina
{
  /// <summary>
  /// Terrain object.
  /// </summary>
  [Serializable]
  public class Terrain : DefaultSceneNode, ISolid
  {

    public List<Triangle> Triangles { get; set; }

    public double UnitSize { get; set; }

    public bool Border { get; set; }

    /// <summary>
    /// Lower bound for x coordinate.
    /// </summary>
    public double XMin { get; set; }

    /// <summary>
    /// Upper bound for x coordinate (or triangle).
    /// </summary>
    public double XMax { get; set; }

    /// <summary>
    /// Lower bound for y coordinate.
    /// </summary>
    public double YMin { get; set; }

    /// <summary>
    /// Upper bound for y coordinate (or triangle).
    /// </summary>
    public double YMax { get; set; }



    /// <summary>
    /// Rectangle (four restrictions).
    /// </summary>
    public Terrain (double xMi, double xMa, double yMi, double yMa, double zMi, double zMa, double unitSize, bool random, bool border, float k)
    {

      XMin = xMi;
      XMax = xMa;
      YMin = yMi;
      YMax = yMa;
      UnitSize = unitSize;
      Border = border;

      if (random)
        NoiseMaker.Reseed();

      // Generate mesh
      int sizeX = (int)Math.Ceiling((xMa - xMi) / unitSize) + 1;
      int sizeY = (int)Math.Ceiling((yMa - yMi) / unitSize) + 1;
      Vector3d[,] mesh = new Vector3d[sizeX, sizeY];
      double x = xMi;
      for (int m = 0; m < sizeX; m++)
      {
        double y = yMi;
        for (int n = 0; n < sizeY; n++)
        {
          double z = ((NoiseMaker.Noise((float)m / k, (float)n / k, 0) + 1) / 2) * (zMa - zMi) + zMi;
          mesh[m, n] = new Vector3d(x, z, y);
          y += unitSize;
        }
        x += unitSize;
      }

      // Convert mesh to list of triangles
      Triangles = new List<Triangle>();
      for (int m = 0; m < sizeX - 1; ++m)
        for (int n = 0; n < sizeY - 1; ++n)
        {
          Vector3d a = mesh[m, n];
          Vector3d b = mesh[m + 1, n];
          Vector3d c = mesh[m, n + 1];
          Vector3d d = mesh[m + 1, n + 1];

          if (m % 2 == 0 && n % 2 == 0 || m % 2 == 1 && n % 2 == 1)
          {
            Triangles.Add(new Triangle(b, a, c));
            Triangles.Add(new Triangle(b, c, d));
          }
          else
          {
            Triangles.Add(new Triangle(b, a, d));
            Triangles.Add(new Triangle(a, c, d));
          }
        }
    }

    public double GetTerrainHeight(double x, double y)
    {
      double height = Double.NegativeInfinity;
      for (int idx = 0; idx < Triangles.Count; ++idx)
      {
        Triangle tri = Triangles[idx];
        Vector3d a = tri.a;
        Vector3d b = tri.b;
        Vector3d c = tri.c;
        Vector2d uv;
        Vector3d p0 = new Vector3d(x, 0, y);
        Vector3d p1 = new Vector3d(0, 1, 0);

        height = Geometry.RayTriangleIntersection(ref p0, ref p1, ref a, ref b, ref c, out uv);
        if (!Double.IsInfinity(height))
          return height;
      }
      return height;
    }


    /// <summary>
    /// Computes the complete intersection of the given ray with the object.
    /// </summary>
    /// <param name="p0">Ray origin.</param>
    /// <param name="p1">Ray direction vector.</param>
    /// <returns>Sorted list of intersection records.</returns>
    public override LinkedList<Intersection> Intersect (Vector3d p0, Vector3d p1)
    {
      List<Intersection> result = null;
      Intersection i;

      for (int idx = 0; idx < Triangles.Count; ++idx) 
      {
        Triangle tri = Triangles[idx];
        Vector3d a = tri.a;
        Vector3d b = tri.b;
        Vector3d c = tri.c;

        Vector2d uv;
        CSGInnerNode.countTriangles++;
        double t = Geometry.RayTriangleIntersection(ref p0, ref p1, ref a, ref b, ref c, out uv);

        if (double.IsInfinity(t))
          continue;

        if (result == null)
          result = new List<Intersection>();

        // Compile the 1st Intersection instance:
        i = new Intersection(this)
        {
          T = t,
          Enter = true,
          Front = true,
          CoordLocal = p0 + t * p1,
          Normal = new Vector3d(tri.normal) // set normal vector
        };
        //border of triangle, using zero vector
        if (Border && MathUtils.IsPointNearTriangleEdge(i.CoordLocal, tri, 0.02 * UnitSize))
          i.Normal = new Vector3d();


        result.Add(i);
      }

      if (result == null)
        return null;

      // Finalizing the result: sort the result list
      result.Sort();
      return new LinkedList<Intersection>(result);
    }

    /// <summary>
    /// Complete all relevant items in the given Intersection object.
    /// </summary>
    /// <param name="inter">Intersection instance to complete.</param>
    public override void CompleteIntersection (Intersection inter)
    {
      // normal vector:
      //Vector3d tu = Vector3d.TransformVector(Vector3d.UnitX, inter.LocalToWorld);
      //Vector3d tv = Vector3d.TransformVector(Vector3d.UnitY, inter.LocalToWorld);
      //Vector3d.Cross(ref tu, ref tv, out inter.Normal);
    }

    public void GetBoundingBox (out Vector3d corner1, out Vector3d corner2)
    {
      throw new NotImplementedException();
    }
  }
}

