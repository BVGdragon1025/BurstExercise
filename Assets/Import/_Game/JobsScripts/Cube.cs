using UnityEngine;

namespace _Game.JobsScripts
{
    public class Cube : MonoBehaviour
    {
        public Vector3[] Closest { get; set; }
        public Vector3 Farthest { get; set; }

        private void OnDrawGizmosSelected()
        {
            if (Closest is not null)
            {
                Gizmos.color = Color.green;
                for (int i = 0; i < 3; i++)
                    Gizmos.DrawLine(transform.position, Closest[i]);
            }

            Gizmos.color = Color.red;
            Gizmos.DrawLine(transform.position, Farthest);

        }
    }
}