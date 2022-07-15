using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Runtime.InteropServices;
using Microsoft.Xna.Framework;


// Lots of platform-specific code in here.  Wherever different platforms need different behaviour,
//   we check with RuntimeInformation.IsOSPlatform.
// Also check DatProcessing.GetRootFolder
//
// Data is stored in:
//   Windows:   %AppData%/Roaming/MonoCubeTimer
//   OSX:       ~\Library\Application Support\MonoCubeTimer
//   Linux:     home\.monoCubeTimer

namespace MonoCube_Timer
{
    struct Puzzle
    {
        public string Name { get; }
        public WCAPuzzle ScrambleType { get; }

        public Puzzle(string name, WCAPuzzle scrambleType)
        {
            this.Name = name;
            this.ScrambleType = scrambleType;
        }

        public override bool Equals(object obj)
        {
            if (obj is Puzzle)
            {
                return this.Name == ((Puzzle)obj).Name;
            }
            return false;
        }

        /// <summary>
        /// Bad hash function
        /// </summary>
        /// <returns></returns>
        public override int GetHashCode()
        {
            // Totally worthless hash here:
            return ((int)Name[0] << 20) + ((int)Name[1] << 12) + ((int)Name[2] << 4) + (int)ScrambleType;
        }
    }
    class ImportData
    {
        private bool cacheUpToDate = false;
        public bool CacheUpToDate { get { return cacheUpToDate; } }
        string AppDataPath;
        int[] maxLengths;
        List<Puzzle> puzzles;
        DateTime sessionStart;

        public ImportData(int[] maxLengths)
        {
            this.AppDataPath = DataProcessing.GetRootFolder();

            this.maxLengths = maxLengths;
            this.sessionStart = DateTime.Now;

            if (!Directory.Exists(AppDataPath))
            {
                Directory.CreateDirectory(AppDataPath);
            }

            LoadPuzzleList(); // Must be done first so that IsCacheUpToDate can use it
            cacheUpToDate = IsCacheUpToDate();
        }

        /// <summary>
        /// Loads the list of puzzle names and scramble types from file.
        /// </summary>
        public void LoadPuzzleList()
        {
            string puzzleIndexPath = Path.Combine(AppDataPath, "puzzleindex.csv");

            if (!File.Exists(puzzleIndexPath))
            {
                File.WriteAllText(puzzleIndexPath, "");
            }

            string[] puzzleData = File.ReadAllLines(puzzleIndexPath);

            puzzles = new List<Puzzle>();

            for (int i = 2; i <= 7; i++)
            {
                puzzles.Add(new Puzzle($"{i}x{i}", (WCAPuzzle)i));
            }

            Puzzle newPuzzle = new Puzzle();
            for (int i = 0; i < puzzleData.Length; i++)
            {
                string[] lineData = puzzleData[i].Split(',');
                if (lineData.Length == 2)
                {
                    int parsedPuzzle;
                    if (int.TryParse(lineData[1], out parsedPuzzle))
                    {
                        newPuzzle = new Puzzle(lineData[0], (WCAPuzzle)parsedPuzzle);
                    }
                    else
                    {
                        Log.Fatal("Invalid Puzzle Type in puzzleindex.csv");
                        Environment.Exit(1);
                    }

                    if (!puzzles.Contains(newPuzzle))
                    {
                        puzzles.Add(newPuzzle);
                    }
                }
            }
            Log.Info("Loaded puzzle list from puzzleindex.csv");
        }
        /// <summary>
        /// Saves the list of puzzle names and scramble types to file.
        /// </summary>
        public void SavePuzzleList()
        {
            string[] saveData = new string[puzzles.Count() - 6];

            for (int i = 6; i < puzzles.Count(); i++)
            {
                saveData[i - 6] = puzzles[i].Name + "," + ((int)puzzles[i].ScrambleType).ToString();
            }

            File.WriteAllLines(Path.Combine(AppDataPath, "puzzleindex.csv"), saveData);
            Log.Info("Saved puzzle list to puzzleindex.csv");
        }

        /// <summary>
        /// Adds a new puzzle to the puzzle list.
        /// </summary>
        /// <param name="puzzle">The name of the puzzle to be added</param>
        /// <returns></returns>
        public bool AddPuzzle(string puzzle)
        {
            Puzzle newPuzzle = new Puzzle(puzzle, WCAPuzzle.Null); // Change to add actual scramble type later
            if (!puzzles.Contains(newPuzzle))
            {
                puzzles.Add(newPuzzle);
                ConfirmFolders();
                SavePuzzleList();
                return true;
            }

            return false;
        }

        /// <summary>
        /// Gets the specified puzzle's scramble type.
        /// </summary>
        /// <param name="n">The integer corresponding to the specified puzzle.</param>
        /// <returns></returns>
        public WCAPuzzle GetPuzzleType(int n)
        {
            if (n >= 0 && n < puzzles.Count())
            {
                return puzzles[n].ScrambleType;
            }

            return WCAPuzzle.Null;
        }

        /// <summary>
        /// Returns the puzzle list.
        /// </summary>
        /// <returns></returns>
        public List<Puzzle> ListPuzzles()
        {
            return puzzles;
        }
        /// <summary>
        /// Gets a puzzle's name, given its number.
        /// </summary>
        /// <param name="n">The puzzle number.</param>
        /// <returns></returns>
        public string GetPuzzleName(int n)
        {
            if (n >= 0 && n < puzzles.Count())
            {
                return puzzles[n].Name;
            }

            Log.Warn($"Attempted to get puzzle name of invalid puzzle number {n}.");
            return "null";
        }
        /// <summary>
        /// Gets a puzzle's number given its name.
        /// </summary>
        /// <param name="puzzleName">The puzzle's name.</param>
        /// <returns></returns>
        public int GetPuzzleNumber(string puzzleName)
        {
            for (int i = 0; i < puzzles.Count(); i++)
            {
                if (puzzles[i].Name == puzzleName)
                {
                    return i;
                }
            }

            Log.Warn($"Attempted to get puzzle number of invalid puzzle name \"{puzzleName}\"");
            return -1;
        }
        /// <summary>
        /// Returns the list of puzzle names.
        /// </summary>
        /// <returns></returns>
        public string[] ListPuzzleNames()
        {
            string[] puzzleNames = new string[puzzles.Count()];
            for (int i = 0; i < puzzles.Count(); i++)
            {
                puzzleNames[i] = puzzles[i].Name;
            }

            return puzzleNames;
        }


        /// <summary>
        /// Gets the session ID (a unique identifier which includes the date), for a given puzzle.
        /// </summary>
        /// <param name="puzzle">The puzzle who's session ID is desired.</param>
        /// <returns></returns>
        public string GetSessionId(int puzzle)
        {
            string fullPath = GetCurrentFolderPath(puzzle);
            string[] sessions = Directory.GetFiles(fullPath);

            int todaysId = 0;
            for (int i = 0; i < sessions.Length; i++)
            {
                sessions[i] = sessions[i].Split(Path.DirectorySeparatorChar)[sessions[i].Split(Path.DirectorySeparatorChar).Length - 1].Split('.')[0];
                if (sessions[i].Length >= 8 && sessions[i].Substring(0, 8) == GetTodaysFileName())
                {
                    int result = -1;
                    if (int.TryParse(sessions[i].Substring(8), out result))
                    {
                        if (result > todaysId)
                        {
                            todaysId = result;
                        }
                    }
                }
            }
            return GetTodaysFileName() + (todaysId + 1).ToString();
        }
        /// <summary>
        /// Gets a numeric version of the current date.
        /// </summary>
        /// <returns></returns>
        public string GetTodaysFileName()
        {
            return $"{sessionStart.Year.ToString("0000")}{sessionStart.Month.ToString("00")}{sessionStart.Day.ToString("00")}";
        }

        /// <summary>
        /// Pulls all the times for the specified puzzle from file and sorts them.
        /// </summary>
        /// <param name="sessionId">The current session ID.</param>
        /// <param name="numberOfTimes">The maximum number of times to be loaded.</param>
        /// <param name="puzzle">The specified puzzle.</param>
        /// <returns></returns>
        public List<Time> GetSortedTimes(string sessionId, int numberOfTimes, int puzzle)
        {
            List<Time> returnList = new List<Time>();

            if (cacheUpToDate)
            {
                string singleFilePath = Path.Combine(DataProcessing.GetCacheFolder(), GetPuzzleName(puzzle), "single.csv");
                if (File.Exists(singleFilePath))
                {
                    return ParseFile(singleFilePath, sessionId);
                }
            }

            numberOfTimes++; //To account for the zeroed time inserted at the start of the list.

            returnList.Add(new Time());
            List<Time> fileList = new List<Time>();

            string[] subFiles = GetAllFilesAndSubfiles(Path.Combine(AppDataPath, "Puzzles", GetPuzzleName(puzzle)));
            if (subFiles.Length > 0)
            {
                subFiles = DataProcessing.MergeSort(subFiles);
            }

            for (int i = 0; i < subFiles.Length; i++)
            {
                fileList = ParseFile(subFiles[i], sessionId);

                for (int j = 0; j < fileList.Count(); j++)
                {
                    Insert(ref returnList, fileList[j]);
                    if (returnList.Count() > numberOfTimes)
                    {
                        returnList.RemoveAt(numberOfTimes);
                    }
                }
            }

            returnList.RemoveAt(0);

            return returnList;
        }

        /// <summary>
        /// Inserts a Time into a sorted list of Times.
        /// </summary>
        /// <param name="list">A sorted list of Times.</param>
        /// <param name="time">The time to be inserted.</param>
        public void Insert(ref List<Time> list, Time time)
        {
            int count = list.Count();
            while (time.ComparisonMilliseconds < list[count - 1].ComparisonMilliseconds)
            {
                count--;
            }

            if (count == 1)
            {
                time.BackColor = Constants.GetColor("TimeBoxPBColor");
            }
            list.Insert(count, time);
        }
        /// <summary>
        /// Inserts an Average into a sorted list of Averages.
        /// </summary>
        /// <param name="list">A sorted list of Averages.</param>
        /// <param name="average">The Average to be inserted.</param>
        public void Insert(ref List<Average> list, Average average)
        {
            Insert(ref list, average, true);
        }
        /// <summary>
        /// Inserts an Average into a sorted list of Averages.
        /// </summary>
        /// <param name="list">A sorted list of Averages.</param>
        /// <param name="average">The Average to be inserted.</param>
        /// <param name="betterThanPreviousPB">A boolean indicating whether this time is better than the previous PB, and thus needs a special back color.</param>
        public void Insert(ref List<Average> list, Average average, bool betterThanPreviousPB)
        {
            int count = list.Count();
            while (average.ComparisonMilliseconds < list[count - 1].ComparisonMilliseconds)
            {
                count--;
            }

            if (count == 1 && betterThanPreviousPB)
            {
                average.BackColor = Constants.GetColor("TimeBoxPBColor");
            }

            list.Insert(count, average);
        }
        /// <summary>
        /// Pulls all the times for the specified puzzle from file, computes averages, and sorts them.
        /// </summary>
        /// <param name="sessionId">The current session ID.</param>
        /// <param name="numberOfAverages">The maximum number of averages to be loaded.</param>
        /// <param name="puzzle">The specified puzzle.</param>
        /// <param name="lastThousandTimes">The 1000 most recent times for this puzzle.</param>
        /// <returns></returns>
        public List<Average>[] GetSortedAverages(string sessionId, int numberOfAverages, int puzzle, out List<Time> lastThousandTimes)
        {
            List<Average>[] returnList = new List<Average>[7];

            if (cacheUpToDate)
            {
                string filePath = Path.Combine(DataProcessing.GetCacheFolder(), GetPuzzleName(puzzle));

                for (int i = 0; i < 7; i++)
                {
                    if (File.Exists(Path.Combine(filePath, $"average{i}.csv")))
                    {
                        returnList[i] = ParseAverages(Path.Combine(filePath, $"average{i}.csv"));
                    }
                    else
                    {
                        // One of the lists is missing -- parse everything instead
                        goto parseAll;
                    }
                }

                if (File.Exists(Path.Combine(filePath, "thousand.csv")))
                {
                    lastThousandTimes = ParseFile(Path.Combine(filePath, "thousand.csv"), sessionId);
                    return returnList;
                }
            }
        parseAll:


            numberOfAverages++; //To account for the zeroed time inserted at the start of the list.

            for (int i = 0; i < 7; i++)
            {
                returnList[i] = new List<Average>();
                returnList[i].Add(new Average());
            }

            List<Time> fileList = new List<Time>();

            string[] subFiles = GetAllFilesAndSubfiles(Path.Combine(AppDataPath, "Puzzles", GetPuzzleName(puzzle)));
            if (subFiles.Length > 0)
            {
                subFiles = DataProcessing.MergeSort(subFiles);
            }

            RollingAverage[] averages = new RollingAverage[7];
            SetupAverages(ref averages, Constants.GetColor("TimeTextDefaultColor"));


            int totalTimesRecorded = 0;
            for (int i = 0; i < subFiles.Length; i++)
            {
                fileList = ParseFile(subFiles[i], sessionId);

                for (int j = 0; j < fileList.Count(); j++)
                {
                    totalTimesRecorded++;
                    for (int k = 0; k < 7; k++)
                    {
                        averages[k].AddTime(fileList[j]);

                        if (totalTimesRecorded >= maxLengths[k])
                        {
                            Insert(ref returnList[k], averages[k].CalculateAverage());
                            if (returnList[k].Count() > numberOfAverages)
                            {
                                returnList[k].RemoveAt(numberOfAverages);
                            }
                        }
                    }
                }
            }
            lastThousandTimes = averages[6].GetTimes();

            for (int i = 0; i < 7; i++)
            {
                returnList[i].RemoveAt(0);
            }
            return returnList;
        }

        /// <summary>
        /// Sets up 7 Rolling Averages to efficiently compute averages.
        /// </summary>
        /// <param name="a"></param>
        /// <param name="textColor"></param>
        private void SetupAverages(ref RollingAverage[] a, Color textColor)
        {
            a = new RollingAverage[7];
            for (int i = 0; i < 7; i++)
            {
                a[i] = new RollingAverage();
                a[i].MaxLength = maxLengths[i];
                a[i].textColor = textColor;
            }
            a[1].RemoveHighestAndLowest = true; //3 of 5
            a[2].RemoveHighestAndLowest = true; //10 of 12
        }

        /// <summary>
        /// Updates all averages for this puzzle.
        /// </summary>
        /// <param name="lastThousand">The most recent 1000 times for each puzzle.</param>
        /// <param name="sessionTimes">The times recorded in the current session.</param>
        /// <param name="previousAverages">The previously recorded averages for this puzzle.</param>
        /// <returns></returns>
        public List<Average>[] UpdateSessionAverages(List<Time> lastThousand, List<Time> sessionTimes, List<Average>[] previousAverages)
        {
            List<Average>[] returnList = new List<Average>[7];
            for (int i = 0; i < 7; i++)
            {
                returnList[i] = new List<Average>();
                returnList[i].Add(new Average());
            }

            RollingAverage[] averages = new RollingAverage[7];
            SetupAverages(ref averages, Constants.GetColor("TimeTextHybridColor"));

            int totalTimesRecorded = 0;
            for (int i = 0; i < lastThousand.Count(); i++)
            {
                totalTimesRecorded++;
                for (int j = 0; j < 7; j++)
                {
                    averages[j].AddTime(lastThousand[i]);
                }
            }

            Average calculatedAverage;
            int[] bestPreviousMilliseconds = new int[7];
            for (int i = 0; i < 7; i++)
            {
                bestPreviousMilliseconds[i] = GetBest(previousAverages[i]).ComparisonMilliseconds;
            }

            for (int i = 0; i < sessionTimes.Count(); i++)
            {
                totalTimesRecorded++;
                for (int k = 0; k < 7; k++)
                {
                    averages[k].AddTime(sessionTimes[i]);

                    if (totalTimesRecorded >= maxLengths[k])
                    {
                        calculatedAverage = averages[k].CalculateAverage();
                        Insert(ref returnList[k], calculatedAverage, calculatedAverage.ComparisonMilliseconds < bestPreviousMilliseconds[k]);
                    }
                    if (totalTimesRecorded - lastThousand.Count() >= maxLengths[k] - 1)
                    {
                        averages[k].textColor = Constants.GetColor("TimeTextCurrentColor");
                    }
                }
            }

            for (int i = 0; i < 7; i++)
            {
                returnList[i].RemoveAt(0);
            }
            return returnList;
        }
        /// <summary>
        /// Updates averages for this puzzle composed only of times recorded in this session.
        /// </summary>
        /// <param name="sessionTimes">The times recorded in the current session.</param>
        /// <param name="previousAverages">The previously recorded averages for this puzzle.</param>
        /// <returns></returns>
        public List<Average>[] UpdateSessionExclusiveAverages(List<Time> sessionTimes, List<Average>[] previousAverages)
        {
            return UpdateSessionAverages(new List<Time>(), sessionTimes, previousAverages);
        }

        /// <summary>
        /// Parses a Time storage file into a list of Times.
        /// </summary>
        /// <param name="filePath">The file path of the file to be parsed.</param>
        /// <param name="sessionId">The current session ID.</param>
        /// <returns></returns>
        private List<Time> ParseFile(string filePath, string sessionId)
        {
            List<Time> returnList = new List<Time>();

            string[] contents = File.ReadAllLines(filePath);

            /*
            string time;
            bool plus2;
            bool dnf;
            string dateRecorded;
            string puzzle;
            string scramble;
            string "comments";
            [optional] string filePath;
            [optional] bool PB;
            */
            string[] split;

            for (int i = 0; i < contents.Length; i++)
            {
                if (contents[i] != "")
                {
                    split = contents[i].Split(',');

                    if (split.Length >= 7)
                    {
                        int arg0;
                        bool arg1, arg2;
                        DateTime arg3;
                        string arg7 = split.Length >= 8 ? split[7] : filePath;
                        Color textColor = Constants.GetColor("TimeTextDefaultColor");
                        Color backColor = Constants.GetColor("TimeBoxDefaultColor");
                        bool arg6;
                        if (split.Length >= 9 && bool.TryParse(split[8], out arg6) && arg6)
                        {
                            backColor = Constants.GetColor("TimeBoxPBColor");
                        }

                        if (int.TryParse(split[0], out arg0) && bool.TryParse(split[1], out arg1) && bool.TryParse(split[2], out arg2) && DateTime.TryParse(split[3], out arg3))
                        {
                            returnList.Add(new Time(arg0, arg1, arg2, arg3, split[4], textColor, backColor, split[5], split[6], arg7));
                        }
                    }
                }
            }

            return returnList;
        }
        /// <summary>
        /// Parses an Average storage file into a list of Averages.
        /// </summary>
        /// <param name="filePath">The file path of the file to be parsed.</param>
        /// <returns></returns>
        private List<Average> ParseAverages(string filePath)
        {
            List<Average> returnList = new List<Average>();

            string[] contents = File.ReadAllLines(filePath);

            /*
            string time;
            bool dnf;
            string startDate;
            string endDate;
            string puzzle;
            string "comments";
            [optional] bool PB;
            */
            string[] split;

            for (int i = 0; i < contents.Length; i++)
            {
                if (contents[i] != "")
                {
                    split = contents[i].Split(',');

                    if (split.Length >= 6)
                    {
                        int arg0;
                        bool arg1;
                        DateTime arg2, arg3;
                        Color textColor = Constants.GetColor("TimeTextDefaultColor");
                        Color backColor = Constants.GetColor("TimeBoxDefaultColor");
                        bool arg6;
                        if (split.Length >= 7 && bool.TryParse(split[6], out arg6) && arg6)
                        {
                            backColor = Constants.GetColor("TimeBoxPBColor");
                        }

                        if (int.TryParse(split[0], out arg0) && bool.TryParse(split[1], out arg1) && DateTime.TryParse(split[2], out arg2) && DateTime.TryParse(split[3], out arg3))
                        {
                            returnList.Add(new Average(arg0, arg1, arg2, arg3, split[4], textColor, backColor, split[5]));
                        }
                    }
                }
            }

            return returnList;
        }


        /// <summary>
        /// Recursively fetches all files and subfiles inside the specified directory.
        /// </summary>
        /// <param name="dirPath">The directory to search.</param>
        /// <returns></returns>
        private string[] GetAllFilesAndSubfiles(string dirPath)
        {
            List<string> returnList = new List<string>();

            string[] subDirs = Directory.GetDirectories(dirPath);
            for (int i = 0; i < subDirs.Length; i++)
            {
                returnList.AddRange(GetAllFilesAndSubfiles(subDirs[i]));
            }
            returnList.AddRange(Directory.GetFiles(dirPath).ToList());

            for (int i = 0; i < returnList.Count(); i++)
            {
                // Checks that the file is the correct number of folders in, and that its name is fully numeric
                string[] temp = returnList[i].Split(Path.DirectorySeparatorChar);
                // The 6 represents 6 layers in: MonoCubeTimer\Puzzles\Name\Year\Month\File.csv
                if (temp.Length < 6 || temp[temp.Length - 6] != "MonoCubeTimer")
                {
                    returnList.RemoveAt(i--);
                }
                else if (!int.TryParse(temp[temp.Length - 1].Split('.')[0], out int foo))
                {
                    returnList.RemoveAt(i--);
                }
            }

            return returnList.ToArray();
        }

        /// <summary>
        /// Updates the Time file corresponding to the current session.
        /// </summary>
        /// <param name="times">The list of times to include in the file.</param>
        /// <param name="sessionId">The current session ID.</param>
        /// <param name="puzzle">The current puzzle.</param>
        public void UpdateCurrentSessionFile(List<Time> times, string sessionId, int puzzle)
        {
            string fullPath = Path.Combine(GetCurrentFolderPath(puzzle), $"{sessionId}.csv");

            if (times.Count() == 0)
            {
                File.Delete(fullPath);
                return;
            }

            string[] allLines = new string[times.Count()];

            for (int i = 0; i < times.Count(); i++)
            {
                allLines[i] = times[i].ToString();
            }

            File.WriteAllLines(fullPath, allLines);
        }
        /// <summary>
        /// Gets the folder path corresponding to the current puzzle, year, and month.
        /// </summary>
        /// <param name="puzzle">The puzzle to find a folder path for.</param>
        /// <returns></returns>
        public string GetCurrentFolderPath(int puzzle)
        {
            return Path.Combine(new string[] { AppDataPath, "Puzzles", GetPuzzleName(puzzle), sessionStart.Year.ToString(), sessionStart.Date.ToString("MMMMMMMMMM") });
        }

        /// <summary>
        /// Recreates all current folders and cache folders for all puzzles.
        /// </summary>
        public void ConfirmFolders()
        {
            Directory.CreateDirectory(DataProcessing.GetCacheFolder());

            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                DirectoryInfo dirInf = new DirectoryInfo(DataProcessing.GetCacheFolder());
                dirInf.Attributes |= FileAttributes.Hidden;
            }

            Directory.CreateDirectory(Path.Combine(DataProcessing.GetCacheFolder(), "Logs"));

            for (int i = 0; i < puzzles.Count(); i++)
            {
                Directory.CreateDirectory(GetCurrentFolderPath(i));
                Directory.CreateDirectory(Path.Combine(DataProcessing.GetCacheFolder(), GetPuzzleName(i)));
            }
        }


        /// <summary>
        /// Deletes a time stored in an old session file.
        /// </summary>
        /// <param name="time">The time to be deleted.</param>
        /// <returns></returns>
        public bool DeleteOldTime(Time time)
        {
            string stored = time.ToString();

            if (time.Filepath == "" || !File.Exists(time.Filepath))
            {
                return false;
            }

            string[] file = File.ReadAllLines(time.Filepath);

            string[] file2 = new string[file.Length - 1];
            int count = 0;
            for (int i = 0; i < file.Length; i++)
            {
                if (!(stored == file[i]))
                {
                    if (count == file2.Length)
                    {
                        // The time we are trying to delete doesn't exist in the file
                        Log.Warn($"The time: \"{time.ToString()}\" was not found in the file supposed to contain it.");
                        return false;
                    }
                    file2[count++] = file[i];
                }
            }
            SaveRecentlyDeletedTimes(stored);
            if (file2.Length == 0)
            {
                // Deletes the session file if it is now empty.
                File.Delete(time.Filepath);
            }
            else
            {
                // Otherwise re-writes it.
                File.WriteAllLines(time.Filepath, file2);
            }
            return true;
        }
        /// <summary>
        /// Saves a deleted time to the dedicated "recycle bin" file.
        /// </summary>
        /// <param name="saveTime">The time to be saved.</param>
        private void SaveRecentlyDeletedTimes(string saveTime)
        {
            string[] data = new string[0];
            string deletedPath = Path.Combine(AppDataPath, "recentlydeleted.csv");

            if (File.Exists(deletedPath))
            {
                data = File.ReadAllLines(deletedPath);
            }

            string[] data2 = new string[Math.Min(100, data.Length + 1)];
            data2[0] = saveTime;
            for (int i = 0; i < data2.Length - 1; i++)
            {
                data2[i + 1] = data[i];
            }

            File.WriteAllLines(deletedPath, data2);
        }
        /// <summary>
        /// Updates a time stored in an old session file.
        /// </summary>
        /// <param name="newTime">The time to be updated, with changes.</param>
        /// <param name="oldTime">The time to be updated, without changes.</param>
        /// <returns></returns>
        public bool ChangeOldTime(Time newTime, Time oldTime)
        {
            string stored = oldTime.ToString();

            if (oldTime.Filepath == "" || !File.Exists(oldTime.Filepath))
            {
                return false;
            }

            string[] file = File.ReadAllLines(oldTime.Filepath);

            for (int i = 0; i < file.Length; i++)
            {
                if (stored == file[i])
                {
                    file[i] = newTime.ToString();
                }
            }

            File.WriteAllLines(oldTime.Filepath, file);
            return true;
        }


        /// <summary>
        /// Gets the best Time in a list.
        /// </summary>
        /// <param name="times">The list of Times.</param>
        /// <returns></returns>
        public Time GetBest(List<Time> times)
        {
            return GetBest(times, DateTime.MaxValue);
        }
        /// <summary>
        /// Gets the best Time in a list, before a specified date.
        /// </summary>
        /// <param name="times">The list of Times.</param>
        /// <param name="latestPossibleDate">The cutoff date.</param>
        /// <returns></returns>
        public Time GetBest(List<Time> times, DateTime latestPossibleDate)
        {
            if (times.Count() == 0)
            {
                return new Time(int.MaxValue, false, true, "", Color.White, Color.White);
            }

            int milliseconds = int.MaxValue;
            int index = -1;
            for (int i = 0; i < times.Count(); i++)
            {
                if (times[i].ComparisonMilliseconds <= milliseconds && times[i].DateRecorded <= latestPossibleDate)
                {
                    milliseconds = times[i].ComparisonMilliseconds;
                    index = i;
                }
            }

            return times[index];
        }
        /// <summary>
        /// Gets the best Average in a list.
        /// </summary>
        /// <param name="times">The list of Averages.</param>
        /// <returns></returns>
        public Average GetBest(List<Average> times)
        {
            if (times.Count() == 0)
            {
                return new Average(int.MaxValue, true, DateTime.Now, DateTime.Now, "", Color.White, Color.White);
            }

            int milliseconds = int.MaxValue;
            int index = -1;
            for (int i = 0; i < times.Count(); i++)
            {
                if (times[i].ComparisonMilliseconds <= milliseconds)
                {
                    milliseconds = times[i].ComparisonMilliseconds;
                    index = i;
                }
            }

            return times[index];
        }



        /* Cache Storage/Retrieval
         * On startup, we check the date/time that the AppData/Roaming folder was last updated.
         * If it matches the one on file (MonoCubeTimer/.cache/date), we load the cache from its corresponding
         * folders in MonoCubeTimer/.cache.
         * Otherwise, we run the normal loading process.
         * 
         * Sorted times are stored in:          single.csv
         * Sorted averages are stored in:       average[i].csv
         * Last thousand times are stored in:   thousand.csv
         */

        
        /// <summary>
        /// Checks if the cache is up-to-date (that is, nothing has been modified since the program was closed.
        /// </summary>
        /// <returns></returns>
        private bool IsCacheUpToDate()
        {
            if (!CheckCacheFiles())
            {
                return false;
            }

            string datePath = Path.Combine(DataProcessing.GetCacheFolder(), "date");

            long ticks = 0;
            if (File.Exists(datePath))
            {
                long.TryParse(File.ReadAllText(datePath), out ticks);
            }

            DateTime cacheStoreValue = new DateTime(ticks);
            DateTime lastCacheWrite = File.GetLastWriteTime(datePath);
            DateTime lastUserWrite = GetLatestWriteTimeRecursive(DataProcessing.GetCacheFolder());
            DateTime lastUserWrite2 = GetLatestWriteTimeRecursive(Path.Combine(AppDataPath, "Puzzles"));
            if (lastUserWrite2 > lastUserWrite)
            {
                lastUserWrite = lastUserWrite2;
            }

            return (cacheStoreValue == lastCacheWrite) && (lastUserWrite <= lastCacheWrite);
        }
        /// <summary>
        /// Checks to make sure all normal cache files are present.
        /// </summary>
        /// <returns></returns>
        private bool CheckCacheFiles()
        {
            string cachePath = DataProcessing.GetCacheFolder();

            for (int i = 0; i < puzzles.Count(); i++)
            {
                if (!File.Exists(Path.Combine(cachePath, GetPuzzleName(i), "single.csv")) ||
                    !File.Exists(Path.Combine(cachePath, GetPuzzleName(i), "thousand.csv")))
                {
                    return false;
                }

                for (int j = 0; j < 7; j++)
                {
                    if (!File.Exists(Path.Combine(cachePath, GetPuzzleName(i), $"average{j}.csv")))
                    {
                        return false;
                    }
                }
            }

            return true;
        }

        /// <summary>
        /// Recurses throug the given directory and all its children, and returns the most recent write time to any file or directory
        /// </summary>
        /// <param name="filePath">The directory to recursively check</param>
        /// <returns></returns>
        private DateTime GetLatestWriteTimeRecursive(string filePath)
        {
            string[] files = Directory.GetFiles(filePath, "*.csv");
            string[] dirs = Directory.GetDirectories(filePath);

            DateTime latestDate = Directory.GetLastWriteTime(filePath);

            for (int i = 0; i < dirs.Length; i++)
            {
                DateTime lastWrite = GetLatestWriteTimeRecursive(dirs[i]);
                if (lastWrite > latestDate)
                {
                    latestDate = lastWrite;
                }
            }
            for (int i = 0; i < files.Length; i++)
            {
                DateTime lastWrite = File.GetLastWriteTime(files[i]);
                if (lastWrite > latestDate)
                {
                    latestDate = lastWrite;
                }
            }

            return latestDate;
        }

        /// <summary>
        /// Saves all sorted data for the specified puzzle to the cache.
        /// </summary>
        /// <param name="numberOfTimes">The maximum number of times to store.</param>
        /// <param name="numberOfAverages">The maximum number of averages to store.</param>
        /// <param name="puzzle">The puzzle to save.</param>
        /// <param name="allTimes">The list of times from previous sessions.</param>
        /// <param name="currentTimes">The list of times from this session.</param>
        /// <param name="allAverages">The list of averages from previous sessions.</param>
        /// <param name="currentAverages">The list of averages exclusively from this session.</param>
        /// <param name="lastThousand">The 1000 most recent times for this puzzle.</param>
        public void SaveCache(int numberOfTimes, int numberOfAverages, int puzzle, List<Time> allTimes, List<Time> currentTimes, List<Average>[] allAverages, List<Average>[] currentAverages, List<Time> lastThousand)
        {
            string datePath = Path.Combine(DataProcessing.GetCacheFolder(), "date");
            ConfirmFolders();

            SaveCacheTimes(numberOfTimes, puzzle, allTimes, currentTimes);
            SaveCacheAverages(numberOfAverages, puzzle, allAverages, currentAverages);
            SaveCacheThousand(puzzle, lastThousand, currentTimes);

            DateTime now = DateTime.Now;

            File.WriteAllText(datePath, now.Ticks.ToString());
            File.SetLastWriteTime(datePath, now);
        }
        /// <summary>
        /// Saves the times for the specified puzzle to the cache.
        /// </summary>
        /// <param name="numberOfTimes">The maximum number of times to store.</param>
        /// <param name="puzzle">The puzzle to save.</param>
        /// <param name="allTimes">The list of times from previous sessions.</param>
        /// <param name="currentTimes">The list of times from this session.</param>
        private void SaveCacheTimes(int numberOfTimes, int puzzle, List<Time> allTimes, List<Time> currentTimes)
        {
            List<Time> combinedTimes = DataProcessing.MergeArrays(allTimes.ToArray(), DataProcessing.MergeSort(currentTimes.ToArray())).ToList();
            if (combinedTimes.Count() > numberOfTimes)
            {
                combinedTimes.RemoveRange(numberOfTimes, combinedTimes.Count() - numberOfTimes);
            }

            string[] saveTimes = new string[combinedTimes.Count()];
            for (int i = 0; i < saveTimes.Length; i++)
            {
                saveTimes[i] = combinedTimes[i].ToString() + $",{combinedTimes[i].Filepath},{(combinedTimes[i].BackColor == Constants.GetColor("TimeBoxPBColor")).ToString().ToLower()}";
            }

            File.WriteAllLines(Path.Combine(DataProcessing.GetCacheFolder(), GetPuzzleName(puzzle), "single.csv"), saveTimes);
        }
        /// <summary>
        /// Saves the averages for the specified puzzle to the cache.
        /// </summary>
        /// <param name="numberOfAverages">The maximum number of averages to store.</param>
        /// <param name="puzzle">The puzzle to save.</param>
        /// <param name="allAverages">The list of averages from previous sessions.</param>
        /// <param name="currentAverages">The list of averages exclusively from this session.</param>
        private void SaveCacheAverages(int numberOfAverages, int puzzle, List<Average>[] allAverages, List<Average>[] currentAverages)
        {
            List<Average>[] combinedAverages = new List<Average>[7];
            for (int i = 0; i < 7; i++)
            {
                combinedAverages[i] = DataProcessing.MergeArrays(allAverages[i].ToArray(), DataProcessing.MergeSort(currentAverages[i].ToArray())).ToList();
                if (combinedAverages[i].Count() > numberOfAverages)
                {
                    combinedAverages[i].RemoveRange(numberOfAverages, combinedAverages[i].Count() - numberOfAverages);
                }

                string[] saveAverages = new string[combinedAverages[i].Count()];
                for (int j = 0; j < saveAverages.Length; j++)
                {
                    saveAverages[j] = combinedAverages[i][j].ToString() + $",{(combinedAverages[i][j].BackColor == Constants.GetColor("TimeBoxPBColor")).ToString().ToLower()}";
                }

                File.WriteAllLines(Path.Combine(DataProcessing.GetCacheFolder(), GetPuzzleName(puzzle), $"average{i}.csv"), saveAverages);
            }
        }
        /// <summary>
        /// Saves the last 1000 times for the specified puzzle to the cache.
        /// </summary>
        /// <param name="puzzle">The puzzle to save.</param>
        /// <param name="lastThousand">The 1000 most recent times for this puzzle.</param>
        /// <param name="currentTimes">The list of times from this session.</param>
        private void SaveCacheThousand(int puzzle, List<Time> lastThousand, List<Time> currentTimes)
        {
            lastThousand.AddRange(currentTimes);

            if (lastThousand.Count() > 1000)
            {
                lastThousand.RemoveRange(0, lastThousand.Count() - 1000);
            }

            string[] saveTimes = new string[lastThousand.Count()];
            for (int i = 0; i < saveTimes.Length; i++)
            {
                saveTimes[i] = lastThousand[i].ToString() + "," + lastThousand[i].Filepath;
            }

            File.WriteAllLines(Path.Combine(DataProcessing.GetCacheFolder(), GetPuzzleName(puzzle), "thousand.csv"), saveTimes);
        }
    }
}