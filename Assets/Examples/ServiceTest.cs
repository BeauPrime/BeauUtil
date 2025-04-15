using UnityEngine;
using BeauUtil.Services;
using BeauUtil.Debugger;
using System.Collections;
using System;
using BeauUtil;

public class ServiceTest : MonoBehaviour
{
    [ServiceReference] static private SubsystemB subsystemB;

    private IEnumerator Start()
    {
        yield return null;
        yield return null;

        Log.Msg("" + UnityHelper.Id(this));
        throw new NullReferenceException();
        Assert.Fail("blah");

        ServiceCache services = new ServiceCache();
        services.Process();

        services.Add(new SubsystemA());
        services.Add(new SubsystemB());
        services.Add(new SubsystemC());
        services.Add(new SubsystemD());
        services.Add(new SubsystemE());

        using(Profiling.Time("Processing Subsystem Init"))
        {
            services.Process();
        }

        services.ClearAll();
    }

    private class SubsystemA : IService
    {
        [ServiceReference] private SubsystemB subsystemB;

        public void InitializeService()
        {
            Debug.LogFormat("SubsystemA initialized");
        }

        public void ShutdownService()
        {
            Debug.LogFormat("SubsystemA shutdown");
        }
    }

    private class SubsystemB : IService
    {
        public void InitializeService()
        {
            Debug.LogFormat("SubsystemB initialized");
        }

        public void ShutdownService()
        {
            Debug.LogFormat("SubsystemB shutdown");
        }
    }

    private class SubsystemC : IService
    {
        [ServiceReference] private SubsystemB subsystemB;
        [ServiceReference] private SubsystemA subsystemA;

        public void InitializeService()
        {
            Debug.LogFormat("SubsystemC initialized");
        }

        public void ShutdownService()
        {
            Debug.LogFormat("SubsystemC shutdown");
        }
    }

    private class SubsystemD : IService
    {
        [ServiceReference] private SubsystemA subsystemA;

        public void InitializeService()
        {
            Debug.LogFormat("SubsystemD initialized");
        }

        public void ShutdownService()
        {
            Debug.LogFormat("SubsystemD shutdown");
        }
    }
    private class SubsystemE : IService
    {
        [ServiceReference] private SubsystemB subsystemA;

        public void InitializeService()
        {
            Debug.LogFormat("SubsystemE initialized");
        }

        public void ShutdownService()
        {
            Debug.LogFormat("SubsystemE shutdown");
        }
    }
}