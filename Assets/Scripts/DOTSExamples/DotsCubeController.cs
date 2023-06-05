using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class DotsCubeController : MonoBehaviour
{
    public Vector3[] ClosestDotsCube { get; set; }
    public Vector3 FarthestDotsCube { get; set; }

    private void OnDrawGizmosSelected()
    {
        if (ClosestDotsCube != null)
        {

            Gizmos.color = Color.green;
            for (int i = 0; i < 3; i++)
            {
                Gizmos.DrawLine(transform.position, ClosestDotsCube[i]);
            }

        }

        Gizmos.color = Color.red;
        Gizmos.DrawLine(transform.position, FarthestDotsCube);

        
    }

}
