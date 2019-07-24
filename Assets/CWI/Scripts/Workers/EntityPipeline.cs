﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EntityPipeline : MonoBehaviour {
    Workers.BaseWorker  reader;
    Workers.BaseWorker  codec;
    Workers.BaseWorker  writer;
    Workers.BaseWorker  preparer;
    MonoBehaviour       render;

    // Start is called before the first frame update
    public EntityPipeline Init(Config._User cfg, Transform parent) {
        if (cfg.Render.forceMesh || SystemInfo.graphicsShaderLevel < 50)
        { // Mesh
            preparer = new Workers.MeshPreparer();
            render = gameObject.AddComponent<Workers.PointMeshRenderer>();
            ((Workers.PointMeshRenderer)render).preparer = (Workers.MeshPreparer)preparer;
        }
        else
        { // Buffer
            preparer = new Workers.BufferPreparer();
            render = gameObject.AddComponent<Workers.PointBufferRenderer>();
            ((Workers.PointBufferRenderer)render).preparer = (Workers.BufferPreparer)preparer;
        }

        int forks = 1;
        switch (cfg.sourceType) {
            case "pcself": // old "rs2"
                
                if (cfg.PCSelfConfig != null) {
                    reader = new Workers.RS2Reader(cfg.PCSelfConfig);
                    reader.AddNext(preparer).AddNext(reader); // <- local render tine.
                
                    if (cfg.PCSelfConfig.Encoder != null) {
                        codec = new Workers.PCEncoder(cfg.PCSelfConfig.Encoder);
                        writer = new Workers.B2DWriter(cfg.PCSelfConfig.Bin2Dash);
                        reader.AddNext(codec).AddNext(writer).AddNext(reader); // <- encoder and bin2dash tine.
                        forks = 2;
                    }
                    if (cfg.PCSelfConfig.AudioBin2Dash != null)
                        gameObject.AddComponent<VoiceDashSender>().Init(cfg.PCSelfConfig.AudioBin2Dash);
                }
                break;
            case "pcsub":
                reader = new Workers.SUBReader(cfg.SUBConfig);
                codec  = new Workers.PCDecoder();
                reader.AddNext(codec).AddNext(preparer).AddNext(reader);
                if (cfg.AudioSUBConfig != null)
                    gameObject.AddComponent<VoiceDashReceiver>().Init(cfg.AudioSUBConfig);
                break;
            case "net":
                reader = new Workers.NetReader(cfg.NetConfig);
                codec = new Workers.PCDecoder();
                reader.AddNext(codec).AddNext(preparer).AddNext(reader);
                break;
        }



        if(reader!=null) reader.token = new Workers.Token(forks);

        transform.parent = parent;
        transform.position = new Vector3(cfg.Render.position.x, cfg.Render.position.y, cfg.Render.position.z);
        transform.rotation = Quaternion.Euler(cfg.Render.rotation);
        transform.localScale = cfg.Render.scale;
        return this;
    }

    // Start is called before the first frame update
    public EntityPipeline Init(Config._User cfg, Transform parent, string name, string pc_url, string audio_url) {
        if (cfg.Render.forceMesh || SystemInfo.graphicsShaderLevel < 50) { // Mesh
            preparer = new Workers.MeshPreparer();
            render = gameObject.AddComponent<Workers.PointMeshRenderer>();
            ((Workers.PointMeshRenderer)render).preparer = (Workers.MeshPreparer)preparer;
        }
        else { // Buffer
            preparer = new Workers.BufferPreparer();
            render = gameObject.AddComponent<Workers.PointBufferRenderer>();
            ((Workers.PointBufferRenderer)render).preparer = (Workers.BufferPreparer)preparer;
        }

        int forks = 1;
        switch (cfg.sourceType) {
            case "pcself": // old "rs2"

                if (cfg.PCSelfConfig != null) {
                    reader = new Workers.RS2Reader(cfg.PCSelfConfig);
                    reader.AddNext(preparer).AddNext(reader); // <- local render tine.

                    if (cfg.PCSelfConfig.Encoder != null) {
                        codec = new Workers.PCEncoder(cfg.PCSelfConfig.Encoder);
                        writer = new Workers.B2DWriter(cfg.PCSelfConfig.Bin2Dash, name);
                        reader.AddNext(codec).AddNext(writer).AddNext(reader); // <- encoder and bin2dash tine.
                        forks = 2;
                    }
                    if (cfg.PCSelfConfig.AudioBin2Dash != null)
                        gameObject.AddComponent<VoiceDashSender>().Init(cfg.PCSelfConfig.AudioBin2Dash, name);
                }
                break;
            case "pcsub":
                reader = new Workers.SUBReader(cfg.SUBConfig, pc_url);
                codec = new Workers.PCDecoder();
                reader.AddNext(codec).AddNext(preparer).AddNext(reader);
                if (cfg.AudioSUBConfig != null)
                    gameObject.AddComponent<VoiceDashReceiver>().Init(cfg.AudioSUBConfig, audio_url);
                break;
            case "net":
                reader = new Workers.NetReader(cfg.NetConfig);
                codec = new Workers.PCDecoder();
                reader.AddNext(codec).AddNext(preparer).AddNext(reader);
                break;
        }



        if (reader != null) reader.token = new Workers.Token(forks);

        transform.parent = parent;
        transform.position = new Vector3(cfg.Render.position.x, cfg.Render.position.y, cfg.Render.position.z);
        transform.rotation = Quaternion.Euler(cfg.Render.rotation);
        transform.localScale = cfg.Render.scale;
        return this;
    }

    // Update is called once per frame
    void OnDestroy() {
        if (reader != null)     reader.Stop();
        if (codec != null)      codec.Stop();
        if (writer != null)     writer.Stop();
        if (preparer != null)   preparer.Stop();
    }
}
