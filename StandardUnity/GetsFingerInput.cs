using UnityEngine;
using System.Collections;
using System.Collections.Generic;


class GetsFingerInputSharedContext
{
    public class PerLayer
    {
        public GameObject pressedObject = null;
        public bool fingerLeftPressedObject = false;
        public bool pressedObjectNeedsLongPress = true;
    }

    public int fingerProcessedByLayerMask = 0;
    public bool hasUpdatedColliderSizes = false;
    public float FingerSizeSphereCastRadius = 0.5f;

    public Vector2 fingerDownScreenPoint = Vector2.zero;
    public float tapDownTime = -999f;

    public PerLayer[] Layer = new PerLayer[32];

    public GetsFingerInputSharedContext ()
    {
        for (int i = 0; i < this.Layer.Length; ++i)
        {
            this.Layer[i] = new PerLayer ();
        }
    }
}

// GameObjects with this class will have the following methods called when relevant:
//    void OnFingerDown() 
//    void OnFingerUp() 
//    void OnFingerTap() 
//    void OnFingerLongPress()
//      Called on a delay after OnFingerDown when the user has continuously pressed an object
//    void OnFingerDrag(Vector2 delta)
//      Called each frame when an object was tapped, finger has not been released but is moving
public class GetsFingerInput : MonoBehaviour
{
    private const float FingerSizeInches = 0.3f;
    private const float AmbiguityRadiusAsRatioOfFingerSize = 0.25f;
    private const float LongPressDelaySeconds = 1.25f;
    private const int MaxSimultaneousRaycastHits = 16;

    private static RaycastHit[] hitResultsBuffer = new RaycastHit[MaxSimultaneousRaycastHits];
    private static Vector2[] objectScreenCoordinatesBuffer = new Vector2[MaxSimultaneousRaycastHits];
    private static GetsFingerInputSharedContext context = new GetsFingerInputSharedContext ();

    private SphereCollider tapReceiverSphereCollider;

    private static float computeFingerSizePixels ()
    {
        float pixelsPerInch = (Screen.dpi < 25f || Screen.dpi > 1000f) ? 150f : Screen.dpi;
        float fingerSizePixels = FingerSizeInches * pixelsPerInch;
        if (null == Camera.main)
        {
            fingerSizePixels = Mathf.Min (Screen.width, Screen.height) / 6f;
        }
        return fingerSizePixels;
    }

    private static float computeFingerSizeWorldUnits ()
    {
        var fingerSizePixels = computeFingerSizePixels ();
        var measurePoint = new Vector3 (Screen.width/2 + fingerSizePixels, Screen.height/2, Camera.main.nearClipPlane);
        var zeroPoint = new Vector3 (Screen.width/2, Screen.height/2, Camera.main.nearClipPlane);
        var delta = Camera.main.ScreenToWorldPoint(measurePoint) - Camera.main.ScreenToWorldPoint(zeroPoint);
        float fingerSizeWorldUnits = delta.magnitude;
        return fingerSizeWorldUnits;
    }

	void Start ()
    {
        var collider = (SphereCollider)this.gameObject.AddComponent (typeof(SphereCollider));
        collider.radius = 1f;
        collider.isTrigger = true;
        this.tapReceiverSphereCollider = collider;
	}

    void OnDrawGizmos ()
    {
        Gizmos.color = new Color (1f, 1f, 0f, 0.45f);
        Gizmos.DrawSphere (this.transform.position, this.calculateColliderRadius ());
    }

    float calculateColliderRadius ()
    {
        var go = this.gameObject;
        var center = go.transform.position;
        var centerInScreenCoordinates = Camera.main.WorldToScreenPoint (center);
        var edge = centerInScreenCoordinates + new Vector3 (computeFingerSizePixels (), 0f, 0f);
        var ray = Camera.main.ScreenPointToRay (edge);
        float distanceToRay = Vector3.Cross(ray.direction, center - ray.origin).magnitude;
        return distanceToRay * 0.5f;
    }

    void LateUpdate ()
    {
        bool isHandlingOnDown = Input.GetMouseButtonDown (0);
        bool isHandlingOnUp   = Input.GetMouseButtonUp (0);
        if (isHandlingOnDown || isHandlingOnUp)
        {
            if (isHandlingOnUp)
            {
                context.Layer[this.gameObject.layer].pressedObject = null;
                context.fingerDownScreenPoint = Vector2.zero;
                context.tapDownTime = -999f;
            }
            context.hasUpdatedColliderSizes = false;
        }
        context.fingerProcessedByLayerMask = 0;
    }

    void Update ()
    {
//        var touches = Input.touches;
//        touches[0].
        this.updateLayer (this.gameObject.layer);
    }

    void updateLayer (int layer)
    {
        bool isHandlingOnDown = Input.GetMouseButtonDown (0);
        bool isHandlingOnUp   = Input.GetMouseButtonUp (0);
        bool isHandlingStay   = Input.GetMouseButton (0);
        if (!isHandlingOnUp && !isHandlingStay)
        {
            return;
        }

        var layerMask = 1 << this.gameObject.layer;
        if (0 != (layerMask & context.fingerProcessedByLayerMask))
        {
            return;
        }
        context.fingerProcessedByLayerMask |= layerMask;
        var contextLayer = context.Layer[layer];

        if (isHandlingOnDown)
        {
            var graphicRaycasters = GameObject.FindObjectsOfType (typeof(UnityEngine.UI.GraphicRaycaster));
            var results = new List<UnityEngine.EventSystems.RaycastResult>();
            for (int i = 0; i < graphicRaycasters.Length; ++i)
            {
                var raycaster = (UnityEngine.UI.GraphicRaycaster)graphicRaycasters[i];
                var pointerEventData = new UnityEngine.EventSystems.PointerEventData (null);
                pointerEventData.position = Input.mousePosition;
                raycaster.Raycast (pointerEventData, results);
                if (results.Count > 0)
                {
                    return;
                }
            }
            for (int i = 0; i < graphicRaycasters.Length; ++i)
            {
                var raycaster = (UnityEngine.UI.GraphicRaycaster)graphicRaycasters[i];
                raycaster.enabled = false;
            }
            context.tapDownTime = Time.time;
        }
        else if (!isHandlingOnUp && null == contextLayer.pressedObject)
        {
            return;
        }

        if (isHandlingOnDown || isHandlingOnUp)
        {
            if (false == context.hasUpdatedColliderSizes)
            {
                var objects = GameObject.FindObjectsOfType (typeof (GetsFingerInput));
                for (int i = 0; i < objects.Length; ++i)
                {
                    var mb = (GetsFingerInput)objects[i];
                    float radius = mb.transform.InverseTransformVector (new Vector3 (mb.calculateColliderRadius (), 0f, 0f)).magnitude;
                    mb.tapReceiverSphereCollider.radius = radius;
                }
                context.hasUpdatedColliderSizes = true;
            }
    
            GameObject interactedObject = null;
            context.FingerSizeSphereCastRadius = computeFingerSizeWorldUnits () * 0.5f;

            var ray = Camera.main.ScreenPointToRay (Input.mousePosition);
            int numberOfHits = Physics.SphereCastNonAlloc (ray, context.FingerSizeSphereCastRadius, hitResultsBuffer, Camera.main.farClipPlane, layerMask);
            if (numberOfHits == 0)
            {
                goto HandleInteractedObject;
            }
    
            Vector2 hitPointScreenCoordinate = Vector2.zero;
            for (int i = 0; i < numberOfHits; ++i)
            {
                var hit = hitResultsBuffer[i];
                if (0 == i)
                {
                    hitPointScreenCoordinate = removeZ (Camera.main.WorldToScreenPoint (hit.point));
                }
                var go = hit.collider.gameObject;
                if (null == go.GetComponent (typeof(GetsFingerInput)))
                {
                    objectScreenCoordinatesBuffer[i] = new Vector2 (-float.MaxValue*0.25f, -float.MaxValue*0.25f);
                    continue;
                }
                objectScreenCoordinatesBuffer[i] = removeZ (Camera.main.WorldToScreenPoint (go.transform.position));
            }
    
            int closestHitIndex = this.findClosestHitIndex (hitPointScreenCoordinate, objectScreenCoordinatesBuffer);
            if (0 > closestHitIndex)
            {
                goto HandleInteractedObject;
            }
            interactedObject = hitResultsBuffer[closestHitIndex].collider.gameObject;
            numberOfHits = Physics.SphereCastNonAlloc (ray, context.FingerSizeSphereCastRadius * AmbiguityRadiusAsRatioOfFingerSize, hitResultsBuffer, Camera.main.farClipPlane, layerMask);
            bool isClickAmbiguous;
            {
                bool anyHitIsNotInteractedObject = false;
                for (int i = 0; i < numberOfHits; ++i)
                {
                    if (!object.ReferenceEquals (hitResultsBuffer[i].collider.gameObject, interactedObject))
                    {
                        anyHitIsNotInteractedObject = true;
                        break;
                    }
                }
                isClickAmbiguous = anyHitIsNotInteractedObject;
            }
            if (isClickAmbiguous)
            {
                interactedObject = null;
                goto HandleInteractedObject;
            }


HandleInteractedObject:
    
            if (isHandlingOnDown)
            {
                context.fingerDownScreenPoint = Input.mousePosition;
                contextLayer.pressedObject = interactedObject;
                contextLayer.fingerLeftPressedObject = false;
                contextLayer.pressedObjectNeedsLongPress = true;
                if (null == interactedObject)
                {
                    return;
                }
                interactedObject.SendMessage ("OnFingerDown", SendMessageOptions.DontRequireReceiver);
            }
            else if (isHandlingOnUp)
            {
                var graphicRaycasters = GameObject.FindObjectsOfType (typeof(UnityEngine.UI.GraphicRaycaster));
                for (int i = 0; i < graphicRaycasters.Length; ++i)
                {
                    var raycaster = (UnityEngine.UI.GraphicRaycaster)graphicRaycasters[i];
                    raycaster.enabled = true;
                }
                var pressedObject = contextLayer.pressedObject;
                if (null == interactedObject)
                {
                    if (null != pressedObject)
                    {
                        pressedObject.SendMessage ("OnFingerUp", SendMessageOptions.DontRequireReceiver);
                    }
                    return;
                }
                interactedObject.SendMessage ("OnFingerUp", SendMessageOptions.DontRequireReceiver);
                if (null != pressedObject)
                {
                    if (object.ReferenceEquals (pressedObject, interactedObject))
                    {
                        if (!contextLayer.fingerLeftPressedObject)
                        {
                            pressedObject.SendMessage ("OnFingerTap", SendMessageOptions.DontRequireReceiver);
                        }
                    }
                    else
                    {
                        pressedObject.SendMessage ("OnFingerUp", SendMessageOptions.DontRequireReceiver);
                    }
                }
            }
        }
        else
        {
            var pressedObject = contextLayer.pressedObject;
            if (null == pressedObject)
            {
                return;
            }
            if (!contextLayer.fingerLeftPressedObject)
            {
                bool fingerOutsidePressedObject;
                {
                    var ray = Camera.main.ScreenPointToRay (Input.mousePosition);
                    fingerOutsidePressedObject = !sphereCastHitsGameObject (contextLayer.pressedObject, ray, context.FingerSizeSphereCastRadius, Camera.main.farClipPlane, layerMask);
                }
                if (fingerOutsidePressedObject)
                {
                    contextLayer.fingerLeftPressedObject = true;
                }
                if (contextLayer.pressedObjectNeedsLongPress)
                {
                    if (fingerOutsidePressedObject)
                    {
                        contextLayer.pressedObjectNeedsLongPress = false;
                    }
                    else
                    {
                        if ((Time.time - context.tapDownTime) > LongPressDelaySeconds)
                        {
                            pressedObject.SendMessage ("OnFingerLongPress", SendMessageOptions.DontRequireReceiver);
                            contextLayer.pressedObjectNeedsLongPress = false;
                        }
                    }
                }
            }
            var delta = removeZ (Input.mousePosition) - context.fingerDownScreenPoint;
            pressedObject.SendMessage ("OnFingerDrag", delta, SendMessageOptions.DontRequireReceiver);
        }
	}

    private static bool sphereCastHitsGameObject (GameObject gameObject, Ray ray, float radius, float distance, int layerMask)
    {
        int results = Physics.SphereCastNonAlloc (ray, radius, hitResultsBuffer, distance, layerMask);
        for (int i = 0; i < results; ++i)
        {
            if (object.ReferenceEquals (hitResultsBuffer[i].collider.gameObject, gameObject))
            {
                return true;
            }
        }
        return false;
    }

    private static Vector2 removeZ (Vector3 v)
    {
        return new Vector2 (v.x, v.y);
    }

    private int findClosestHitIndex (Vector2 hitCoordinate, Vector2[] objectCoordinates)
    {
        int closest = -1;
        float closestDistance = Screen.width * Screen.width + Screen.height * Screen.height;
        for (int i = 0; i < objectCoordinates.Length; ++i)
        {
            float distance = (hitCoordinate - objectCoordinates[i]).sqrMagnitude;
            if (distance < closestDistance)
            {
                closestDistance = distance;
                closest = i;
            }
        }
        return closest;
    }

    private static bool otherPointsAreWithinRadius (
        Vector2 point,
        float radius,
        Vector2[] pointsToTest,
        int numberOfPointsToTest,
        int indexToIgnore
        )
    {
        float radiusSq = radius * radius;
        for (int i = 0; i < numberOfPointsToTest; ++i)
        {
            if (i == indexToIgnore)
            {
                continue;
            }
            float distance = (point - pointsToTest[i]).sqrMagnitude;
            if (distance < radiusSq)
            {
                return true;
            }
        }
        return false;
    }
}
