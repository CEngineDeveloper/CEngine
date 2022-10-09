using UnityEngine;
using System.Collections;
using System.Collections.Generic;
namespace CYM
{

    public delegate void QueueSpotDelegate();

    /// <summary>
    /// Struct that represents a spot in the queue
    /// </summary>
    public struct QueueSpot
    {
        /// <summary>
        /// The gameObject that's issuing the job
        /// </summary>
        public GameObject Instigator;

        /// <summary>
        /// the job's delegate
        /// </summary>
        public QueueSpotDelegate Execute;
    }

    /// <summary>
    /// Job scheduler
    /// </summary>
    public class Scheduler : MonoBehaviour
    {
        /// <summary>
        /// the queue list
        /// </summary>
        private List<QueueSpot> Queue = new List<QueueSpot>();

        /// <summary>
        /// Number of jobs to do per frame
        /// </summary>
        private int jobsPerFrame;

        /// <summary>
        /// if true the queue will loop after all queued jobs are executed
        /// Otherwise, queued jobs will be be removed from the queue once they are executed and the queue will be destroyed once all jobs have been executed
        /// </summary>
        private bool bIsLooping;

        /// <summary>
        /// The queue will try to spread the workload over this number of frames
        /// Only taken into consideration  if > 1
        /// Example :
        /// MaxFrames = 5 means that the queue will do all the queued work in 5 frames
        /// MaxFrames = 0 means that the queue will do WorkPerFrame and disregard this parameter
        /// </summary>
        private int maxFrames;

        /// <summary>
        /// Should the scheduler destroy itself once all jobs have been executed ?
        /// </summary>
        private bool bDestroyOnEmpty;

        /// <summary>
        /// How often the queue is processed.
        /// </summary>
        private float UpdateRate;

        /// <summary>
        /// is the queue running ? Internal flag.
        /// </summary>
        private bool bIsRunning = false;
        private int CurrentIndex = 0;

        /// <summary>
        /// Desired number of jobs per frames.<br></br>
        /// This shouldn't be set directly.
        /// </summary>
        private int DesiredNbInterations = 0;


        /// <summary>
        /// Creates a queue
        /// </summary>
        /// <param name="jobsPerFrame">number of jobs to run per frame</param>
        /// <param name="bIsLooping">does the queue loop or delete jobs after execution</param>
        public void InitialiazeQueue(int jobsPerFrame, bool bIsLooping)
        {
            this.jobsPerFrame = jobsPerFrame;
            this.bIsLooping = bIsLooping;
            this.maxFrames = 0;
            this.bDestroyOnEmpty = false;
            DetermineNumberOfJobs();

            bIsRunning = true;
            StartCoroutine(ProcessQueueRoutine());
        }

        /// <summary>
        /// Creates a queue
        /// </summary>
        /// <param name="bNewIsLooping">Is the queue looped or are jobs removed from the queue once exectued ?</param>
        /// <param name="newMaxFrames">Instead of doing a fixed number of jobs per frame, make sure all jobs are done in this many frames. Ignored if > 1</param>
        /// 
        public void InitialiazeQueue(bool bIsLooping, int maxFrames)
        {
            this.jobsPerFrame = 1;
            this.bIsLooping = bIsLooping;
            this.maxFrames = maxFrames;
            this.bDestroyOnEmpty = false;
            DetermineNumberOfJobs();

            bIsRunning = true;
            StartCoroutine(ProcessQueueRoutine());
        }

        /// <summary>
        /// sets the size the size of a job batch (number of jobs per queue tick) and lifts the maxFrame constraint
        /// </summary>
        /// <param name="batchSize"></param>
        public void SetJobBatchSize(int batchSize)
        {
            this.jobsPerFrame = batchSize;
            this.maxFrames = 0;

            DetermineNumberOfJobs();
        }

        /// <summary>
        /// constraints the queue to finish all jobs in the given number of frames
        /// </summary>
        /// <param name="maxFrames"></param>
        public void SetMaxFrames(int maxFrames)
        {
            this.maxFrames = maxFrames;
            this.jobsPerFrame = 0;

            DetermineNumberOfJobs();
        }

        /// <summary>
        /// Sets the frequency at which the queue is processed
        /// </summary>
        /// <param name="UpdateRate"></param>
        public void SetUpdateRate(float UpdateRate)
        {
            if (UpdateRate > 0)
                this.UpdateRate = UpdateRate;
            else this.UpdateRate = 0.00001f;

        }

        /// <summary>
        /// Adds a job to the queue
        /// </summary>
        /// <param name="Instigator">The object responsible for the job</param>
        /// <param name="newDelgate">The function to run</param>
        /// <returns></returns>
        public bool AddJobToQueue(GameObject Instigator, QueueSpotDelegate newDelgate)
        {
            if (Instigator == null) return false;

            QueueSpot newSpot = new QueueSpot();
            newSpot.Execute = newDelgate;
            newSpot.Instigator = Instigator;
            Queue.Add(newSpot);

            //update number of jobs per frame
            DetermineNumberOfJobs();

            if (!bIsRunning)
                StartCoroutine(ProcessQueueRoutine());
            return true;
        }

        /// <summary>
        /// Removes all jobs queued by given GameObject
        /// returns true if remove successfuly. False otherwise.
        /// </summary>
        /// <param name="ObjectToRemove"></param>
        /// <returns></returns>
        public bool RemoveJobFromQueue(GameObject Instigator)
        {
            for (int i = 0; i < Queue.Count; i++)
            {
                if (Queue[i].Instigator == Instigator)
                {
                    Queue.RemoveAt(i);
                    if (i > 0)
                        i--;
                }
            }
            return false;
        }

        /// <summary>
        /// Destroys the queue.
        /// </summary>
        /// <param name="bImmediate">if false, the queue will run all its jobs and then destroy itself, otherwise the destruction is immediate</param>
        public void DestroyQueue(bool bImmediate)
        {
            if (bImmediate)
                Destroy(this);
            else bDestroyOnEmpty = true;
        }

        IEnumerator ProcessQueueRoutine()
        {
            while (true)
            {
                ProcessQueue();
                yield return new WaitForSecondsRealtime(UpdateRate);
            }
        }

        /// <summary>
        /// Determines how many jobs should be executed per frame
        /// </summary>
        void DetermineNumberOfJobs()
        {
            DesiredNbInterations = 0;
            if (maxFrames > 1)
                DesiredNbInterations = Mathf.FloorToInt(Queue.Count / maxFrames);
            else
                DesiredNbInterations = Mathf.Min(Queue.Count, jobsPerFrame);
        }

        /// <summary>
        /// Processes the queue
        /// </summary>
        void ProcessQueue()
        {
            //Debug.Log("Queue count = " + Queue.Count);
            if (Queue.Count == 0) return;

            //main loop
            if (bIsLooping)
            {
                int i = 0;
                while (i < Queue.Count && i < DesiredNbInterations)
                {
                    if (CurrentIndex >= Queue.Count)
                        CurrentIndex = 0;
                    Queue[CurrentIndex].Execute();
                    CurrentIndex++;
                    i++;

                }
            }

            else
            {
                int i = 0;
                for (; Queue.Count > 0 && i < DesiredNbInterations; i++)
                {
                    Queue[0].Execute();
                    Queue.RemoveAt(0);
                    if (Queue.Count == 0)
                    {
                        OnEmpty();
                        break;
                    }
                }
            }
        }

        /// <summary>
        /// What to do once the queue is empty
        /// </summary>
        void OnEmpty()
        {
            if (bDestroyOnEmpty)
                Destroy(this);
            else
            {
                bIsRunning = false;
                StopCoroutine(ProcessQueueRoutine());
            }
        }

    }
}