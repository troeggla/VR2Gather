﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Workers {
    public class BaseWorker {
        public enum WorkerType { Init, Run, End };

        bool                    bRunning = false;
        System.Threading.Thread thread;
        public Token        token { get; set; }
        protected List<BaseWorker>  nexts =  new List<BaseWorker>();

        WorkerType type;

        public BaseWorker(WorkerType _type= WorkerType.Run) {
            type = _type;
        }

        public BaseWorker AddNext(BaseWorker _next) {
            nexts.Add(_next);
            return _next;
        }

        protected void Start() {
            bRunning = true;
            thread = new System.Threading.Thread(new System.Threading.ThreadStart(_Update));
            thread.Start();
        }

        public void Stop() {
            bRunning = false;
        }

        public virtual void OnStop() { }

        void _Update() {
            while (bRunning) {
                Update();
                System.Threading.Thread.Yield();
            }
            // Wait to stop.
            bool waitNext = false;
            do{
                for (int i = 0; i < nexts.Count; ++i)
                    if (nexts[i].type != WorkerType.Init && nexts[i].bRunning)
                        waitNext = true;
            } while (waitNext) ;
            OnStop();
        }

        protected virtual void Update(){ }

        public void Next() {
            if (type == WorkerType.Init)
                token.currentForks = token.totalForks;
            else {
                if (type == WorkerType.End) {
                    if (token.original != null) token = token.original;
                    token.currentForks--;
                    if (token.currentForks != 0) {
                        token = null;
                        return;
                    }
                }
            }
            for (int i = 0; i < nexts.Count; ++i)
                if (i > 0)
                    nexts[i].token = new Token(token);
                else
                    nexts[i].token = token;
            token = null;
        }

    }
}