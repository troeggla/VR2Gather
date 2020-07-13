﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Workers;

public class VoiceDashReceiver : MonoBehaviour {
    Workers.AudioSubReader      reader;
    Workers.BaseWorker      codec;
    Workers.AudioPreparer   preparer;

    // xxxjack nothing is dropped here. Need to investigate what is the best idea.
    QueueThreadSafe decoderQueue = new QueueThreadSafe();
    QueueThreadSafe preparerQueue = new QueueThreadSafe();

    // Start is called before the first frame update
    public void Init(string _url, string _streamName, int _streamNumber, int _initialDelay) {
        VoiceReader.PrepareDSP();
        //        const int frequency = 16000;
        //        const double optimalAudioBufferDuration = 1.2;   // How long we want to buffer audio (in seconds)
        //        const int optimalAudioBufferSize = (int)(frequency * optimalAudioBufferDuration);
        AudioSource audioSource = gameObject.AddComponent<AudioSource>();
        audioSource.spatialize = true;
        audioSource.spatialBlend = 1.0f;
        audioSource.minDistance = 4f;
        audioSource.maxDistance = 100f;
        audioSource.loop = true;
        audioSource.Play();

        reader = new Workers.AudioSubReader(_url, _streamName, _streamNumber, _initialDelay, decoderQueue);
//        reader = new Workers.SocketIOReader(userID, decoderQueue);
        codec = new Workers.VoiceDecoder(decoderQueue, preparerQueue);
        preparer    = new Workers.AudioPreparer(preparerQueue);//, optimalAudioBufferSize);
    }

    void OnDestroy() {
        reader?.StopAndWait();
        codec?.StopAndWait();
        preparer?.StopAndWait();
    }

    void OnAudioRead(float[] data) {
        if (preparer == null || !preparer.GetAudioBuffer(data, data.Length))
            System.Array.Clear(data, 0, data.Length);
    }

    float[] tmpBuffer;
    void OnAudioFilterRead(float[] data, int channels) {
        if (tmpBuffer == null) tmpBuffer = new float[data.Length];
        if (preparer != null && preparer.GetAudioBuffer(tmpBuffer, tmpBuffer.Length)) {
            int cnt = 0;
            do {
                data[cnt] += tmpBuffer[cnt];
            } while (++cnt < data.Length);
        }
    }

    public void SetSyncInfo(SyncConfig.ClockCorrespondence _clockCorrespondence)
    {
        reader.SetSyncInfo(_clockCorrespondence);
    }


}
