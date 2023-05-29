using System.Collections;
using System.Collections.Generic;
using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;

public class DotsExampleManager : MonoBehaviour
{
    [SerializeField] private DotsCubeController _dotsExamplePrefab;
    List<DotsCubeController> _spawnedDotsCubes = new List<DotsCubeController>();
    NativeArray<float3> _cubesPosition;

    FindClosestJob _findClosestJob;
    JobHandle _handler;

    [ContextMenu("Spawn")]
    public void Spawn()
    {
        for(int i = 0; i < 1000; i++)
        {
            var cube = Instantiate(_dotsExamplePrefab, UnityEngine.Random.insideUnitSphere * 100f, Quaternion.identity ,transform);
            _spawnedDotsCubes.Add(cube);
        }
    }

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
        var result = _findClosestJob.result;

        for (int i = 0; i < spawnedDotsCubeCount / 2; i++)
        {
            _spawnedDotsCubes[i].ClosestDotsCube = _spawnedDotsCubes[result[i]];
        }
    }

    public void UpdatePositionDots()
    {
        var spawnedDotsCubesCount = _spawnedDotsCubes.Count;
        _cubesPosition = new NativeArray<float3>(spawnedDotsCubesCount, Allocator.TempJob, NativeArrayOptions.UninitializedMemory);
        var result1 = new NativeArray<int>(spawnedDotsCubesCount, Allocator.TempJob, NativeArrayOptions.UninitializedMemory);

        for(int i = 0; i < spawnedDotsCubesCount; i++)
        {
            _cubesPosition[i] = _spawnedDotsCubes[i].transform.position;
        }

        _findClosestJob = new FindClosestJob
        {
            positionOffset = 0,
            length = spawnedDotsCubesCount,
            cubesPosition = _cubesPosition,
            result = result1
        };

        _handler = _findClosestJob.Schedule();

    }

    [BurstCompile]
    public struct FindClosestJob : IJob
    {
        public int length;
        public int positionOffset;
        [ReadOnly]public NativeArray<float3> cubesPosition;
        [WriteOnly]public NativeArray<int> result;

        public void Execute()
        {
            var distances = new NativeArray<float>(length, Allocator.Temp, NativeArrayOptions.UninitializedMemory);
            for(int i = 0; i < length; i++)
            {
                distances[i] = math.INFINITY;
            }

            for(int i = positionOffset; i < length; i++)
            {
                for(int j = 0; j < length; j++)
                {
                    if(i == j)
                    {
                        continue;
                    }

                    var newDistance = math.distance(cubesPosition[i], cubesPosition[j]);
                    if(newDistance < distances[i])
                    {
                        distances[i] = newDistance;
                        result[i] = j;
                    }

                }
            }

        }
    }


}
