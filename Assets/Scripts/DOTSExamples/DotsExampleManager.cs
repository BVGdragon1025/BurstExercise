using System.Collections.Generic;
using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;

public class DotsExampleManager : MonoBehaviour
{
    [SerializeField] private DotsCubeController _dotsExamplePrefab;
    [SerializeField] private int _dotsCubeAmount;
    List<DotsCubeController> _spawnedDotsCubes = new();
    NativeArray<float3> _cubesPosition;

    FindClosestAndFurthestJob _findClosestAndFurthestJob;
    JobHandle _handler;

    private void Awake()
    {
        Spawn();
    }

    private void Update()
    {
        UpdatePositionDots();
    }

    private void LateUpdate()
    {
        _handler.Complete();

        var spawnedDotsCubeCount = _spawnedDotsCubes.Count;
        var result = _findClosestAndFurthestJob.result;

        for (int i = 0; i < spawnedDotsCubeCount; i++)
        {
            _spawnedDotsCubes[i].ClosestDotsCube = new[]
            {
                _spawnedDotsCubes[result[i].close1].transform.position,
                _spawnedDotsCubes[result[i].close2].transform.position,
                _spawnedDotsCubes[result[i].close3].transform.position,
            };
            _spawnedDotsCubes[i].FarthestDotsCube = _spawnedDotsCubes[result[i].far].transform.position;

        }
    }

    [ContextMenu("Spawn")]
    public void Spawn()
    {
        for(int i = 0; i < _dotsCubeAmount; i++)
        {
            var cube = Instantiate(_dotsExamplePrefab, UnityEngine.Random.insideUnitSphere * 100f, Quaternion.identity ,transform);
            _spawnedDotsCubes.Add(cube);

        }
    }

    public void UpdatePositionDots()
    {
        var spawnedDotsCubesCount = _spawnedDotsCubes.Count;
        _cubesPosition = new NativeArray<float3>(spawnedDotsCubesCount, Allocator.TempJob, NativeArrayOptions.UninitializedMemory);
        var result = new NativeArray<DotsCubesNearby>(spawnedDotsCubesCount, Allocator.TempJob, NativeArrayOptions.UninitializedMemory);

        for(int i = 0; i < spawnedDotsCubesCount; i++)
        {
            _cubesPosition[i] = _spawnedDotsCubes[i].transform.position;
        }

        _findClosestAndFurthestJob = new FindClosestAndFurthestJob
        {
            length = spawnedDotsCubesCount,
            cubesPosition = _cubesPosition,
            result = result
        };

        _handler = _findClosestAndFurthestJob.Schedule();

    }

    [BurstCompile]
    public struct FindClosestAndFurthestJob : IJob
    {
        public int length;
        [ReadOnly]public NativeArray<float3> cubesPosition;
        [WriteOnly]public NativeArray<DotsCubesNearby> result;

        public void Execute()
        {
            for(int i = 0; i < length; i++)
            {
                result[i] = GetClosestAndFarthestCube(i, cubesPosition[i]);
            }
        }

        private DotsCubesNearby GetClosestAndFarthestCube(int currentIndex, float3 currentPosition)
        {
            var closestDistances = new NativeArray<float>(3, Allocator.Temp, NativeArrayOptions.UninitializedMemory);
            var closestIndexes = new NativeArray<int>(3, Allocator.Temp, NativeArrayOptions.UninitializedMemory);

            var furthestDistance = float.MinValue;
            var furthestIndex = -1;

            for(int i = 0; i < 3; i++)
            {
                closestDistances[i] = float.MaxValue;
                closestIndexes[i] = -1;
            }

            for (int i = 0; i < length; i++)
            {
                if (i == currentIndex)
                {
                    continue;
                }

                var newDistance = math.distance(currentPosition, cubesPosition[i]);

                for (int j = 0; j < 3; j++)
                {
                    if (newDistance < closestDistances[j])
                    {
                        for (int k = 3 - 1; k > j; k--)
                        {
                            closestDistances[k] = closestDistances[k - 1];
                            closestIndexes[k] = closestIndexes[k - 1];

                        }

                        closestDistances[j] = newDistance;
                        closestIndexes[j] = i;
                        break;
                    }
                }

                if(newDistance > furthestDistance)
                {
                    furthestDistance = newDistance;
                    furthestIndex = i;
                }
            }

            return new DotsCubesNearby()
            {
                close1 = closestIndexes[0],
                close2 = closestIndexes[1],
                close3 = closestIndexes[2],
                far = furthestIndex
            };
        }
    }


}
