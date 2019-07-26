﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using BestHTTP.SocketIO;

namespace Workers
{
    public class SocketIOWriter : BaseWorker
    {
        byte userID;
        SocketIOConnection  socketIOConnection;
        public SocketIOWriter(SocketIOConnection socketIOConnection, int userID) : base(WorkerType.End)
        {
            this.userID = (byte)userID;
            this.socketIOConnection = socketIOConnection;
            Start();
        }
        protected override void Update()
        {
            base.Update();
            if (token != null ) {
                byte[] tmp = token.currentByteArray;
                if (token.currentSize != tmp.Length) {
                    tmp = new byte[token.currentSize];
                    System.Array.Copy(token.currentByteArray, tmp, token.currentSize);
                }
                tmp[0] = userID;
                token.latency.GetByteArray(tmp, 1);                

                if(socketIOConnection != null)
                {
                    socketIOConnection.socket.Emit("dataChannel", (object)tmp);
                }

                if(OrchestratorGui.orchestratorWrapper != null)
                {
                    Packet lPacket = new Packet();
                    List<byte[]> lList = new List<byte[]>();
                    lList.Add(tmp);
                    lPacket.Attachments = lList;
                    OrchestratorGui.orchestratorWrapper.PushAudioPacket(lPacket);
                }

                Next();
            }
        }

        public override void OnStop()
        {
            base.OnStop();
            Debug.Log("SocketIOReader Sopped");
        }
   }
}