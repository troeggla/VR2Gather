﻿#define NO_VOICE

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WebCamPipeline : MonoBehaviour {
    public int              width = 1280;
    public int              height = 720;
    public int              fps = 12;
    bool                    ready = false;

    public Texture2D        texture;
    public WebCamTexture    webCamTexture;


    bool isSource = false;
    Workers.WebCamReader    webReader;
    Workers.BaseWorker      reader;
    Workers.BaseWorker      encoder;
    Workers.VideoDecoder    decoder;
    Workers.BaseWorker      writer;
    Workers.VideoPreparer   preparer;

    VoiceSender audioSender;
    VoiceReceiver audioReceiver;

    QueueThreadSafe encoderQueue;
    QueueThreadSafe writerQueue         = new QueueThreadSafe();
    QueueThreadSafe videoCodecQueue     = new QueueThreadSafe();
    QueueThreadSafe videoPreparerQueue  = new QueueThreadSafe();

    TilingConfig tilingConfig;  // Information on pointcloud tiling and quality levels

    const bool debugTiling = false;

    /// <summary> Orchestrator based Init. Start is called before the first frame update </summary> 
    /// <param name="cfg"> Config file json </param>
    /// <param name="url_pcc"> The url for pointclouds from sfuData of the Orchestrator </param> 
    /// <param name="url_audio"> The url for audio from sfuData of the Orchestrator </param>
    public WebCamPipeline Init(OrchestratorWrapping.User user, Config._User cfg, bool useDash, bool preview = false) {
        if (user!=null && user.userData != null && user.userData.webcamName == "None") return this;
        switch (cfg.sourceType) {
            case "self": // Local
                isSource = true;
                //
                // Allocate queues we need for this sourceType
                //
                encoderQueue = new QueueThreadSafe(2, true);
                //
                // Create reader
                //
                webReader = new Workers.WebCamReader(user.userData.webcamName, width, height, fps, this, encoderQueue);
                webCamTexture = webReader.webcamTexture;
                if (!preview) {
                    //
                    // Create encoders for transmission
                    //
                    try {
                        encoder = new Workers.VideoEncoder(encoderQueue, null, writerQueue, null);
                    }
                    catch (System.EntryPointNotFoundException e) {
                        Debug.Log($"WebCamPipeline: VideoEncoder: EntryPointNotFoundException: {e}");
                        throw new System.Exception("WebCamPipeline: PCEncoder() raised EntryPointNotFound exception, skipping PC encoding");
                    }
                    //
                    // Create bin2dash writer for PC transmission
                    //
                    var Bin2Dash = cfg.PCSelfConfig.Bin2Dash;
                    if (Bin2Dash == null)
                        throw new System.Exception("WebCamPipeline: missing self-user PCSelfConfig.Bin2Dash config");
                    try {
                        Workers.B2DWriter.DashStreamDescription[] b2dStreams = new Workers.B2DWriter.DashStreamDescription[1] {
                        new Workers.B2DWriter.DashStreamDescription() {
                        tileNumber = 0,
                        quality = 0,
                        inQueue = writerQueue
                        }
                    };
                        if (useDash)
                            writer = new Workers.B2DWriter(user.sfuData.url_pcc, "webcam", "wcwc", Bin2Dash.segmentSize, Bin2Dash.segmentLife, b2dStreams);
                        else
                            writer = new Workers.SocketIOWriter(user, "webcam", b2dStreams);
                    }
                    catch (System.EntryPointNotFoundException e) {
                        Debug.Log($"WebCamPipeline: SocketIOWriter(): EntryPointNotFound({e.Message})");
                        throw new System.Exception($"WebCamPipeline: B2DWriter() raised EntryPointNotFound({e.Message}) exception, skipping PC writing");
                    }
                    /*
                                        //
                                        // Create pipeline for audio, if needed.
                                        // Note that this will create its own infrastructure (capturer, encoder, transmitter and queues) internally.
                                        //
                                        var AudioBin2Dash = cfg.PCSelfConfig.AudioBin2Dash;
                                        if (AudioBin2Dash == null)
                                            throw new System.Exception("WebCamPipeline: missing self-user PCSelfConfig.AudioBin2Dash config");
                                        try {
                                            audioSender = gameObject.AddComponent<VoiceSender>();
                                            audioSender.Init(user, "audio", AudioBin2Dash.segmentSize, AudioBin2Dash.segmentLife, Config.Instance.protocolType == Config.ProtocolType.Dash); //Audio Pipeline
                                        }
                                        catch (System.EntryPointNotFoundException e) {
                                            Debug.LogError("WebCamPipeline: VoiceDashSender.Init() raised EntryPointNotFound exception, skipping voice encoding\n" + e);
                                            throw new System.Exception("WebCamPipeline: VoiceDashSender.Init() raised EntryPointNotFound exception, skipping voice encoding\n" + e);
                                        }
                    */
                }
                else {
                    Transform screen = transform.Find("PlayerHeadScreen");
                    var renderer = screen.GetComponent<Renderer>();
                    if (renderer != null) {
                        renderer.material.mainTexture = webCamTexture;
                        renderer.transform.localScale = new Vector3(0.5f, (webCamTexture.height / (float)webCamTexture.width) * 0.5f, 1);
                    }
                }
                break;
            case "remote": // Remoto
                if (useDash)    reader = new Workers.BaseSubReader(user.sfuData.url_pcc, "webcam", 1, 0, videoCodecQueue);
                else            reader = new Workers.SocketIOReader(user, "webcam", videoCodecQueue);

                //
                // Create video decoder.
                //
                decoder = new Workers.VideoDecoder(videoCodecQueue, null, videoPreparerQueue, null);
                //
                // Create video preparer.
                //
                preparer = new Workers.VideoPreparer(videoPreparerQueue, null);
                /*
                                //
                                // Create pipeline for audio, if needed.
                                // Note that this will create its own infrastructure (capturer, encoder, transmitter and queues) internally.
                                //
                                var AudioSUBConfig = cfg.AudioSUBConfig;
                                if (AudioSUBConfig == null) throw new System.Exception("WebCamPipeline: missing other-user AudioSUBConfig config");
                                audioReceiver = gameObject.AddComponent<VoiceReceiver>();
                                audioReceiver.Init(user, "audio", AudioSUBConfig.streamNumber, AudioSUBConfig.initialDelay, Config.Instance.protocolType == Config.ProtocolType.Dash); //Audio Pipeline                
                */
                ready = true;
                break;
        }
        return this;
    }

    // Update is called once per frame
    System.DateTime lastUpdateTime;
    float timeToFrame = 0;
    private void Update() {
        if (ready) {
            lock (preparer) {
                if (preparer.availableVideo > 0) {
                    if (texture == null) {
                        texture = new Texture2D( decoder!=null?decoder.Width: width, decoder != null ? decoder.Height:height, TextureFormat.RGB24, false, true);
                        Transform screen = transform.Find("PlayerHeadScreen");
                        var renderer = screen.GetComponent<Renderer>();
                        if (renderer != null) {
                            renderer.material.mainTexture = texture;
                            renderer.transform.localScale = new Vector3(0.5f, (texture.height / (float)texture.width) * 0.5f, 1);
                        }
                    }
                    try {
                        texture.LoadRawTextureData(preparer.GetVideoPointer(preparer.videFrameSize), preparer.videFrameSize);
                        texture.Apply();
                    } catch {
                        Debug.Log("[FPA] ERROR on LoadRawTextureData.");
                    }
                }
            }
        }



        if (debugTiling)
        {
            // Debugging: print position/orientation of camera and others every 10 seconds.
            if (lastUpdateTime == null || (System.DateTime.Now > lastUpdateTime + System.TimeSpan.FromSeconds(10)))
            {
                lastUpdateTime = System.DateTime.Now;
                if (isSource)
                {
                    ViewerInformation vi = GetViewerInformation();
                    Debug.Log($"xxxjack WebCamPipeline self: pos=({vi.position.x}, {vi.position.y}, {vi.position.z}), lookat=({vi.gazeForwardDirection.x}, {vi.gazeForwardDirection.y}, {vi.gazeForwardDirection.z})");
                }
                else
                {
                    Vector3 position = GetPosition();
                    Vector3 rotation = GetRotation();
                    Debug.Log($"xxxjack WebCamPipeline other: pos=({position.x}, {position.y}, {position.z}), rotation=({rotation.x}, {rotation.y}, {rotation.z})");
                }
            }
        }
    }

    void OnDestroy() {
        ready = false;
        if (texture != null) {
            DestroyImmediate(texture);
            texture = null;
        }
        webReader?.StopAndWait();
        reader?.StopAndWait();
        encoder?.StopAndWait();
        decoder?.StopAndWait();
        writer?.StopAndWait();
        preparer?.StopAndWait();
        // xxxjack the ShowTotalRefCount call may come too early, because the VoiceDashSender and VoiceDashReceiver seem to work asynchronously...
        BaseMemoryChunkReferences.ShowTotalRefCount();
    }


    public SyncConfig GetSyncConfig()
    {
        if (!isSource)
        {
            Debug.LogError("Programmer error: WebCamPipeline: GetSyncConfig called for pipeline that is not a source");
            return new SyncConfig();
        }
        SyncConfig rv = new SyncConfig();
        Workers.B2DWriter pcWriter = (Workers.B2DWriter)writer;
        if (pcWriter != null)
        {
            rv.visuals = pcWriter.GetSyncInfo();
        }
        else
        {
            Debug.LogWarning("WebCamPipeline: GetSyncCOnfig: isSource, but writer is not a B2DWriter");
        }
        if (audioSender != null)
        {
            rv.audio = audioSender.GetSyncInfo();
        }
        // xxxjack also need to do something for VioceIOSender....
        return rv;
    }

    public void SetSyncConfig(SyncConfig config)
    {
        if (isSource)
        {
            Debug.LogError("Programmer error: WebCamPipeline: SetSyncConfig called for pipeline that is a source");
            return;
        }
        Workers.PCSubReader pcReader = (Workers.PCSubReader)reader;
        if (pcReader != null)
        {
            pcReader.SetSyncInfo(config.visuals);
        }
        else
        {
            Debug.LogWarning("WebCamPipeline: SetSyncConfig: reader is not a PCSubReader");
        }

        audioReceiver?.SetSyncInfo(config.audio);
    }

    public Vector3 GetPosition()
    {
        if (isSource)
        {
            Debug.LogError("Programmer error: WebCamPipeline: GetPosition called for pipeline that is a source");
            return new Vector3();
        }
        return transform.position;
    }

    public Vector3 GetRotation()
    {
        if (isSource)
        {
            Debug.LogError("Programmer error: WebCamPipeline: GetRotation called for pipeline that is a source");
            return new Vector3();
        }
        return transform.rotation * Vector3.forward;
    }

    public float GetBandwidthBudget()
    {
        return 999999.0f;
    }

    public ViewerInformation GetViewerInformation()
    {
        if (!isSource)
        {
            Debug.LogError("Programmer error: WebCamPipeline: GetViewerInformation called for pipeline that is not a source");
            return new ViewerInformation();
        }
        // The camera object is nested in another object on our parent object, so getting at it is difficult:
        Camera _camera = gameObject.transform.parent.GetComponentInChildren<Camera>();
        if (_camera == null)
        {
            Debug.LogError("Programmer error: WebCamPipeline: no Camera object for self user");
            return new ViewerInformation();
        }
        Vector3 position = _camera.transform.position;
        Vector3 forward = _camera.transform.rotation * Vector3.forward;
        return new ViewerInformation()
        {
            position = position,
            gazeForwardDirection = forward
        };
    }
}
