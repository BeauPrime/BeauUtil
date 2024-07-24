using UnityEngine;
using BeauUtil;
using BeauUtil.Streaming;
using UnityEngine.Networking;
using System.IO;
using System.Collections;
using BeauUtil.Debugger;
using BeauUtil.IO;

public unsafe class DownloadTest : MonoBehaviour
{
    [StreamingPath] public string File0;
    [StreamingPath] public string File1;
    public ReloadableRef<TextAsset> AssetRef;
    [AssetOnly] public Transform Prefab;

    public string Url0;
    public string Url1;
    public int BufferSize = 512;

    private Unsafe.ArenaHandle m_Arena;
    private byte[] m_ChunkBuffer;

    private void Start() {
        m_ChunkBuffer = new byte[BufferSize];
        m_Arena = Unsafe.CreateArena(2 * 1024 * 1024);

        byte* testAlloc = (byte*) m_Arena.Alloc(64);

        for(int i = 0; i < 64; i++)
        {
            testAlloc[i] = 127;
        }

        StringHash32 hash = "whatever";

        Unsafe.Clear(testAlloc, 64);

        for(int i = 0; i < 64; i++)
        {
            if (testAlloc[i] != 0)
                Log.Error("zero did not work!");
        }

        Debug.Log("Is64: " + Unsafe.Is64);

        Enums.AreEqual(PrimitiveType.Capsule, PrimitiveType.Cube);
        Enums.Or(CollisionFlags.Below, CollisionFlags.Above);
        Enums.Not(CollisionFlags.Below);
        Enums.Xor(CollisionFlags.CollidedBelow, CollisionFlags.CollidedAbove);

        StartCoroutine(TestCoroutine());
    }

    private IEnumerator TestCoroutine()
    {
        UnityWebRequest file0Request = CreateFileStream(m_ChunkBuffer, File0);
        UnityWebRequest file1Request = CreateFileStream(m_ChunkBuffer, File1);
        file0Request.SendWebRequest();
        file1Request.SendWebRequest();

        while(!file0Request.isDone || !file1Request.isDone) {
            yield return null;
        }

        UnityWebRequest url0Request = CreateURLStream(m_ChunkBuffer, Url0);
        UnityWebRequest url1Request = CreateURLStream(m_ChunkBuffer, Url1);
        url0Request.SendWebRequest();
        url1Request.SendWebRequest();

        while(!url0Request.isDone || !url1Request.isDone) {
            yield return null;
        }

        UnityWebRequest file0BufferRequest = CreateFileBufferDownload(m_ChunkBuffer, File0, DownloadHandlerUnsafeBuffer.WriteLocation.Start);
        UnityWebRequest file1BufferRequest = CreateFileBufferDownload(m_ChunkBuffer, File1, DownloadHandlerUnsafeBuffer.WriteLocation.End);
        file0BufferRequest.SendWebRequest();
        file1BufferRequest.SendWebRequest();

        while(!file0BufferRequest.isDone || !file1BufferRequest.isDone) {
            yield return null;
        }

        LogBufferFinished(file0BufferRequest);
        LogBufferFinished(file1BufferRequest);
    }

    private UnityWebRequest CreateFileStream(byte[] chunkBuffer, string path) {
        DownloadHandlerStream streamBuffer = new DownloadHandlerStream(chunkBuffer, ReceiveBytes, path);
        UnityWebRequest webRequest = new UnityWebRequest(GetDownloadURL(path), UnityWebRequest.kHttpVerbGET, streamBuffer, null);
        return webRequest;
    }

    private UnityWebRequest CreateURLStream(byte[] chunkBuffer, string url) {
        DownloadHandlerStream streamBuffer = new DownloadHandlerStream(chunkBuffer, ReceiveBytes, url);
        UnityWebRequest webRequest = new UnityWebRequest(url, UnityWebRequest.kHttpVerbGET, streamBuffer, null);
        return webRequest;
    }

    private UnityWebRequest CreateFileBufferDownload(byte[] chunkBuffer, string path, DownloadHandlerUnsafeBuffer.WriteLocation location) {
        DownloadHandlerUnsafeBuffer buffer = new DownloadHandlerUnsafeBuffer(chunkBuffer, location);
        UnityWebRequest webRequest = new UnityWebRequest(GetDownloadURL(path), UnityWebRequest.kHttpVerbGET, buffer, null);
        return webRequest;
    }

    private unsafe bool ReceiveBytes(byte* buffer, int length, object context, int flags) {
        string dump = Unsafe.DumpMemory(buffer, length, ' ', 4);
        Debug.LogFormat("[Frame {0}] Received {1} bytes from '{2}':\n{3}", Time.frameCount, length, Path.GetFileName(context.ToString()), dump);
        return true;
    }

    private unsafe void LogBufferFinished(UnityWebRequest webRequest) {
        DownloadHandlerUnsafeBuffer buffer = (DownloadHandlerUnsafeBuffer) webRequest.downloadHandler;
        string dump = Unsafe.DumpMemory(buffer.DataHead, buffer.DataLength, ' ', 4);
        Debug.LogFormat("[Frame {0}] Finished downloading buffer '{1}' (size {2}):\n{3}", Time.frameCount, Path.GetFileName(webRequest.url), buffer.DataLength, dump);
        webRequest.Dispose();
    }

    private void OnDestroy() {
        Unsafe.TryDestroyArena(ref m_Arena);
    }

    static private string GetDownloadURL(string path) {
        string streamingPath = Path.Combine(Application.streamingAssetsPath, path);
        switch(Application.platform) {
            case RuntimePlatform.Android:
            case RuntimePlatform.WebGLPlayer: {
                return streamingPath;
            }

            case RuntimePlatform.WSAPlayerARM:
            case RuntimePlatform.WSAPlayerX64:
            case RuntimePlatform.WSAPlayerX86:
            case RuntimePlatform.WindowsEditor:
            case RuntimePlatform.WindowsPlayer: {
                return "file:///" + streamingPath;
            }

            default: {
                return "file://" + streamingPath;
            }
        }
    }
}