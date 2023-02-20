using UnityEngine;
using System.Collections;
using System.Collections.Generic;
namespace CYM
{
    /// <summary>
    /// Container that handles all queues.<br></br>
    /// This should be used to manupilate all available queues.
    /// </summary>
    public class QueueHub : MonoBehaviour
    {

        private static QueueHub instance;

        public static QueueHub Instance
        {
            get
            {
                if (instance == null)
                {
                    GameObject tmp = new GameObject("Queue Hub");
                    instance = tmp.AddComponent<QueueHub>();
                    return instance;
                }
                else return instance;
            }
        }

        /// <summary>
        /// The list of queues
        /// </summary>
        public Dictionary<string, Scheduler> Queues = new Dictionary<string, Scheduler>();

        /// <summary>
        /// Creates a queue that executes a number of jobs per frame
        /// </summary>
        /// <param name="queueName">name of the que</param>
        /// <param name="jobsPerFrame">number of jobs per frame</param>
        /// <param name="bIsLooping">does queue loop or remove jobs after execution ?</param>
        /// <returns>true on success, false if queue already exists</returns>
        public static bool CreateQueue(string queueName, int jobsPerFrame, bool bIsLooping)
        {
            if (Instance.Queues.ContainsKey(queueName))
            {
                Debug.Log("Queue already exists");
                return false;
            }
            else
            {
                Scheduler newQueue = Instance.gameObject.AddComponent<Scheduler>();
                newQueue.InitialiazeQueue(jobsPerFrame, bIsLooping);
                Instance.Queues.Add(queueName, newQueue);
                return true;
            }
        }

        /// <summary>
        /// Creates a queue that will execute all jobs within the given maxFrames
        /// </summary>
        /// <param name="queueName">name of the queue</param>
        /// <param name="bIsLooping">does queue loop or remove jobs after execution ?</param>
        /// <param name="maxFrames">number frames for the queue to execute all jobs</param>
        /// <returns>true on success, false if queue already exists</returns>
        public static bool CreateQueue(string queueName, bool bIsLooping, int maxFrames)
        {
            if (Instance.Queues.ContainsKey(queueName))
            {
                Debug.Log("Queue already exists");
                return false;
            }
            else
            {
                Scheduler newQueue = Instance.gameObject.AddComponent<Scheduler>();
                newQueue.InitialiazeQueue(bIsLooping, maxFrames);
                Instance.Queues.Add(queueName, newQueue);
                return true;
            }
        }

        /// <summary>
        /// sets the size the size of a job batch (number of jobs per queue tick)
        /// </summary>
        /// <param name="queueName"></param>
        /// <param name="newSize"></param>
        public static void SetJobBatchSize(string queueName, int newSize)
        {
            Scheduler OutTmpQueue = null;

            if (Instance.Queues.TryGetValue(queueName, out OutTmpQueue))
            {
                OutTmpQueue.SetJobBatchSize(newSize);
            }
        }

        /// <summary>
        /// constraints the queue to finish all jobs in the given number of frames
        /// </summary>
        /// <param name="queueName"></param>
        /// <param name="maxFrames"></param>
        public static void SetMaxFrames(string queueName, int maxFrames)
        {
            Scheduler OutTmpQueue = null;

            if (Instance.Queues.TryGetValue(queueName, out OutTmpQueue))
            {
                OutTmpQueue.SetMaxFrames(maxFrames);
            }
        }

        /// <summary>
        /// Adds job to the queue.
        /// </summary>
        /// <param name="queueName">name of the queue</param>
        /// <param name="Instigator">object responsible for the fob</param>
        /// <param name="newDelegate"></param>
        /// <returns>true on success, false if queue doesn't exist</returns>
        public static bool AddJobToQueue(string queueName, GameObject Instigator, QueueSpotDelegate newDelegate)
        {
            Scheduler OutTmpQueue = null;

            if (Instance.Queues.TryGetValue(queueName, out OutTmpQueue))
            {
                return OutTmpQueue.AddJobToQueue(Instigator, newDelegate);
            }

            else
            {
                Debug.Log("couldn't find queue");
                return false;
            }
        }

        /// <summary>
        /// Removes all jobs in the queue from the given game object
        /// </summary>
        /// <param name="queueName">name of the queue</param>
        /// <param name="Instigator"></param>
        public static void RemoveJobFromQueue(string queueName, GameObject Instigator)
        {
            Scheduler OutTmpQueue = null;

            if (Instance.Queues.TryGetValue(queueName, out OutTmpQueue))
            {
                OutTmpQueue.RemoveJobFromQueue(Instigator);
            }

            else
            {
                Debug.Log("couldn't find queue");
            }
        }

        /// <summary>
        /// Sets the frequency at which the queue is processed.
        /// </summary>
        /// <param name="queueName">The name of the queue</param>
        /// <param name="UpdateRate">will update everyframe if <= 0.</param>
        public static void SetQueueUpdateRate(string queueName, float UpdateRate)
        {
            Scheduler OutTmpQueue = null;

            if (Instance.Queues.TryGetValue(queueName, out OutTmpQueue))
            {
                OutTmpQueue.SetUpdateRate(UpdateRate);
            }

            else
            {
                Debug.Log("couldn't find queue");
            }
        }

        /// <summary>
        /// Destroys the queue
        /// </summary>
        /// <param name="queueName">the name of the queue</param>
        /// <param name="bImmediate">if false, the queue will run all its jobs and then destroy itself, otherwise the destruction is immediate</param>
        public static void DestroyQueue(string queueName, bool bImmediate)
        {
            if (Instance == null)
                return;
            Scheduler OutTmpQueue = null;

            if (Instance.Queues.TryGetValue(queueName, out OutTmpQueue))
            {
                OutTmpQueue.DestroyQueue(bImmediate);
            }
        }

        /// <summary>
        /// Checks if given queue exists
        /// </summary>
        /// <param name="queueName"></param>
        /// <returns>true if queue exists, false otherwise</returns>
        public static bool DoesQueueExist(string queueName)
        {
            if (Instance == null)
                return false;
            return Instance.Queues.ContainsKey(queueName);
        }

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