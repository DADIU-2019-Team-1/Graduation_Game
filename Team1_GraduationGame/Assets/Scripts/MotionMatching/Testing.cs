using System.Collections;
using System.Collections.Generic;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;
using Unity.Collections;
using Unity.Burst;


public class Testing : MonoBehaviour
{
    [SerializeField] private bool useJobs;
    [SerializeField] private Transform pfZombie;
    private List<Zombie> zombieList;

    public class Zombie
    {
        public Transform transform;
        public float moveY; 
    }

    private void Start()
    {
        zombieList = new List<Zombie>();
        for (int i = 0; i < 1000; i++)
        {
            Transform zombieTransform = Instantiate(pfZombie,
                new Vector3(UnityEngine.Random.Range(-8f, 8f), UnityEngine.Random.Range(-5f, 5f)), Quaternion.identity);

            zombieList.Add(new Zombie
            {
                transform = zombieTransform,
                moveY = UnityEngine.Random.Range(1f, 2f)
            });
        }
    }

    void Update()
    {
        float startTime = Time.realtimeSinceStartup;
        if (useJobs)
        {
            NativeArray<float3> positionArray = new NativeArray<float3>(zombieList.Count, Allocator.TempJob);
            NativeArray<float> moveYArray = new NativeArray<float>(zombieList.Count, Allocator.TempJob);

            for (int i = 0; i < zombieList.Count; i++)
            {
                positionArray[i] = zombieList[i].transform.position;
                moveYArray[i] = zombieList[i].moveY;
            }

            ToughParralelJob parallelJob = new ToughParralelJob
            {
                deltaTime = Time.deltaTime,
                positionArray = positionArray,
                moveYArray = moveYArray
            };

            JobHandle handle = parallelJob.Schedule(zombieList.Count, 100);
            handle.Complete();

            for (int i = 0; i < zombieList.Count; i++)
            {
                zombieList[i].transform.position = positionArray[i];
                zombieList[i].moveY = moveYArray[i];
            }

            positionArray.Dispose();
            moveYArray.Dispose();
        }
        else
        {
            foreach (Zombie zombie in zombieList)
            {
                zombie.transform.position += new Vector3(0, zombie.moveY * Time.deltaTime);
                if (zombie.transform.position.y > 5f)
                {
                    zombie.moveY = -math.abs(zombie.moveY);
                }
                if (zombie.transform.position.y < -5f)
                {
                    zombie.moveY = +math.abs(zombie.moveY);
                }
                float value = 0f;
                for (int i = 0; i < 50000; i++)
                {
                    value = math.exp10(math.sqrt(value));
                }
            }
        }
        //if (useJobs)
        //{
        //    NativeList<JobHandle> jobHandleList = new NativeList<JobHandle>(Allocator.Temp);
        //    for (int i = 0; i < 10; i++)
        //    {
        //        JobHandle jobHandle = ToughJobTask();
        //        jobHandleList.Add(jobHandle);
        //    }
        //    JobHandle.CompleteAll(jobHandleList);
        //    jobHandleList.Dispose();
        //}
        //else
        //{
        //    for (int i = 0; i < 10; i++)
        //    {
        //        Task();
        //    }
        //}

        Debug.Log((Time.realtimeSinceStartup - startTime) * 1000f + "ms");
    }

    private void Task()
    {
        float value = 0f;
        for (int i = 0; i < 50000; i++)
        {
            value = math.exp10(math.sqrt(value));
        }
    }

    private JobHandle ToughJobTask()
    {
        ToughJob job = new ToughJob();
        return job.Schedule();
    }
}

[BurstCompile]
public struct ToughJob : IJob
{
    public void Execute()
    {
        float value = 0f;
        for (int i = 0; i < 50000; i++)
        {
            value = math.exp10(math.sqrt(value));
        }
    }
}

[BurstCompile]
public struct ToughParralelJob : IJobParallelFor
{
    public NativeArray<float3> positionArray;
    public NativeArray<float> moveYArray;
    public float deltaTime;
    public void Execute(int index)
    {
        positionArray[index] += new float3(0, moveYArray[index] * deltaTime, 0f);
        if (positionArray[index].y > 5f)
        {
            moveYArray[index] = -math.abs(moveYArray[index]);
        }
        if (positionArray[index].y < -5f)
        {
            moveYArray[index] = +math.abs(moveYArray[index]);
        }
        float value = 0f;
        for (int i = 0; i < 50000; i++)
        {
            value = math.exp10(math.sqrt(value));
        }
    }
}