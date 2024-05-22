﻿using System;
using VRT.Orchestrator.Wrapping;

namespace VRT.Orchestrator.Responses { 
    public interface IOrchestratorResponseBody { }

    public class OrchestratorResponse<T> where T : IOrchestratorResponseBody
    {
        public int error { get; set; }
        public string message { get; set; }

        public T body;

        public ResponseStatus ResponseStatus {
            get {
                return new ResponseStatus(error, message);
            }
        }
    }

    public class OrchestratorVersionResponse : IOrchestratorResponseBody {
        public string orchestratorVersion;
    }
}
