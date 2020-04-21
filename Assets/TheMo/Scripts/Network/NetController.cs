﻿using LitJson;
using OrchestratorWrapping;
using System.Collections.Generic;
using UnityEngine;

public class NetController : MonoBehaviour {
    public static NetController Instance { get; private set; }
    public string url = "https://vrt-orch-sandbox.viaccess-orca.com/socket.io/";

    public string userName = "Fernando@THEMO";
    public string userPassword = "THEMO2020";

    public uint uuid { get; set; }

    public static float LocalClock;
    public static float OffsetClock;
    public static float NetClock;

    Scenario scenarioToConnect;

    BinaryConnection connection;
    void Awake() {
        Instance = this;
    }

    void Start() {
        connection = new BinaryConnection();
        connection.RegisterMessage<NetTime>();

        Connect();
    }

    public bool isConnected { get { return connection.isConnected; } }

    public void Connect() {
        OrchestratorController.Instance.OnOrchestratorRequestEvent += OnOrchestratorRequest;
        OrchestratorController.Instance.OnOrchestratorResponseEvent += OnOrchestratorResponse;
        OrchestratorController.Instance.OnUserLeaveSessionEvent += OnUserLeftSessionHandler;


        OrchestratorController.Instance.OnConnectionEvent += OnConnect;
        OrchestratorController.Instance.OnLoginEvent += OnLogin;
        OrchestratorController.Instance.OnLogoutEvent += OnLogout;
        OrchestratorController.Instance.OnGetSessionsEvent += OnGetSessionsHandler;
        OrchestratorController.Instance.OnAddSessionEvent += OnAddSessionHandler;
        OrchestratorController.Instance.OnJoinSessionEvent += OnJoinSessionHandler;
        OrchestratorController.Instance.OnGetUserInfoEvent += OnGetUserInfoHandler;
        OrchestratorController.Instance.OnGetUsersEvent += OnGetUsersHandler;

        OrchestratorController.Instance.OnGetScenariosEvent += OnGetScenariosHandler;
        OrchestratorController.Instance.SocketConnect(url);
        // connection?.Connect("");
    }

    public void OnOrchestratorRequest(string pRequest) {
    //    Debug.Log($"OnOrchestratorRequest {pRequest}");
    }

    // Display the received message in the logs
    public void OnOrchestratorResponse(string pResponse) {
    //    Debug.Log($"OnOrchestratorResponse {pResponse}");
    }


    private void OnConnect(bool pConnected) {
        OrchestratorController.Instance.Login(userName, userPassword);
    }

    private void OnLogin(bool userLoggedSucessfully) {
        OrchestratorController.Instance.GetUsers();
    }

    private void OnGetUsersHandler(User[] users) {
        if (users != null && users.Length > 0) {
            for (int i = 0; i < users.Length; ++i) {
                if (users[i].userName == userName) {
                    OrchestratorController.Instance.GetUserInfo(users[i].userId);
                }
            }
        }
    }
    string UserID;
    private void OnGetUserInfoHandler(User user) {
        if (user != null) {
            UserID = user.userId;
            OrchestratorController.Instance.SelfUser = user;
            OrchestratorController.Instance.GetScenarios();
        }
    }
    private void OnGetScenariosHandler(Scenario[] scenarios) {
        if (scenarios.Length > 0) {
            scenarioToConnect = scenarios[0];
            OrchestratorController.Instance.GetSessions();
        }
    }

    private void OnLogout(bool userLogoutSucessfully) {
    }

    private void OnGetSessionsHandler(Session[] sessions) {
        if (sessions != null && sessions.Length > 0) {
            for (int i = 0; i < sessions.Length; ++i) {
                if (sessions[i].sessionId.StartsWith("NetController_")) {
                    OrchestratorController.Instance.JoinSession(sessions[i].sessionId);
                    break;
                }
            }
        } else {
            OrchestratorController.Instance.AddSession( scenarioToConnect.scenarioId, $"NetController_{Random.Range(0,1000)}", "Session test.");
        }
    }
    Session mySession;
    private void OnAddSessionHandler(Session session) {
        mySession = session;
        // Here you should store the retrieved session.
        if (session != null) {
            SubscribeToBinaryChannel();
            SendBinaryData();
        } 
    }

    private void OnJoinSessionHandler(Session session) {
        if (session != null) {
            SubscribeToBinaryChannel();
            SendBinaryData();
        } 
    }

    string binaryDataChannelName;
    private void SubscribeToBinaryChannel() {

        binaryDataChannelName = $"BINARYTATA_{Random.Range(0, 1000)}";
        OrchestratorWrapper.instance.DeclareDataStream(binaryDataChannelName);
        OrchestratorWrapper.instance.RegisterForDataStream(UserID, binaryDataChannelName);
        OrchestratorWrapper.instance.OnDataStreamReceived += OnDataPacketReceived;
    }

    private void SendBinaryData() {
        byte[] data = new byte[] { 1, 2, 3, 4, 5 };
        OrchestratorWrapper.instance.SendData(binaryDataChannelName, data);
    }

    private void OnDataPacketReceived(UserDataStreamPacket pPacket) {
        Debug.Log($"-------> OnDataPacketReceived {pPacket.dataStreamPacket.Length}");
        if (pPacket.dataStreamUserID == UserID) {
            Debug.Log($"!!! DATA {pPacket.dataStreamPacket.Length}");
        }
    }

    private void OnUserLeftSessionHandler(string userID) {
        Debug.Log($"OnUserLeftSessionHandler {userID}");
    }

    public void Disconnect() {
        OrchestratorWrapper.instance.UnregisterFromDataStream(UserID, binaryDataChannelName);
        OrchestratorWrapper.instance.RemoveDataStream(binaryDataChannelName);
        connection?.Close();
    }
/*
    void Update() {
        LocalClock = Time.realtimeSinceStartup;
        NetClock = LocalClock + OffsetClock;
        connection?.Update();
    }
*/
    private void OnDestroy() {
        Disconnect();
    }

    public bool Send(MessageBase message) {
        if (connection.isConnected) {
            connection?.Send(message);
            return true;
        }
        return false;
    }

    public T GetMessage<T>() where T: MessageBase {
        return connection?.GetMessage<T>();
    }
}