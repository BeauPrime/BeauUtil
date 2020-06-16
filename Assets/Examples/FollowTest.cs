using System;
using System.Collections;
using System.Runtime.InteropServices;
using UnityEngine;

namespace BeauUtil.Examples
{
    public class FollowTest : MonoBehaviour
    {
        [StructLayout(LayoutKind.Sequential, Pack = 4)]
        private struct FollowData
        {
            public Vector2 Position;
            public float Timestamp;
            public float Duration;

            public override string ToString()
            {
                return string.Format("{0} (length {1}): {2}", Timestamp, Duration, Position);
            }
        }

        [SerializeField]
        private Transform m_FollowerRoot = null;

        [SerializeField]
        private float m_FollowerDelay = 1f;

        [SerializeField]
        private int m_SampleRate = 15;

        [SerializeField]
        private float m_LerpStrength = 2;

        [NonSerialized] private Transform[] followers;
        [NonSerialized] private RingBuffer<FollowData> positionBuffer;
        [NonSerialized] private float currentTime;

        private IEnumerator Start()
        {
            yield return null;
            yield return null;

            Debug.LogFormat("sizeof(FollowData)={0}", Marshal.SizeOf<FollowData>());

            followers = new Transform[m_FollowerRoot.childCount];
            for(int i = 0; i < followers.Length; ++i)
            {
                followers[i] = m_FollowerRoot.GetChild(i);
            }

            int bufferSize = (int) Math.Ceiling((m_SampleRate + 1) * m_FollowerDelay * followers.Length);

            positionBuffer = new RingBuffer<FollowData>(bufferSize, RingBufferMode.Overwrite);
            Debug.LogFormat("{0} followers, sample rate {1}, buffer size {2}", followers.Length, m_SampleRate, positionBuffer.Capacity);
            currentTime = 0;

            {
                FollowData data = new FollowData()
                {
                    Position = transform.position,
                    Timestamp = 0,
                    Duration =  1f / m_SampleRate
                };

                positionBuffer.PushFront(data);
            }
        }

        private void Update()
        {
            if (followers == null)
                return;

            {
                Vector3 mousePos = Input.mousePosition;
                mousePos.z = 10;
                Vector3 target = Camera.main.ScreenToWorldPoint(mousePos);
                target.z = transform.position.z;

                float lerp = 1.0f - (float) Math.Exp(-m_LerpStrength * Time.deltaTime);
                transform.position = Vector3.Lerp(transform.position, target, lerp);
            }

            float prevTime = currentTime;
            float nextTime = currentTime + Time.deltaTime;

            int prevSampleCount = (int) (prevTime * m_SampleRate);
            int nextSampleCount = (int) (nextTime * m_SampleRate);
            if (prevSampleCount < nextSampleCount)
            {
                FollowData data = new FollowData()
                {
                    Position = transform.position,
                    Timestamp = (float) nextSampleCount / m_SampleRate,
                    Duration =  (float) (nextSampleCount - prevSampleCount) / m_SampleRate
                };

                positionBuffer.PushFront(data);
            }

            currentTime = nextTime;

            float minSep = 1f / m_SampleRate;

            for (int i = 0; i < followers.Length; ++i)
            {
                Transform follower = followers[i];
                float targetTimestamp = currentTime - minSep - m_FollowerDelay * (i + 1);
                int posIdx = positionBuffer.BinarySearch(targetTimestamp, Finder, SearchFlags.IsReversed);
                if (posIdx >= 0 && posIdx < positionBuffer.Count - 1)
                {
                    ref FollowData target = ref positionBuffer[posIdx];
                    ref FollowData prev = ref positionBuffer[posIdx + 1];
                    float lerp = (targetTimestamp - prev.Timestamp) / target.Duration;
                    // Debug.LogFormat("Following {0}:{1}", i, posIdx);
                    Vector3 positionTarget = Vector3.Lerp(prev.Position, target.Position, lerp);
                    positionTarget.z = transform.position.z;
                    follower.position = positionTarget;
                    follower.gameObject.SetActive(true);
                }
                else
                {
                    follower.gameObject.SetActive(false);
                    if (targetTimestamp >= 0)
                        Debug.LogWarningFormat("Unable to find target for {0} at timestamp {1}", i, targetTimestamp);
                }
            }
        }

        private void LateUpdate()
        {
            if (Input.GetMouseButton(1))
            {
                Time.timeScale = 0.2f;
            }
            else
            {
                Time.timeScale = 1;
            }
        }

        static private readonly ComparePredicate<FollowData, float> Finder = (f, ts) =>
        {
            if (ts < f.Timestamp - f.Duration)
            {
                return 1;
            }

            if (ts >= f.Timestamp)
            {
                return -1;
            }

            return 0;
        };
    }
}