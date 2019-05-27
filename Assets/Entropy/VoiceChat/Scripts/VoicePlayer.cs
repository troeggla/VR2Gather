﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VoicePlayer : MonoBehaviour {
    public VoiceReceiver    receiver { get; private set; }
    AudioSource             audioSource;
    SocketIOServer          fakeServer;
    AudioClip               audioClip;

    public void Init(int frequency) {
        audioSource = gameObject.AddComponent<AudioSource>();
        receiver = new VoiceReceiver();
        audioClip = AudioClip.Create("clip0", 512, 1, frequency, true, OnAudioRead);


        audioSource.clip = audioClip;
        audioSource.loop = true;
        audioSource.Play();

    }


    void OnAudioRead(float[] data) {
        receiver.GetBuffer(data, data.Length);
    }

}