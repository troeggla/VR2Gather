﻿using System;
using System.Collections.Generic;
using LitJson;
using System.Reflection;

namespace OrchestratorWrapping
{
    // Base class for the elements returned by the orchestrator
    public abstract class OrchestratorElement
    {
        // used to retrieve the ID
        public abstract string GetId();

        //used to display the element for Gui
        public abstract string GetGuiRepresentation();

        // Parse a JSonData to a C# object
        public static T ParseJsonData<T>(JsonData data)
        {
            return JsonMapper.ToObject<T>(data.ToJson());
        }
    }

    public class User: OrchestratorElement
    {
        public string userId="";
        public string userName="";
        public bool userAdmin = false;

        // empty constructor callled by the JsonData parser
        public User(){}

        // Parse a JSonData to a C# object
        public static User ParseJsonData(JsonData data)
        {
            return JsonMapper.ToObject<User>(data.ToJson());
        }

        public override string GetId()
        {
            return userId;
        }

        public override string GetGuiRepresentation()
        {
            return userName;
        }
    }

    //public class UserSession : OrchestratorElement
    //{
    //    public string userSessionId = "";
    //    public string userSessionLoggedAs = "";

    //    public override string GetId()
    //    {
    //        return userSessionId;
    //    }

    //    public override string GetGuiRepresentation()
    //    {
    //        return GetId();
    //    }
    //}

    public class Scenario : OrchestratorElement
    {
        public string scenarioId;
        public string scenarioName;
        public string scenarioDescription;
        public List<Room> scenarioRooms = new List<Room>();
        public JsonData scenarioGltf;

        public static Scenario ParseJsonData(JsonData data)
        {
            Scenario scenario = new Scenario();
            scenario.scenarioId = data["scenarioId"].ToString();
            scenario.scenarioName = data["scenarioName"].ToString();
            scenario.scenarioDescription = data["scenarioDescription"].ToString();
            JsonData rooms = data["scenarioRooms"];
            scenario.scenarioRooms = Helper.ParseElementsList<Room>(rooms);
            scenario.scenarioGltf = data["scenarioGltf"];
            return scenario;
        }

        public override string GetId()
        {
            return scenarioId;
        }

        public override string GetGuiRepresentation()
        {
            return scenarioName + " (" + scenarioDescription + ")";
        }
    }

    public class ScenarioInstance : OrchestratorElement
    {
        public string scenarioRefId; //store reference on the source scenario
        public string sessionId;
        public string scenarioId;
        public string scenarioName;
        public string scenarioDescription;
        public List<OrchestratorElement> scenarioRooms = new List<OrchestratorElement>();

        public override string GetId()
        {
            return scenarioId;
        }

        public override string GetGuiRepresentation()
        {
            return scenarioName + " (" + scenarioDescription + ")";
        }
    }

    public class Room : OrchestratorElement
    {
        public string roomId;
        public string roomName;
        public string roomDescription;
        public int roomCapacity;

        public static Room ParseJsonData(JsonData data)
        {
            return JsonMapper.ToObject<Room>(data.ToJson());
        }

        public override string GetId()
        {
            return roomId;
        }

        public override string GetGuiRepresentation()
        {
            return roomName + " (" + roomDescription + ")";
        }
    }

    public class RoomInstance : Room
    {
        public string roomRefId;
        public string[] roomUsers;

        public new static RoomInstance ParseJsonData(JsonData data)
        {
            return JsonMapper.ToObject<RoomInstance>(data.ToJson());
        }

        public override string GetId()
        {
            return roomId;
        }

        public override string GetGuiRepresentation()
        {
            return roomName + " (" + roomDescription + ")";
        }
    }


    public class Session : OrchestratorElement
    {
        public string sessionId;
        public string sessionName;
        public string sessionDescription;
        public string sessionAdministrator;
        public string scenarioId; // the scenario ID
        public string[] sessionUsers;

        public static Session ParseJsonData(JsonData data)
        {
            return JsonMapper.ToObject<Session>(data.ToJson());
        }

        public override string GetId()
        {
            return sessionId;
        }

        public override string GetGuiRepresentation()
        {
            return sessionName + " (" + sessionDescription + ")";
        }
    }
}