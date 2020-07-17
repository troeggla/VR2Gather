﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.XR;

public class LoginController : PilotController {

    private static LoginController instance;

    public static LoginController Instance { get { return instance; } }

    //AsyncOperation async;
    Coroutine loadCoroutine = null;

    void Awake() {
        if (!XRDevice.isPresent) {
            Resolution[] resolutions = Screen.resolutions;
            bool fullRes = false;
            foreach (var res in resolutions) {
                if (res.width == 1920 && res.height == 1080) fullRes = true;
            }
            if (fullRes) Screen.SetResolution(1920, 1080, false, 30);
            else Screen.SetResolution(1280, 720, false, 30);
            Debug.Log("Resolution: " + Screen.width + "x" + Screen.height);
        }
    }

    public override void Start() {
        base.Start();
        if (instance == null) {
            instance = this;
        }
    }

    IEnumerator RefreshAndLoad(string scenary) {
        yield return null;
        OrchestratorController.Instance.GetUsers();
        yield return new WaitForSeconds(0.5f);
        SceneManager.LoadScene(scenary);
    }

    public override void MessageActivation(string message) {
        Debug.Log(message);
        string[] msg = message.Split(new char[] { '_' });
        if (msg[0] == MessageType.START) {
            // Check Audio
            switch (msg[2]) {
                case "0": // No Audio
                    Config.Instance.audioType = Config.AudioType.None;
                    break;
                case "1": // Socket Audio
                    Config.Instance.audioType = Config.AudioType.SocketIO;
                    break;
                case "2": // Dash Audio
                    Config.Instance.audioType = Config.AudioType.Dash;
                    break;
                default:
                    break;
            } 
            // Check Pilot
            switch (msg[1]) {
                case "Pilot 0": // PILOT 0
                    // Load Pilot
                    if (loadCoroutine == null) loadCoroutine = StartCoroutine(RefreshAndLoad("Pilot0"));
                    break;
                case "Pilot 1": // PILOT 1
                    // Load Pilot
                    if (loadCoroutine == null) loadCoroutine = StartCoroutine(RefreshAndLoad("Pilot1"));
                    break;
                case "Pilot 2": // PILOT 2
                    // Check Presenter
                    switch (msg[3]) {
                        case "0": // NONE
                            Config.Instance.presenter = Config.Presenter.None;
                            break;
                        case "1": // LOCAL
                            Config.Instance.presenter = Config.Presenter.Local;
                            break;
                        case "2": // LIVE
                            Config.Instance.presenter = Config.Presenter.Live;
                            break;
                        default:
                            break;
                    }
                    // Load Pilot
                    if (loadCoroutine == null) {
                        if (OrchestratorController.Instance.UserIsMaster && Config.Instance.presenter == Config.Presenter.Live)
                            loadCoroutine = StartCoroutine(RefreshAndLoad("Pilot2_Presenter"));
                        else
                            loadCoroutine = StartCoroutine(RefreshAndLoad("Pilot2_Player"));
                    }
                    break;
				case "Pilot 3":
					if (loadCoroutine == null) loadCoroutine = StartCoroutine(RefreshAndLoad("Pilot3"));
					break;
                default:
                    break;
            }
        }
        else if (msg[0] == MessageType.READY) {
            // Do something to check if all the users are ready (future implementation)
        }
    }
}
