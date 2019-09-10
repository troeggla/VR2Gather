﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TestVoiceSocketIOSender : MonoBehaviour {
    public int userID;
    Workers.BaseWorker reader;
    Workers.BaseWorker codec;
    Workers.BaseWorker writer;

    public SocketIOConnection socketIOConnection;

    // Start is called before the first frame update
    IEnumerator Start() {
        NTPTools.GetNetworkTime();
        yield return socketIOConnection.WaitConnection();
        codec = new Workers.VoiceEncoder(4);
        reader = new Workers.VoiceReader(this, ((Workers.VoiceEncoder)codec).bufferSize);
        writer = new Workers.SocketIOWriter(socketIOConnection, userID);
        reader.AddNext(codec).AddNext(writer).AddNext(reader);
        reader.token = new Workers.Token(1);
    }

    void OnDestroy() {
        reader?.Stop();
        codec?.Stop();
        writer?.Stop();
    }
}