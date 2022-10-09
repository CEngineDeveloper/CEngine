using UnityEngine;
using System.Collections.Generic;
using System.Threading;

namespace FoW
{
    enum FogOfWarThreadState
    {
        Running,
        Stopping,
        Stopped
    }

    public abstract class FogOfWarThreadTask
    {
        public abstract void Run();
    }

    class FogOfWarThread
    {
        public bool isWaiting { get { return task == null; } }
        public FogOfWarThreadState state { get; private set; }
        public FogOfWarThreadPool threadPool { get; private set; }
        public FogOfWarThreadTask task { get; private set; }
        Thread _thread;

        public FogOfWarThread(FogOfWarThreadPool pool)
        {
            threadPool = pool;
            state = FogOfWarThreadState.Running;
            _thread = new Thread(ThreadRun);
            _thread.Start();
        }

        void ThreadRun()
        {
            while (state == FogOfWarThreadState.Running)
            {
                if (task != null)
                {
                    try
                    {
                        task.Run();
                    }
                    catch (System.Exception e)
                    {
                        Debug.LogException(e);
                    }
                    task = null;
                }
                else
                {
                    task = threadPool.RequestNewTask(this);
                    if (task == null)
                        Thread.Sleep(threadPool.sleepTime);
                }
            }
            state = FogOfWarThreadState.Stopped;
            _thread = null;
        }

        public void Run(FogOfWarThreadTask newtask)
        {
            if (task != null)
                Debug.LogError("FogOfWarThread is trying to start task before another ends!");
            else
                task = newtask;
        }

        public void Stop()
        {
            if (state == FogOfWarThreadState.Running)
                state = FogOfWarThreadState.Stopping;
        }
    }

    public class FogOfWarThreadPool
    {
        public int maxThreads = 2;
        public int sleepTime { get { return 1; } }
        public bool hasAllFinished { get { return _taskQueue.Count == 0 && _threads.Find(t => !t.isWaiting) == null; } }

        List<FogOfWarThread> _threads = new List<FogOfWarThread>();
        List<FogOfWarThreadTask> _taskQueue = new List<FogOfWarThreadTask>();

        void RemoveStoppedThreads()
        {
            _threads.RemoveAll(t => t.state == FogOfWarThreadState.Stopped);
        }

        // this should be called once per frame
        public void Clean()
        {
            // remove any unneeded threads
            if (_threads.Count > maxThreads)
            {
                RemoveStoppedThreads();
                for (int i = maxThreads; i < _threads.Count; ++i)
                    _threads[i].Stop();
            }
        }

        public void Run(FogOfWarThreadTask task)
        {
            // add to any waiting threads
            for (int i = maxThreads; i < _threads.Count; ++i)
            {
                if (_threads[i].state == FogOfWarThreadState.Running && _threads[i].isWaiting)
                {
                    _threads[i].Run(task);
                    return;
                }
            }

            // create thread
            if (_threads.Count < maxThreads)
            {
                FogOfWarThread newthread = new FogOfWarThread(this);
                _threads.Add(newthread);
                newthread.Run(task);
                return;
            }

            // no available threads, so just add it to the queue
            lock (_taskQueue)
                _taskQueue.Add(task);
        }

        internal FogOfWarThreadTask RequestNewTask(FogOfWarThread thread)
        {
            lock (_taskQueue)
            {
                if (_taskQueue.Count > 0)
                {
                    FogOfWarThreadTask newtask = _taskQueue[_taskQueue.Count - 1];
                    _taskQueue.RemoveAt(_taskQueue.Count - 1);
                    return newtask;
                }
            }
            return null;
        }

        public void StopAllThreads()
        {
            for (int i = maxThreads; i < _threads.Count; ++i)
                _threads[i].Stop();
        }

        public void WaitUntilFinished()
        {
            while (_taskQueue.Count > 0)
                Thread.Sleep(sleepTime);
            for (int i = 0; i < _threads.Count; ++i)
            {
                while (!_threads[i].isWaiting)
                    Thread.Sleep(sleepTime);
            }
        }
    }
}