using UnityEngine;
using System.Collections;

public static partial class CameraExt
{
    public static Rect GetExtentsOnXYPlaneOrthoAligned (this Camera self)
    {
        var bl = self.ViewportToWorldPoint (Vector3.zero);
        var tr = self.ViewportToWorldPoint (Vector3.one);
        return Rect.MinMaxRect (bl.x, bl.y, tr.x, tr.y);
    }

    public static Rect GetExtentsOnXYPlane (this Camera self)
    {
        throw new System.Exception ("This function hasn't been tested. I wrote it but only need the ortho version");
        var xyPlane = new Plane (Vector3.forward, Vector3.zero);
        var ray00 = self.ViewportPointToRay (new Vector3 (0f, 0f));
        var ray01 = self.ViewportPointToRay (new Vector3 (0f, 1f));
        var ray10 = self.ViewportPointToRay (new Vector3 (1f, 0f));
        var ray11 = self.ViewportPointToRay (new Vector3 (1f, 1f));
        float d00, d01, d10, d11;
        if (!xyPlane.Raycast (ray00, out d00) ||
            !xyPlane.Raycast (ray01, out d01) ||
            !xyPlane.Raycast (ray10, out d10) ||
            !xyPlane.Raycast (ray11, out d11))
        {
            throw new System.InvalidOperationException ("Camera is not looking at XY plane");
        }
        var v00 = ray00.GetPoint (d00);
        var v01 = ray01.GetPoint (d01);
        var v10 = ray10.GetPoint (d10);
        var v11 = ray11.GetPoint (d11);
        return Rect.MinMaxRect (
            Mathf.Min (v00.x, v01.x, v10.x, v11.x),
            Mathf.Min (v00.y, v01.y, v10.y, v11.y),
            Mathf.Max (v00.x, v01.x, v10.x, v11.x),
            Mathf.Max (v00.y, v01.y, v10.y, v11.y)
        );
    }
}