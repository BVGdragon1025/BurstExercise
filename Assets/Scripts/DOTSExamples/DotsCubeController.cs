using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class DotsCubeController : MonoBehaviour
{
    private DotsCubeController _closestDotsCube;

    public DotsCubeController closestDotsCube
    {
        get => _closestDotsCube;
        set
        {
            _closestDotsCube = value;
            closestDotsCubeTransform = closestDotsCube.transform;
        }
    }
    public Transform closestDotsCubeTransform { get; private set; }

    private void OnDrawGizmosSelected()
    {
        if (!closestDotsCube)
        {
            return;
        }

        Gizmos.color = Color.red;
        Gizmos.DrawLine(transform.position, closestDotsCube.transform.position);
        
    }

}
