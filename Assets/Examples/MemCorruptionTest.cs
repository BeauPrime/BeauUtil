using System;
using System.Collections;
using System.Collections.Generic;
using BeauUtil;
using BeauUtil.Tags;
using UnityEngine;

public class MemCorruptionTest : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        Debug.Log("Potentially Broken Test");

        //Debug.Log("sizeof NonBoxedValue = " + Unsafe.SizeOf<NonBoxedValue>());
        StartCoroutine(BeauRoutineStart());
    }

    IEnumerator BeauRoutineStart()
    {
        var m = new MethodCache<MethodTagAttribute>(typeof(MonoBehaviour), new DefaultStringConverter());
        yield return null;

        yield return null;

        m.LoadStatic();

        while (true)
        {
            m.TryStaticInvoke("SomeLogging", StringSlice.Empty, null, out var _);
            m.TryStaticInvoke("GarbageCollect", StringSlice.Empty, null, out var thing);
            yield return (IEnumerator) thing.AsObject();
            m.TryStaticInvoke("SomeLogging", StringSlice.Empty, null, out var _);
            yield return new WaitForSeconds(1);
        }
    }

    private class MethodTagAttribute : ExposedAttribute
    {

    }

    [MethodTagAttribute]
    static private void SomeLogging()
    {
        Debug.Log("haha i'm doing something");
    }

    [MethodTagAttribute]
    static private IEnumerator GarbageCollect()
    {
        Debug.Log("collecting garbage...");
        byte[] garbage = new byte[Unsafe.MiB * 5];
        GC.Collect();
        yield return new WaitForSeconds(1);
        Debug.Log("garbage collected...");
    }
}
