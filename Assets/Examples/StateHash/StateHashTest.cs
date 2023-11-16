using System.Collections;
using BeauUtil;
using BeauUtil.Debugger;
using UnityEngine;

public class StateHashTest : MonoBehaviour
{
    [AssetOnly] public Camera TransformPrefab;
    public int TransformCount;
    public int TestIterations;
    public int TestWarmup;

    private IEnumerator Start()
    {
        Transform[] transforms = new Transform[TransformCount];
        Camera[] cameras = new Camera[TransformCount];
        for (int i = 0; i < TransformCount; i++)
        {
            cameras[i] = Instantiate(TransformPrefab);
            transforms[i] = cameras[i].transform;
            if ((i % 100) == 0)
            {
                yield return null;
            }
        }

        yield return TestCameras(cameras, transforms, TestIterations, TestWarmup);
        yield return TestTransforms(transforms, TestIterations, TestWarmup);
    }

    static private IEnumerator TestTransforms(Transform[] transforms, int iterationCount, int warmup)
    {
        long minTicks = long.MaxValue;
        long maxTicks = long.MinValue;
        RingBuffer<long> averages = new RingBuffer<long>(iterationCount, RingBufferMode.Expand);

        int transformCount = transforms.Length;
        int iterations = iterationCount;

        while (iterations > 0)
        {
            for (int i = 0; i < transformCount; i++)
            {
                transforms[i].SetLocalPositionAndRotation(RNG.Instance.NextVector3(2, 200), Quaternion.Euler(RNG.Instance.NextVector3(0, 360)));
            }
            yield return null;

            long start = Profiling.NowTicks();
            for (int i = 0; i < transformCount; i++)
            {
                transforms[i].GetStateHash();
            }
            long total = Profiling.NowTicks() - start;

            if (warmup > 0)
            {
                warmup--;

                Debug.LogFormat("TRANSFORM: Finished warmup iteration");
            }
            else
            {
                if (total < minTicks)
                    minTicks = total;
                if (total > maxTicks)
                    maxTicks = total;

                averages.PushBack(total);
                iterations--;

                Debug.LogFormat("TRANSFORM: Finished one iteration, {0} to go", iterations);
            }


            yield return null;
        }

        ulong averager = 0;
        foreach (var val in averages)
            averager += (ulong) val;
        double averageTicks = (averager / (double) averages.Count);

        double min = Profiling.TicksToEstCycles(minTicks) / transformCount;
        double avg = Profiling.TicksToEstCycles(averageTicks) / transformCount;
        double max = Profiling.TicksToEstCycles(maxTicks) / transformCount;

        UnityEngine.Debug.LogFormat("TRANSFORM: {0} iterations | {1} items | {2:0.00}cycles average | {3:0.00}cycles min | {4:0.00}cycles max", iterationCount, transformCount, avg, min, max);
    }

    static private IEnumerator TestCameras(Camera[] cameras, Transform[] transforms, int iterationCount, int warmup)
    {
        long minTicks = long.MaxValue;
        long maxTicks = long.MinValue;
        RingBuffer<long> averages = new RingBuffer<long>(iterationCount, RingBufferMode.Expand);

        int transformCount = cameras.Length;
        int iterations = iterationCount;

        while (iterations > 0)
        {
            for (int i = 0; i < transformCount; i++)
            {
                transforms[i].SetLocalPositionAndRotation(RNG.Instance.NextVector3(2, 200), Quaternion.Euler(RNG.Instance.NextVector3(0, 360)));
                cameras[i].fieldOfView = RNG.Instance.NextFloat(10, 120);
            }
            yield return null;

            long start = Profiling.NowTicks();
            for (int i = 0; i < transformCount; i++)
            {
                cameras[i].GetStateHash();
            }
            long total = Profiling.NowTicks() - start;

            if (warmup > 0)
            {
                warmup--;

                Debug.LogFormat("CAMERA: Finished warmup iteration");
            }
            else
            {
                if (total < minTicks)
                    minTicks = total;
                if (total > maxTicks)
                    maxTicks = total;

                averages.PushBack(total);
                iterations--;

                Debug.LogFormat("CAMERA: Finished one iteration, {0} to go", iterations);
            }

            yield return null;
        }

        ulong averager = 0;
        foreach (var val in averages)
            averager += (ulong) val;
        double averageTicks = (averager / (double) averages.Count);

        double min = Profiling.TicksToEstCycles(minTicks) / transformCount;
        double avg = Profiling.TicksToEstCycles(averageTicks) / transformCount;
        double max = Profiling.TicksToEstCycles(maxTicks) / transformCount;

        UnityEngine.Debug.LogFormat("CAMERA: {0} iterations | {1} items | {2:0.00}cycles average | {3:0.00}cycles min | {4:0.00}cycles max", iterationCount, transformCount, avg, min, max);
    }
}