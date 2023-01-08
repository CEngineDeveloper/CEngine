using System.Collections.Generic;

namespace CYM
{
    [Unobfus]
    public static class Performance
    {
        public sealed class PerformanceSampleData
        {
            // Private
            private List<float> samples = new List<float>();

            // Methods
            public void AddSample(float value)
            {
                // Add the value
                samples.Add(value);
            }

            public void ClearSample()
            {
                samples.Clear();
            }

            public float GetAverageUsage()
            {
                float accumulator = 0;

                foreach (float value in samples)
                    accumulator += value;

                return accumulator / samples.Count;
            }

            public float GetHighestUsage()
            {
                float highest = 0;

                foreach (float value in samples)
                    if (value > highest)
                        highest = value;

                return highest;
            }
        }

        #region prop
        // Private
        private static PerformanceSampleData timing = new PerformanceSampleData();
        private static List<PerformanceSampleData> threadUsage = new List<PerformanceSampleData>();
        // Properties
        public static IList<PerformanceSampleData> ThreadSamples => threadUsage;
        #endregion

        #region set
        public static void AddTimingSample(float value)
        {
            // Lock the list
            lock (timing)
            {
                // Add the sample
                timing.AddSample(value);
            }
        }
        public static void AddUsageSample(int id, float normalizedUsage)
        {
            // Skip clamp because thread usage is normalized anyway
            lock (threadUsage)
            {
                // Add samples for the threads
                while (id >= threadUsage.Count)
                {
                    threadUsage.Add(new PerformanceSampleData());
                }

                // Add the sample
                threadUsage[id].AddSample(normalizedUsage);
            }
        }
        public static void StepSample()
        {
            // Clear threead usage
            foreach (PerformanceSampleData sample in threadUsage)
                sample.ClearSample();

            // Clear timing
            timing.ClearSample();
        }
        #endregion

        #region get
        public static float GetAverageTimingValue()
        {
            lock (timing)
            {
                return timing.GetAverageUsage();
            }
        }
        public static float GetPeekTimingValue()
        {
            lock (timing)
            {
                return timing.GetHighestUsage();
            }
        }
        public static float GetUsageValue(int id)
        {
            lock (threadUsage)
            {
                if (id < threadUsage.Count && id >= 0)
                {
                    // Get the value
                    return threadUsage[id].GetHighestUsage();
                }
            }
            return 0;
        }
        #endregion
    }
}
