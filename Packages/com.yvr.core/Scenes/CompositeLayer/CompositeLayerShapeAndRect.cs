using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using YVR.Core;

public class CompositeLayerShapeAndRect : MonoBehaviour
{
    public YVRCompositeLayer compositeLayer;

    public void QuadShape()
    {
        compositeLayer.shape = YVRRenderLayerType.Quad;
        compositeLayer.transform.position = Vector3.forward * 2f;
        compositeLayer.transform.localScale = Vector3.one * 3f;
    }

    public void CylinderShape()
    {
        compositeLayer.shape = YVRRenderLayerType.Cylinder;
        compositeLayer.cylinderAngle = 30f;
        compositeLayer.transform.position = Vector3.forward * 2f;
        compositeLayer.transform.localScale = Vector3.one * 3f;
    }

    public void EquirectShape()
    {
        compositeLayer.shape = YVRRenderLayerType.Equirect;
        compositeLayer.radius = 3f;
        compositeLayer.transform.position = Vector3.zero;
        compositeLayer.transform.localScale = Vector3.one;
    }

    public void Monoscopic()
    {
        compositeLayer.SourceRectLeft = new Rect(0, 0, 1, 1);
        compositeLayer.SourceRectRight = new Rect(0, 0, 1, 1);
    }

    public void LeftRight()
    {
        compositeLayer.SourceRectLeft = new Rect(0, 0, 0.5f, 1);
        compositeLayer.SourceRectRight = new Rect(0.5f, 0, 0.5f, 1);
    }

    public void TopDown()
    {
        compositeLayer.SourceRectLeft = new Rect(0, 0.5f, 1, 0.5f);
        compositeLayer.SourceRectRight = new Rect(0, 0, 1, 0.5f);
    }

    public void StandardDestRect()
    {
        compositeLayer.DestRectLeft = new Rect(0, 0, 1, 1);
        compositeLayer.DestRectRight = new Rect(0, 0, 1, 1);
    }

    public void Video180DestRect()
    {
        compositeLayer.DestRectLeft = new Rect(0.25f, 0, 0.5f, 1);
        compositeLayer.DestRectRight = new Rect(0.25f, 0, 0.5f, 1);
    }
}
