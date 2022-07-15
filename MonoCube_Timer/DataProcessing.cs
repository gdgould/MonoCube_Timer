using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Runtime.InteropServices;
using Microsoft.Xna.Framework.Graphics;

namespace MonoCube_Timer
{
    /// <summary>
    /// Contains various utility methods, mostly sorting
    /// </summary>
    public static class DataProcessing
    {
        public static string GetRootFolder()
        {
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                return Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "MonoCubeTimer");
            }
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
            {
                return Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Personal), "Application Support", "MonoCubeTimer");
            }
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
            {
                return Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), ".monoCubeTimer");
            }
            else
            {
                throw new Exception("Unrecognized operating system");
            }
        }
        public static string GetCacheFolder()
        {
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                return Path.Combine(GetRootFolder(), "Cache");
            }
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX) || RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
            {
                return Path.Combine(GetRootFolder(), ".cache");
            }
            else
            {
                throw new Exception("Unrecognized operating system");
            }
        }

        /// <summary>
        /// Splits the given string up into lines with word wrap, so that no line exceeds the given width.
        /// </summary>
        /// <param name="text">The text to be split.</param>
        /// <param name="width">The maximum width.</param>
        /// <param name="spriteFont">The font in which the text will be displayed.</param>
        /// <returns></returns>
        public static string[] DisplayString(string text, int width, SpriteFont spriteFont)
        {
            List<string> lines = new List<string>();

            string workingText = text;

            if (spriteFont.MeasureString(workingText).X <= width)
            {
                return new string[1] { workingText };
            }

            string[] blocks = workingText.Split(' ');

            string workingLine = "";

            for (int i = 0; i < blocks.Length - 1; i++)
            {
                workingLine += blocks[i];

                if (spriteFont.MeasureString(workingLine + " " + blocks[i + 1]).X > width)
                {
                    lines.Add(workingLine);
                    workingLine = "";
                }
                else
                {
                    workingLine += " ";
                }
            }
            workingLine += blocks[blocks.Length - 1];
            lines.Add(workingLine);

            return lines.ToArray();
        }

        /// <summary>
        /// Converts the given time to a human-readable string.
        /// </summary>
        /// <param name="totalElapsed">The elapsed milliseconds to convert.</param>
        /// <returns></returns>
        public static string ConvertMillisecondsToString(long totalElapsed)
        {
            long milliseconds = totalElapsed % 1000;
            long seconds = ((totalElapsed - milliseconds) % 60000) / 1000;
            long minutes = ((totalElapsed - (seconds * 1000) - milliseconds) % 3600000) / 60000;
            long hours = ((totalElapsed - (minutes * 60000) - (seconds * 1000) - milliseconds)) / 3600000;

            string displayMilliseconds = (milliseconds / 10).ToString().PadLeft(2, '0');
            string displaySeconds = seconds.ToString().PadLeft(2, '0');
            string displayMinutes = minutes.ToString().PadLeft(2, '0');
            string displayHours = hours.ToString().PadLeft(2, '0');

            string returnTime = displaySeconds + "." + displayMilliseconds;
            if (minutes > 0)
            {
                returnTime = displayMinutes + ":" + returnTime;
            }
            if (hours > 0)
            {
                if (minutes == 0)
                {
                    returnTime = displayMinutes + ":" + returnTime;
                }
                returnTime = displayHours + ":" + returnTime;
            }

            if (returnTime.Length >= 5)
            {
                if (returnTime[0] == '0')
                {
                    returnTime = returnTime.Substring(1);
                }
            }

            return returnTime;
        }

        /// <summary>
        /// Converts the given time to a human-readable string.
        /// </summary>
        /// <param name="t">The Time to be converted.</param>
        /// <returns></returns>
        public static string ConvertTimeToString(Time t)
        {
            if (t.DNF)
            {
                return "DNF";
            }
            else
            {
                return ConvertMillisecondsToString(t.ComparisonMilliseconds);
            }
        }

        /// <summary>
        /// Converts the given time to a human-readable string, removing the tenths and hundredths of a second if the time is greater than one hour.
        /// </summary>
        /// <param name="totalElapsed">The elapsed milliseconds to convert.</param>
        /// <returns></returns>
        public static string ConvertMillisecondsToShortString(long totalElapsed)
        {
            string converted = ConvertMillisecondsToString(totalElapsed);

            if (converted.Length >= 10) // "0:00:00.00"
            {
                converted = converted.Remove(converted.IndexOf("."));
            }

            return converted;
        }

        /// <summary>
        /// Converts a typed string in human-readable format into a time in milliseconds.
        /// </summary>
        /// <param name="time">The typed string to convert.</param>
        /// <returns></returns>
        public static long ConvertStringToMilliseconds(string time)
        {
            time = ReverseString(time);

            long[] multipliers = new long[] { 10, 100, 1000, 10000, 60000, 600000, 3600000, 36000000 };
            long milliseconds = 0;
            int digit = 0;

            for (int i = 0; i < Math.Min(8, time.Length); i++)
            {
                int.TryParse(time[i].ToString(), out digit);

                milliseconds += digit * multipliers[i];
            }

            return milliseconds;
        }
        /// <summary>
        /// Converts a typed string with no punctuation into the traditional hh:mm:ss.nn format for displaying times.
        /// </summary>
        /// <param name="time">The typed string without punctuation.</param>
        /// <returns></returns>
        public static string ConvertToDisplayString(string time)
        {
            time = ReverseString(time);
            if (time.Length > 8)
            {
                time = time.Remove(8);
            }
            time = time.PadRight(3, '0');

            time = time.Insert(2, ".");

            for (int i = 5; i < time.Length; i += 3)
            {
                time = time.Insert(i, ":");
            }

            return ReverseString(time);
        }
        /// <summary>
        /// Reverses the given string.
        /// </summary>
        /// <param name="s">The string to be reversed.</param>
        /// <returns></returns>
        private static string ReverseString(string s)
        {
            StringBuilder b = new StringBuilder("");
            for (int i = s.Length - 1; i >= 0; i--)
            {
                b.Append(s[i]);
            }

            return b.ToString();
        }

        /// <summary>
        /// Merge sort for sorting file names specifically.
        /// </summary>
        /// <param name="array"></param>
        /// <returns></returns>
        public static string[] MergeSort(string[] array)
        {
            if (array.Length == 0)
            {
                return new string[0];
            }
            if (array.Length == 1)
            {
                return array;
            }
            else
            {
                string[] array1 = new string[array.Length / 2];
                string[] array2 = new string[array.Length - array.Length / 2];
                Array.Copy(array, array1, array.Length / 2);
                Array.Copy(array, array.Length / 2, array2, 0, array.Length - array.Length / 2);
                return MergeArrays(MergeSort(array1), MergeSort(array2));
            }
        }
        public static string[] MergeArrays(string[] array1, string[] array2)
        {
            string[] returnArray = new string[array1.Length + array2.Length];

            bool finished = false;
            int count1 = 0;
            int count2 = 0;
            int returnCount = 0;
            while (!finished)
            {
                if (count1 < array1.Length && count2 < array2.Length)
                {
                    if (int.Parse(GetFileName(array1[count1]).Substring(0, 8)) < int.Parse(GetFileName(array2[count2]).Substring(0, 8)))
                    {
                        returnArray[returnCount] = array1[count1++];
                    }
                    else if (int.Parse(GetFileName(array1[count1]).Substring(0, 8)) == int.Parse(GetFileName(array2[count2]).Substring(0, 8)))
                    {
                        if (int.Parse(GetFileName(array1[count1]).Substring(8)) < int.Parse(GetFileName(array2[count2]).Substring(8)))
                        {
                            returnArray[returnCount] = array1[count1++];
                        }
                        else
                        {
                            returnArray[returnCount] = array2[count2++];
                        }
                    }
                    else
                    {
                        returnArray[returnCount] = array2[count2++];
                    }
                    returnCount++;
                }
                else if (count1 >= array1.Length && count2 < array2.Length)
                {
                    returnArray[returnCount++] = array2[count2++];
                }
                else if (count1 < array1.Length && count2 >= array2.Length)
                {
                    returnArray[returnCount++] = array1[count1++];
                }
                else
                {
                    finished = true;
                }
            }
            return returnArray;
        }
        private static string GetFileName(string filePath)
        {
            //Gets the file name from the path, and checks if it is a valid 9+ digit number as well, returning 000000000 otherwise
            string fileName = filePath.Split(Path.PathSeparator)[filePath.Split(Path.PathSeparator).Length - 1].Split('.')[0];

            if (!int.TryParse(fileName, out int foo) || fileName.Length < 9)
            {
                fileName = "000000000";
            }

            return fileName;
        }

        /// <summary>
        /// Merge Sort for Times.
        /// </summary>
        /// <param name="array">The Times to be sorted.</param>
        /// <returns></returns>
        public static Time[] MergeSort(Time[] array)
        {
            if (array.Length == 0)
            {
                return new Time[0];
            }
            if (array.Length == 1)
            {
                return array;
            }
            else
            {
                Time[] array1 = new Time[array.Length / 2];
                Time[] array2 = new Time[array.Length - array.Length / 2];
                Array.Copy(array, array1, array.Length / 2);
                Array.Copy(array, array.Length / 2, array2, 0, array.Length - array.Length / 2);
                return MergeArrays(MergeSort(array1), MergeSort(array2));
            }
        }
        public static Time[] MergeArrays(Time[] array1, Time[] array2)
        {
            Time[] returnArray = new Time[array1.Length + array2.Length];

            bool finished = false;
            int count1 = 0;
            int count2 = 0;
            int returnCount = 0;
            while (!finished)
            {
                if (count1 < array1.Length && count2 < array2.Length)
                {
                    if (array1[count1] < array2[count2])
                    {
                        returnArray[returnCount] = array1[count1++];
                    }
                    else if (array1[count1] == array2[count2])
                    {
                        if (array1[count1].DateRecorded < array2[count2].DateRecorded)
                        {
                            returnArray[returnCount] = array1[count1++];
                        }
                        else
                        {
                            returnArray[returnCount] = array2[count2++];
                        }
                    }
                    else
                    {
                        returnArray[returnCount] = array2[count2++];
                    }
                    returnCount++;
                }
                else if (count1 >= array1.Length && count2 < array2.Length)
                {
                    returnArray[returnCount++] = array2[count2++];
                }
                else if (count1 < array1.Length && count2 >= array2.Length)
                {
                    returnArray[returnCount++] = array1[count1++];
                }
                else
                {
                    finished = true;
                }
            }
            return returnArray;
        }

        /// <summary>
        /// Merge Sort for Averages.
        /// </summary>
        /// <param name="array">The Averages to be sorted.</param>
        /// <returns></returns>
        public static Average[] MergeSort(Average[] array)
        {
            if (array.Length == 0)
            {
                return new Average[0];
            }
            if (array.Length == 1)
            {
                return array;
            }
            else
            {
                Average[] array1 = new Average[array.Length / 2];
                Average[] array2 = new Average[array.Length - array.Length / 2];
                Array.Copy(array, array1, array.Length / 2);
                Array.Copy(array, array.Length / 2, array2, 0, array.Length - array.Length / 2);
                return MergeArrays(MergeSort(array1), MergeSort(array2));
            }
        }
        public static Average[] MergeArrays(Average[] array1, Average[] array2)
        {
            Average[] returnArray = new Average[array1.Length + array2.Length];

            bool finished = false;
            int count1 = 0;
            int count2 = 0;
            int returnCount = 0;
            while (!finished)
            {
                if (count1 < array1.Length && count2 < array2.Length)
                {
                    if (array1[count1] < array2[count2])
                    {
                        returnArray[returnCount] = array1[count1++];
                    }
                    else if (array1[count1] == array2[count2])
                    {
                        if (array1[count1].EndDate < array2[count2].EndDate)
                        {
                            returnArray[returnCount] = array1[count1++];
                        }
                        else
                        {
                            returnArray[returnCount] = array2[count2++];
                        }
                    }
                    else
                    {
                        returnArray[returnCount] = array2[count2++];
                    }
                    returnCount++;
                }
                else if (count1 >= array1.Length && count2 < array2.Length)
                {
                    returnArray[returnCount++] = array2[count2++];
                }
                else if (count1 < array1.Length && count2 >= array2.Length)
                {
                    returnArray[returnCount++] = array1[count1++];
                }
                else
                {
                    finished = true;
                }
            }
            return returnArray;
        }
    }
}