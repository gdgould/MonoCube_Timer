using System;
using System.Collections.Generic;
using System.Linq;

namespace MonoCube_Timer
{
    class RollingAverage
    {
        private List<Time> allTimes;
        private int totalMilliseconds;
        private int dnfs;

        public bool RemoveHighestAndLowest { get; set; }
        public int MaxLength { get; set; }

        public Microsoft.Xna.Framework.Color textColor { get; set; }

        /// <summary>
        /// A container for storing times so as to efficiently calculate averages.
        /// </summary>
        public RollingAverage()
        {
            totalMilliseconds = 0;
            dnfs = 0;
            MaxLength = -1;
            allTimes = new List<Time>();
            RemoveHighestAndLowest = false;
            textColor = Microsoft.Xna.Framework.Color.White;
        }

        /// <summary>
        /// Adds a time to the container.
        /// </summary>
        /// <param name="time"></param>
        public void AddTime(Time time)
        {
            allTimes.Add(time);
            if (time.DNF)
            {
                dnfs++;
            }
            totalMilliseconds += time.Milliseconds;

            if (MaxLength > 0 && allTimes.Count() > MaxLength)
            {
                RemoveTime();
            }
        }

        /// <summary>
        /// Removes a time from the container.
        /// </summary>
        public void RemoveTime()
        {
            totalMilliseconds -= allTimes[0].Milliseconds;
            if (allTimes[0].DNF)
            {
                dnfs--;
            }
            allTimes.RemoveAt(0);
        }

        /// <summary>
        /// Calculates the average for th econtainer, using the stored times.
        /// </summary>
        /// <returns></returns>
        public Average CalculateAverage()
        {
            if (RemoveHighestAndLowest && allTimes.Count() >= 3)
            {
                int highest = int.MinValue;
                int highestMilliseconds = int.MinValue;
                int lowest = int.MaxValue;
                int lowestMilliseconds = int.MaxValue;
                for (int i = 0; i < allTimes.Count(); i++)
                {
                    if (allTimes[i].ComparisonMilliseconds < lowest)
                    {
                        lowest = allTimes[i].ComparisonMilliseconds;
                        lowestMilliseconds = allTimes[i].Milliseconds;
                    }
                    if (allTimes[i].ComparisonMilliseconds > highest)
                    {
                        highest = allTimes[i].ComparisonMilliseconds;
                        highestMilliseconds = allTimes[i].Milliseconds;
                    }
                }

                return new Average((totalMilliseconds - highestMilliseconds - lowestMilliseconds) / (allTimes.Count() - 2), (dnfs > 1 && MaxLength < 20), allTimes[0].DateRecorded, allTimes[allTimes.Count() - 1].DateRecorded, allTimes[0].Puzzle, textColor, null);
            }
            else
            {
                return new Average(totalMilliseconds / allTimes.Count(), (dnfs > 1 && MaxLength < 20), allTimes[0].DateRecorded, allTimes[allTimes.Count() - 1].DateRecorded, allTimes[0].Puzzle, textColor, null);
            }
        }

        public int GetNumberOfTimes()
        {
            return allTimes.Count();
        }

        public List<Time> GetTimes()
        {
            return allTimes;
        }

    }
}