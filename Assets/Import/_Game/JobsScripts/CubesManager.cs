using System.Collections.Generic;
using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Serialization;

namespace _Game.JobsScripts
{
    public class CubesManager : MonoBehaviour
    {
        [FormerlySerializedAs("_prefab")] [SerializeField] private Cube prefab;
        [SerializeField] private int minFPS;
        
        private bool _lowMemory;
        private int _spawnFramesOffset = 10;
        private int _currentOffset;
        private float _deltaTime;
        
        private List<Cube> _spawnedCubes = new List<Cube>();
        
        private NativeArray<float3> _cubesPosition;
        private FindClosestJob _findClosestJob;
        private JobHandle _handler;

        private void Awake() => Spawn();

        private void OnEnable() => Application.lowMemory += () => _lowMemory = true;
        
        private void OnDisable() => Application.lowMemory -= () => _lowMemory = true;

        private void Update()
        {
            _currentOffset++;
            if (_currentOffset > _spawnFramesOffset && !_lowMemory)
            {
                _deltaTime += (Time.unscaledDeltaTime - _deltaTime) * 0.1f;
                if(1.0f / _deltaTime > minFPS)
                    Spawn();
                _currentOffset = 0;
            }
            
            UpdatePositionDots();
        }

        private void LateUpdate()
        {
            _handler.Complete();

            var spawnedDotsCubeCount = _spawnedCubes.Count;
            var result = _findClosestJob.result;

            for (int i = 0; i < spawnedDotsCubeCount; i++)
            {
                _spawnedCubes[i].Closest = new[] 
                {
                    _spawnedCubes[result[i].first].transform.position,
                    _spawnedCubes[result[i].second].transform.position,
                    _spawnedCubes[result[i].third].transform.position,
                };
                _spawnedCubes[i].Farthest = _spawnedCubes[result[i].farthest].transform.position;
            }
        }
        
        [ContextMenu("Spawn")]
        public void Spawn()
        {
            for(int i = 0; i < 1000; i++)
                _spawnedCubes.Add(Instantiate(prefab, UnityEngine.Random.insideUnitSphere * 100f, Quaternion.identity, transform));
        }
        
        public void UpdatePositionDots()
        {
            var spawnedDotsCubesCount = _spawnedCubes.Count;
            _cubesPosition = new NativeArray<float3>(spawnedDotsCubesCount, Allocator.TempJob, NativeArrayOptions.UninitializedMemory);
            var result1 = new NativeArray<ClosestAndFarthest>(spawnedDotsCubesCount, Allocator.TempJob, NativeArrayOptions.UninitializedMemory);

            for(int i = 0; i < spawnedDotsCubesCount; i++)
                _cubesPosition[i] = _spawnedCubes[i].transform.position;

            _findClosestJob = new FindClosestJob
            {
                length = spawnedDotsCubesCount,
                cubesPosition = _cubesPosition,
                result = result1
            };

            _handler = _findClosestJob.Schedule();
            //_handler = _findClosestJob.Schedule(spawnedDotsCubesCount, 32);

        }

        [BurstCompile]
        private struct FindClosestJob : IJob
        {
            public int length;
            [ReadOnly] public NativeArray<float3> cubesPosition;
            [WriteOnly] public NativeArray<ClosestAndFarthest> result;

            public void Execute()
            {
                for (int i = 0; i < length; i++)
                    result[i] = GetClosestAndFarthest(i, cubesPosition[i]);
                
            }

            private ClosestAndFarthest GetClosestAndFarthest(int currentIndex, float3 currentPosition)
            {
                var closestDistances = new NativeArray<float>(3, Allocator.Temp, NativeArrayOptions.UninitializedMemory);
                var closestIndices = new NativeArray<int>(3, Allocator.Temp, NativeArrayOptions.UninitializedMemory);
                
                var farthestDistance = float.MinValue;
                var farthestIndex = -1;

                for (int i = 0; i < 3; i++)
                {
                    closestDistances[i] = float.MaxValue;
                    closestIndices[i] = -1;
                }
                
                for (int i = 0; i < length; i++)
                {
                    if (i == currentIndex)
                        continue;
                    
                    var distance = math.distance(currentPosition, cubesPosition[i]);

                    // Check if the current distance is smaller than the stored distances
                    for (int j = 0; j < 3; j++)
                    {
                        if (distance < closestDistances[j])
                        {
                            // Shift the other closest distances and indices
                            for (int k = 3 - 1; k > j; k--)
                            {
                                closestDistances[k] = closestDistances[k - 1];
                                closestIndices[k] = closestIndices[k - 1];
                            }

                            // Store the new closest distance and index
                            closestDistances[j] = distance;
                            closestIndices[j] = i;
                            break;
                        }
                    }
                    // Check if the current distance is larger than the farthest distance
                    if (distance > farthestDistance)
                    {
                        farthestDistance = distance;
                        farthestIndex = i;
                    }
                }

                return new ClosestAndFarthest()
                {
                    first = closestIndices[0],
                    second = closestIndices[1],
                    third = closestIndices[2],
                    farthest = farthestIndex
                };
            }
        }
        
        /*[BurstCompile]
        private struct FindClosestJob : IJobParallelFor
        {
            public int length;
            [ReadOnly] public NativeArray<float3> cubesPosition;
            //[WriteOnly]
            public NativeArray<ClosestAndFarthest> result;

            public void Execute(int index)
            {
                result[index] = GetClosestAndFarthest(index, cubesPosition[index]);
            }

            private ClosestAndFarthest GetClosestAndFarthest(int currentIndex, float3 currentPosition)
            {
                var closestDistances = new NativeArray<float>(3, Allocator.Temp, NativeArrayOptions.UninitializedMemory);
                var closestIndices = new NativeArray<int>(3, Allocator.Temp, NativeArrayOptions.UninitializedMemory);

                var farthestDistance = float.MinValue;
                var farthestIndex = -1;

                for (int i = 0; i < 3; i++)
                {
                    closestDistances[i] = float.MaxValue;
                    closestIndices[i] = -1;
                }

                for (int i = 0; i < length; i++)
                {
                    if (i == currentIndex)
                        continue;

                    var distance = math.distance(currentPosition, cubesPosition[i]);

                    // Check if the current distance is smaller than the stored distances
                    for (int j = 0; j < 3; j++)
                    {
                        if (distance < closestDistances[j])
                        {
                            // Shift the other closest distances and indices
                            for (int k = 3 - 1; k > j; k--)
                            {
                                closestDistances[k] = closestDistances[k - 1];
                                closestIndices[k] = closestIndices[k - 1];
                            }

                            // Store the new closest distance and index
                            closestDistances[j] = distance;
                            closestIndices[j] = i;
                            break;
                        }
                    }
                    // Check if the current distance is larger than the farthest distance
                    if (distance > farthestDistance)
                    {
                        farthestDistance = distance;
                        farthestIndex = i;
                    }
                }

                closestDistances.Dispose();
                closestIndices.Dispose();

                return new ClosestAndFarthest()
                {
                    first = closestIndices[0],
                    second = closestIndices[1],
                    third = closestIndices[2],
                    farthest = farthestIndex
                };
            }
        }*/

    }
}