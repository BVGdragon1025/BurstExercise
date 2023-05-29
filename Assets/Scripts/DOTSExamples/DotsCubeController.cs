using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class DotsCubeController : MonoBehaviour
{
    [SerializeField]
    private DotsCubeController _closestDotsCube;

    public DotsCubeController ClosestDotsCube
    {
        get => _closestDotsCube;
        set
        {
            _closestDotsCube = value;
            ClosestDotsCubeTransform = ClosestDotsCube.transform;
        }
    }
    public Transform ClosestDotsCubeTransform { get; private set; }

    private void OnDrawGizmosSelected()
    {
        if (!ClosestDotsCube)
        {
            return;
        }

        Gizmos.color = Color.red;
        Gizmos.DrawLine(transform.position, ClosestDotsCubeTransform.position);
        
    }

}
