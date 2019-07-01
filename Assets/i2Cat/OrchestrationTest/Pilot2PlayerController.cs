﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using OrchestratorWrapping;
using UnityEngine.Video;

public class Pilot2PlayerController : PilotController {

    public override void Start() {
        base.Start();
        mainPanel.SetActive(false);
        background.SetActive(false);

        test.controller = this;

        for (int i = 0; i < orchestrator.activeSession.sessionUsers.Length; i++) {
            PlayerManager player = players[i];
            if (test.isDebug) player = players[i + 1];

            if (orchestrator.activeSession.sessionUsers[i] == orchestrator.TestGetUserID()) {
                player.cam.SetActive(true);
                my_ID = player.id; // Save my ID.
                if (test.useEcho) ActivateVoiceChat(player.chat, player.id);
            }
            else
                ActivateVoiceChat(player.chat, player.id);

            foreach (User u in orchestrator.availableUsers) {
                if (u.userId == orchestrator.activeSession.sessionUsers[i]) {
                    player.tvm.GetComponent<ShowTVMs>().connectionURI = u.userData.userMQurl;
                    player.tvm.GetComponent<ShowTVMs>().exchangeName = u.userData.userMQexchangeName;
                    player.tvm.SetActive(true);
                    player.pc.GetComponent<PointCloudsMainController>().subURL = u.userData.userPCDash;
                    player.pc.SetActive(false);
                }
            }
        }

        //GetComponent<MicroRecorder>().Init(my_ID, test.useEcho);
    }

    public override void Update() {
        base.Update();
        if (todoAction != Actions.WAIT) timer += Time.deltaTime;

        if (timer >= delay) {
            switch (todoAction) {
                case Actions.VIDEO_1_START:
                    videos[0].Play();
                    break;
                case Actions.VIDEO_1_PAUSE:
                    videos[0].Pause();
                    break;
                case Actions.VIDEO_2_START:
                    videos[1].Play();
                    break;
                case Actions.VIDEO_2_PAUSE:
                    videos[1].Pause();
                    break;
                default:
                    break;
            }
            todoAction = Actions.WAIT;
            timer = 0.0f;
        }
    }

    public override void MessageActivation(string message) {
        string[] msg = message.Split(new char[] { '_' });
        if (msg[0] == MessageType.LIVESTREAM) {

        }
        else if (msg[0] == MessageType.PLAY) {
            if (msg[1] == "1") todoAction = Actions.VIDEO_1_START;
            else if (msg[1] == "2") todoAction = Actions.VIDEO_2_START;
            delay = (float)TimeStampTest.GetDelay(TimeStampTest.ToDateTime(msg[2]));
            Debug.Log(delay);
        }
        else if (msg[0] == MessageType.PAUSE) {
            if (msg[1] == "1") todoAction = Actions.VIDEO_1_PAUSE;
            else if (msg[1] == "2") todoAction = Actions.VIDEO_2_PAUSE;
            delay = (float)TimeStampTest.GetDelay(TimeStampTest.ToDateTime(msg[2]));
            Debug.Log(delay);
        }
    }
}
