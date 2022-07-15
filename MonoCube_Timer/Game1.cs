/*  MonoCube Timer: A timing program primarily intended for speedcubing.
    Copyright (C) 2021-2022, Greyson Gould

    This program is free software: you can redistribute it and/or modify
    it under the terms of the GNU General Public License version 3, as
    published by the Free Software Foundation.

    This program is distributed in the hope that it will be useful,
    but WITHOUT ANY WARRANTY; without even the implied warranty of
    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
    GNU General Public License for more details.

    You should have received a copy of the GNU General Public License
    along with this program.  If not, see <https://www.gnu.org/licenses/>.

    For comments, questions, or suggestions please contact me at:
    greyson.gould@protonmail.com
    */


using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System.Diagnostics;
using System.IO;

namespace MonoCube_Timer
{
    public enum WCAPuzzle { Null = 0, Two = 2, Three = 3, Four = 4, Five = 5, Six = 6, Seven = 7, Pyraminx = 11, Megaminx = 12, Skewb = 13, SquareOne = 14 } // Do not change these numbers!!

    /// <summary>
    /// Stores an average of multiple times, with a start/end date, puzzle type, and other information.
    /// </summary>
    public class Average
    {
        /// <summary>
        /// The true average time in milliseconds.
        /// </summary>
        public int Milliseconds { get; set; }
        /// <summary>
        /// An time used for comparison, in milliseconds.  DNF's are greater than any other time.
        /// </summary>
        public int ComparisonMilliseconds
        {
            get
            {
                if (DNF)
                {
                    return int.MaxValue;
                }
                else
                {
                    return Milliseconds;
                }
            }
        }

        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public string Comments { get; set; }
        public string Puzzle { get; set; }
        public bool DNF { get; set; }

        public Color TextColor { get; set; }
        public Color? BackColor { get; set; }

        public Average()
        {

        }

        /// <summary>
        /// Produces a new Average with the specified properties.
        /// </summary>
        /// <param name="time">The average time in milliseconds.</param>
        /// <param name="dnf">Whether this average is a DNF.</param>
        /// <param name="startDate">The date of the first time in this average.</param>
        /// <param name="endDate">The date of the last time in this average.</param>
        /// <param name="puzzle">The puzzle this average was performed on.</param>
        /// <param name="textColor">The text color of the times.</param>
        /// <param name="backColor">The (nullable) back color of the times.  Default if not specified.</param>
        /// <param name="comments">User comments attached to this average.</param>
        public Average(int time, bool dnf, DateTime startDate, DateTime endDate, string puzzle, Color textColor, Color? backColor, string comments = "")
        {
            this.Milliseconds = time;
            this.DNF = dnf;
            this.Comments = comments;
            this.Puzzle = puzzle;
            this.StartDate = startDate;
            this.EndDate = endDate;

            this.TextColor = textColor;
            this.BackColor = backColor;
        }

        /// <summary>
        /// Produces an average from a list of times, automatically determining the starting and ending dates, as well as whether the average is a DNF.
        /// </summary>
        /// <param name="times">The list of times to average.</param>
        /// <param name="textColor">The text color of the times.</param>
        /// <param name="backColor">The (nullable) back color of the times.  Default if not specified.</param>
        /// <param name="removeHighestAndLowest">Specifies whether the highest and lowest times should be removed before averaging.</param>
        /// <param name="comments">User comments attached to this average.</param>
        public Average(List<Time> times, Color textColor, Color? backColor, bool removeHighestAndLowest, string comments = "")
        {
            int dnfs = 0;

            int lowest = int.MaxValue;
            int highest = int.MinValue;
            DateTime startDate = times[0].DateRecorded;
            DateTime endDate = times[0].DateRecorded;

            long averageMilliseconds = 0;
            long dnfMilliseconds = 0;  // Treated differently depending on whether removeHighestAndLowest is enabled.

            // Loop through the times, counting total milliseconds in DNF solves and regular solves separately.
            // Also determines the quickest and slowest solves, as well as the earliest and latest.
            for (int i = 0; i < times.Count(); i++)
            {
                if (times[i].DNF)
                {
                    dnfs++;
                    dnfMilliseconds += times[i].Milliseconds;
                }
                else
                {
                    lowest = times[i].Milliseconds < lowest ? times[i].Milliseconds : lowest;
                    highest = times[i].Milliseconds > highest ? times[i].Milliseconds : highest;
                    averageMilliseconds += times[i].Milliseconds;

                    startDate = times[i].DateRecorded < startDate ? times[i].DateRecorded : startDate;
                    endDate = times[i].DateRecorded > endDate ? times[i].DateRecorded : endDate;
                }
            }

            // Remove the highest and lowest times if necessary
            // times.Count() >= 3 avoids the case where (times.Count() - 2) gives a divide by zero error
            if (removeHighestAndLowest && times.Count() >= 3)
            {
                if (dnfs == 1)
                {
                    // A single dnf can be excluded from the average as the highest time
                    this.Milliseconds = (int)(averageMilliseconds - lowest) / (times.Count() - 2);
                    dnfs--;
                }
                else
                {
                    this.Milliseconds = (int)(averageMilliseconds - lowest - highest) / (times.Count() - 2);
                }
            }
            else
            {
                this.Milliseconds = ((int)averageMilliseconds + (int)dnfMilliseconds) / times.Count();
            }

            // Checks if the average is still a DNF.  Above 20 times, DNFing an average becomes mostly meaningless, and thus is ignored.
            this.DNF = dnfs > 0 && times.Count < 20;

            // Sets the rest of the parameters
            this.Comments = comments;
            this.Puzzle = times[0].Puzzle; // Breaks if a list with multiple puzzles is given, but this should never happen.
            this.StartDate = startDate;
            this.EndDate = endDate;

            this.TextColor = textColor;
            this.BackColor = backColor;
        }

        public override string ToString()
        {
            return $"{this.Milliseconds.ToString()},{this.DNF.ToString()},{this.StartDate.ToString()},{this.EndDate.ToString()},{this.Puzzle},{this.Comments}";
        }
        public Average Copy()
        {
            return new Average(this.Milliseconds, this.DNF, this.StartDate, this.EndDate, this.Puzzle, this.TextColor, this.BackColor, this.Comments);
        }

        public static bool operator >(Average a, Average b) => a.ComparisonMilliseconds > b.ComparisonMilliseconds;
        public static bool operator <(Average a, Average b) => a.ComparisonMilliseconds < b.ComparisonMilliseconds;
        public static bool operator >=(Average a, Average b) => a.ComparisonMilliseconds >= b.ComparisonMilliseconds;
        public static bool operator <=(Average a, Average b) => a.ComparisonMilliseconds <= b.ComparisonMilliseconds;
        public static bool operator ==(Average a, Average b) => a.ComparisonMilliseconds == b.ComparisonMilliseconds;
        public static bool operator !=(Average a, Average b) => a.ComparisonMilliseconds != b.ComparisonMilliseconds;
        public override bool Equals(object obj)
        {
            if (obj is Average)
            {
                Average a = (Average)obj;

                if (this.Milliseconds == a.Milliseconds &&
                    this.StartDate == a.StartDate &&
                    this.EndDate == a.EndDate &&
                    this.Comments == a.Comments &&
                    this.Puzzle == a.Puzzle &&
                    this.DNF == a.DNF)
                {
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// Bad hash function
        /// </summary>
        /// <returns></returns>
        public override int GetHashCode()
        {
            return (int)(this.StartDate.Ticks + this.EndDate.Ticks + this.Milliseconds * (this.DNF ? 23456789 : 23456819));
        }
    }
    
    /// <summary>
    /// Stores a single time, with puzzle type, comments, and other information.
    /// </summary>
    public class Time
    {

        private int milliseconds;

        /// <summary>
        /// The time in milliseconds, accounting for +2's.
        /// </summary>
        public int Milliseconds
        {
            get
            {
                if (Plus2)
                {
                    return milliseconds + 2000;
                }
                else
                {
                    return milliseconds;
                }
            }
        }
        /// <summary>
        /// The true time in milliseconds, disregarding +2's.
        /// </summary>
        public int TrueMilliseconds
        {
            get
            {
                return milliseconds;
            }
            set
            {
                this.milliseconds = value;
            }
        }
        /// <summary>
        /// An time used for comparison, in milliseconds.  DNF's are greater than any other time.
        /// </summary>
        public int ComparisonMilliseconds
        {
            get
            {
                if (this.DNF)
                {
                    return int.MaxValue;
                }
                else
                {
                    return Milliseconds;
                }
            }
        }

        public string Filepath { get; set; }
        public bool Plus2 { get; set; }
        public bool DNF { get; set; }

        public string Scramble { get; set; }
        public DateTime DateRecorded { get; set; }
        public string Comments { get; set; }
        public string Puzzle { get; set; }

        public Color TextColor { get; set; }
        public Color? BackColor { get; set; }

        public Time()
        {

        }

        /// <summary>
        /// Produces a new Time with the specified properties, using the current date.
        /// </summary>
        /// <param name="time">The time in milliseconds.</param>
        /// <param name="plus2">Whether this time is a +2.</param>
        /// <param name="dnf">Whether this time is a DNF.</param>
        /// <param name="puzzle">The puzzle this time was performed on.</param>
        /// <param name="textColor">The text color of the time.</param>
        /// <param name="backColor">The (nullable) back color of the times.  Default if not specified.</param>
        /// <param name="scramble">The scramble for this time.</param>
        /// <param name="comments">User comments attached to this average.</param>
        /// <param name="filePath">The file path this time is stored at (used for deleting old times).</param>
        public Time(int time, bool plus2, bool dnf, string puzzle, Color textColor, Color? backColor, string scramble = "No Scramble Data", string comments = "", string filePath = "")
        {
            this.Filepath = filePath;
            this.milliseconds = time;
            this.Plus2 = plus2;
            this.DNF = dnf;
            this.Scramble = scramble;
            this.Comments = comments;
            this.Puzzle = puzzle;
            this.DateRecorded = DateTime.Now;

            this.TextColor = textColor;
            this.BackColor = backColor;
        }

        /// <summary>
        /// Produces a new Time with the specified properties.
        /// </summary>
        /// <param name="time">The time in milliseconds.</param>
        /// <param name="plus2">Whether this time is a +2.</param>
        /// <param name="dnf">Whether this time is a DNF.</param>
        /// <param name="dateRecorded">The date this time was recorded.</param>
        /// <param name="puzzle">The puzzle this time was performed on.</param>
        /// <param name="textColor">The text color of the time.</param>
        /// <param name="backColor">The (nullable) back color of the times.  Default if not specified.</param>
        /// <param name="scramble">The scramble for this time.</param>
        /// <param name="comments">User comments attached to this average.</param>
        /// <param name="filePath">The file path this time is stored at (used for deleting old times).</param>
        public Time(int time, bool plus2, bool dnf, DateTime dateRecorded, string puzzle, Color textColor, Color? backColor, string scramble = "No Scramble Data", string comments = "", string filePath = "")
        {
            this.Filepath = filePath;
            this.milliseconds = time;
            this.Plus2 = plus2;
            this.DNF = dnf;
            this.Scramble = scramble;
            this.Comments = comments;
            this.Puzzle = puzzle;
            this.DateRecorded = dateRecorded;

            this.TextColor = textColor;
            this.BackColor = backColor;
        }

        public override string ToString()
        {
            return $"{this.TrueMilliseconds.ToString()},{this.Plus2.ToString().ToLower()},{this.DNF.ToString().ToLower()},{this.DateRecorded.ToString()},{this.Puzzle},{this.Scramble},{this.Comments}";
        }
        public Time Copy()
        {
            return new Time(this.TrueMilliseconds, this.Plus2, this.DNF, this.DateRecorded, this.Puzzle, this.TextColor, this.BackColor, this.Scramble, this.Comments, this.Filepath);
        }

        public static bool operator >(Time a, Time b) => a.ComparisonMilliseconds > b.ComparisonMilliseconds;
        public static bool operator <(Time a, Time b) => a.ComparisonMilliseconds < b.ComparisonMilliseconds;
        public static bool operator >=(Time a, Time b) => a.ComparisonMilliseconds >= b.ComparisonMilliseconds;
        public static bool operator <=(Time a, Time b) => a.ComparisonMilliseconds <= b.ComparisonMilliseconds;
        public static bool operator ==(Time a, Time b) => a.ComparisonMilliseconds == b.ComparisonMilliseconds;
        public static bool operator !=(Time a, Time b) => a.ComparisonMilliseconds != b.ComparisonMilliseconds;

        public override bool Equals(object obj)
        {
            if (obj is Time)
            {
                Time a = (Time)obj;

                if (this.TrueMilliseconds == a.TrueMilliseconds &&
                    this.Plus2 == a.Plus2 &&
                    this.DNF == a.DNF &&
                    this.Scramble == a.Scramble &&
                    this.Comments == a.Comments &&
                    this.Puzzle == a.Puzzle &&
                    this.DateRecorded == a.DateRecorded)
                {
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// Bad hash function.
        /// </summary>
        /// <returns></returns>
        public override int GetHashCode()
        {
            return (int)(this.DateRecorded.Ticks + this.Milliseconds * (this.DNF ? 23456789 : 23456819));
        }
    }

    //ZDepth / Draworder / Colors are handled in the Constants class

    /// <summary>
    /// This is the main type for your game.
    /// </summary>
    public class Game1 : Game
    {
        int[] maxLengths = { 3, 5, 12, 20, 50, 100, 1000 }; // The sizes of the averages displayed
        const int TimeCap = 1000000; // A per-category cap on the number of times to load
        const int AverageCap = 1000000; // A per-category cap on the number of averages to load
        const int TimerDelay = 600; //ms


        // Logging tools
        Stopwatch loggingTimer;


        // Monogame graphics and backend stuff
        GraphicsDeviceManager graphics;
        SpriteBatch spriteBatch;
        GameContent gameContent; //Stores all the sprites and fonts for the project
        ImportData importData;


        //Statistics and Scrolling boxes for displaying times
        StatsContainer SessionStatistics; //Left
        StatsContainer AllTimeStatistics; //Right
        ScrollContainer CurrentSession; //Left

        ScrollContainer AllTimes; //Right (single best)
        AverageDisplayScrollContainer[] AllAverages; //Right (averages)


        //Used for displaying stats about a specific time
        TimeDisplayWindow timeStatsWindow;
        bool statsDisplaying; //true when stats are being displayed
        Time statsTimeViewed; //The time currently being viewed.  Must be global so that its comments can be changed when the viewing window is closed.

        CubeSelectWindow cubeSelectWindow; // Used to select a type of cube other than 2x2 through 7x7


        //Tabs for switching between scrollContainers
        Tab SessionTab; //Left
        Tab[] AllTimeTabs; //Right
        int currentAllTimeTab; //Keeps track of which right-hand tab the user is viewing.
        bool currentTabChanged; //True for one tick when the current tab is changed.


        // Monogame IO requires that we keep track of the previous mouse, keyboard, and window focus state to compare them to the current ones
        private KeyboardState oldKeyboardState;
        private MouseState oldMouseState;
        bool oldWindowFocus = true;


        // Global Constants and useful variables
        const int ScrollContainerHeight = 325;
        int numberOfPuzzles;

        List<string> sessionId; // File names for each type of cube

        int screenWidth;
        int screenHeight;

        double spaceDownTime; //How long the space key must be held befor the timer will start on release.
        bool resetCooldown; //True once the timer is stopped, so that stopping the timer will not also reset it.
        Color timerColor; //The color of the timer -- changes based on how long the space key has been held.  Must be global so that it can be used in the Draw method.

        string scramble; // The scramble currently being displayed
        int currentPuzzle; // Which puzzle it is is stored in the ImportData class

        bool timing; //True when the timer is running
        Stopwatch timer; // The actual timer


        // Selection buttons
        Button[] buttons; //Cube selection buttons at the bottom
        Button PuzzleSelectButton; // The button for selecting additional/custom cubes
        Button PBToggleButton; // Toggling PB-only view


        // Stores the times and averages for the current session
        List<List<Time>> CurrentSessionTimes; //Changed after initial setup.
        List<List<Average>[]> CurrentSessionAverages; //Changed after initial setup

        //These lists are the only things loaded from save files on disk.  After the processing during startup, never reloading these saves
        // having to re-read all the files, which can take a lot of processing time.
        List<List<Average>[]> AllTimeAverages; //Never changed after initial setup
        List<List<Time>> LastThousandTimes; //Never changed after initial setup
        List<List<Time>> AllTimeTimes; //Never changed after initial setup

        int[] sessionTimeIndices; // The indices of text within the sessionTime statsbox
        int[] allTimeIndices; // The indices of text within the allTime statsbox


        public Game1()
        {
            graphics = new GraphicsDeviceManager(this);
            Window.Title = "Cube Timer";
            Content.RootDirectory = "Content";

            //Set screen size (must be done in constructor so that the initial render is the right size.
            screenWidth = 1500;
            screenHeight = 900;
            graphics.PreferredBackBufferWidth = screenWidth;
            graphics.PreferredBackBufferHeight = screenHeight;
            // Fullscreen is buggy in monogame
            //Window.IsBorderless = true;
            //graphics.IsFullScreen = true;
            //graphics.ApplyChanges();
        }

        /// <summary>
        /// Sets Monogame related parameters, activates the data import class, and gets ids for the new session
        /// </summary>
        protected override void Initialize()
        {
            this.IsMouseVisible = true;
            this.Window.AllowUserResizing = true;
            this.Window.ClientSizeChanged += WindowSizeChanged;

            this.IsFixedTimeStep = true;
            this.TargetElapsedTime = TimeSpan.FromSeconds(1d / 144d); //We choose 144 fps as the target so that the timer is theoretically accurate to the hundredth of a second.

            statsDisplaying = false;
            timing = false;
            resetCooldown = false;
            timer = new Stopwatch();

            this.Exiting += Game1_Exiting;

            base.Initialize();
        }

        /// <summary>
        /// Triggered whenever the window size changes.  
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void WindowSizeChanged(object sender, EventArgs e)
        {
            screenWidth = Math.Max(1200, graphics.PreferredBackBufferWidth);
            screenHeight = Math.Max(640, graphics.PreferredBackBufferHeight);

            graphics.PreferredBackBufferWidth = screenWidth;
            graphics.PreferredBackBufferHeight = screenHeight;
            graphics.ApplyChanges();
            Log.Debug($"Window size changed to {screenWidth}x{screenHeight}.");

            SetEverythingLocation();
        }


        /// <summary>
        /// LoadContent loads textures, sets up logging, and then initializes all on-screen objects.
        ///   Finally, past times are loaded.
        /// </summary>
        protected override void LoadContent()
        {
            // Sets up the timer for logging long events
            loggingTimer = new Stopwatch();
            loggingTimer.Restart();

            SetupLogging();

            // Create a new SpriteBatch, which can be used to draw textures.
            spriteBatch = new SpriteBatch(GraphicsDevice);
            // Create a new GameContent, which holds textures and fonts.
            gameContent = new GameContent(Content);


            // Set up color scheme
            Constants.Colors = new Dictionary<string, Color>();
            if (File.Exists(Path.Combine(DataProcessing.GetRootFolder(), "colorscheme.xml")))
            {
                Constants.LoadColorScheme(File.ReadAllText(Path.Combine(DataProcessing.GetRootFolder(), "colorscheme.xml")));
            }


            // Initialize an instance of the ImportData class, which will be used for all file IO
            importData = new ImportData(maxLengths);
            // Checks to make sure that the folders in the save paths for the current month/year are all created.
            importData.ConfirmFolders();
            this.numberOfPuzzles = importData.ListPuzzles().Count();

            // SessionId's are the unique file names for each puzzle, generated here.
            sessionId = new List<string>();
            for (int i = 0; i < this.numberOfPuzzles; i++)
            {
                sessionId.Add(importData.GetSessionId(i));
            }

            // Sets the current puzzle to 3x3 if it exists, otherwise the first puzzle in the list.
            currentPuzzle = Math.Max(0, importData.GetPuzzleNumber("3x3"));
            scramble = ScrambleGenerator.GenerateScramble(importData.GetPuzzleType(currentPuzzle));


            CurrentSessionTimes = new List<List<Time>>();
            CurrentSessionAverages = new List<List<Average>[]>();
            AllTimeTimes = new List<List<Time>>();
            LastThousandTimes = new List<List<Time>>();
            AllTimeAverages = new List<List<Average>[]>();

            //Set up existing (stored) times for each puzzle
            for (int i = 0; i < numberOfPuzzles; i++)
            {
                CurrentSessionTimes.Add(new List<Time>());
                CurrentSessionAverages.Add(new List<Average>[7]);

                for (int j = 0; j < 7; j++)
                {
                    CurrentSessionAverages[i][j] = new List<Average>();
                }

                //Times and averages are capped because the user could potentially have hundreds of thousands of times, which would slow down operations significantly
                // Instead, we process all those times once on startup and keep only relevant ones.
                AllTimeTimes.Add(importData.GetSortedTimes(sessionId[i], TimeCap, i));

                List<Time> newLastThousand;
                AllTimeAverages.Add(importData.GetSortedAverages(sessionId[i], AverageCap, i, out newLastThousand));
                LastThousandTimes.Add(newLastThousand);
            }

            InitializeEverything();

            CurrentSession.ChangeData(CurrentSessionTimes[currentPuzzle]);
            UpdateDisplays();


            loggingTimer.Stop();
            if (importData.CacheUpToDate)
            {
                Log.System($"Loaded from cache in {loggingTimer.ElapsedMilliseconds} milliseconds");
            }
            else
            {
                Log.System($"Loaded from file (cache missing or out of date) in {loggingTimer.ElapsedMilliseconds} milliseconds");
            }
        }

        /// <summary>
        /// Initializes logging and saves the log from the previous session.
        /// </summary>
        public void SetupLogging()
        {
            AppDomain.CurrentDomain.UnhandledException += LogUnhandledException;
            Log.SavePreviousLog();
        }

        private void LogUnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            Log.Fatal((Exception)e.ExceptionObject);
        }


        /// <summary>
        /// Resets the location of every visible object.
        /// </summary>
        private void SetEverythingLocation()
        {
            // Cube Button space Calculations.
            int buffer = 40;
            int usableScreenwidth = screenWidth - (2 * (360 + buffer));
            int buttonWidth = 0;
            {
                Button measureSize = new Button(gameContent, spriteBatch, gameContent.buttonFont);
                measureSize.SetText("3x3");
                buttonWidth = measureSize.Width;
            }
            int betweenButtonSpace = Math.Min(40, (usableScreenwidth - 6 * buttonWidth) / 5);

            for (int i = 0; i < 6; i++)
            {
                buttons[i].Location = new Vector2((float)Math.Round(screenWidth / 2 + (i - 2.5f) * betweenButtonSpace + (i - 3) * buttonWidth), screenHeight - 40 - buttons[i].Height);
            }

            PBToggleButton.Location = new Vector2(360 - PBToggleButton.Width, ScrollContainerHeight - (PBToggleButton.Height + 5));


            // Displays a custom puzzle name if appropriate, otherwise "Select Puzzle".
            if (PuzzleSelectButton.GetText() != "Select Puzzle")
            {
                SetPuzzleSelectText(importData.GetPuzzleName(currentPuzzle));
            }
            PuzzleSelectButton.Location = new Vector2((screenWidth - PuzzleSelectButton.Width) / 2, screenHeight - 40 - buttons[0].Height - PuzzleSelectButton.Height - 20);


            // Sets statistics locations.
            SessionStatistics.Location = new Vector2(5, 5);
            AllTimeStatistics.Location = new Vector2(screenWidth - 365, 5);


            // Sets tab locations.
            SessionTab.Location = new Vector2(magicTabNumber + SessionTab.borderSize, ScrollContainerHeight - SessionTab.Height);
            int toggledTab = 0;
            for (int i = 0; i < AllTimeTabs.Length; i++)
            {
                if (AllTimeTabs[i].IsToggled)
                {
                    toggledTab = i;
                    break;
                }
            }
            SetTabLocations();
            if (toggledTab >= 4)
            {
                AdjustAllTimeTabOrder(toggledTab);
            }


            // Sets scroll container locations and sizes.
            CurrentSession.Location = new Vector2(5, ScrollContainerHeight);
            CurrentSession.Size = new System.Drawing.Size(360, screenHeight - ((int)Math.Round(CurrentSession.Location.Y) + 5));

            AllTimes.Location = new Vector2(screenWidth - 365, ScrollContainerHeight);
            AllTimes.Size = new System.Drawing.Size(360, screenHeight - ((int)Math.Round(AllTimes.Location.Y) + 5));

            for (int i = 0; i < 7; i++)
            {
                AllAverages[i].Location = new Vector2(screenWidth - 365, ScrollContainerHeight);
                AllAverages[i].Size = new System.Drawing.Size(360, screenHeight - ((int)Math.Round(AllTimes.Location.Y) + 5));
            }


            // Sets stat / selection window locations and sizes.
            System.Drawing.Size statsSize = new System.Drawing.Size(1000, 600);
            Vector2 statsLoc = new Vector2((graphics.PreferredBackBufferWidth - statsSize.Width) / 2, (graphics.PreferredBackBufferHeight - statsSize.Height) / 2);

            timeStatsWindow.Size = statsSize;
            timeStatsWindow.Location = statsLoc;

            cubeSelectWindow.Size = statsSize;
            cubeSelectWindow.Location = statsLoc;
        }

        // Initialization functions for LoadContent

        /// <summary>
        /// Initializes all on-screen objects at once, and sets their locations.
        /// </summary>
        private void InitializeEverything()
        {
            InitializeCubeButtons();
            InitializePBButton();
            InitializePuzzleSelectButton();
            InitializeStatsContainers();
            InitializeTabs();
            InitializeScrollContainers();
            InitializeDataWindows();

            SetEverythingLocation();
        }

        /// <summary>
        /// Initializes the 2x2 through 7x7 selection buttons
        /// </summary>
        private void InitializeCubeButtons()
        {
            buttons = new Button[6];

            for (int i = 0; i < 6; i++)
            {
                buttons[i] = new Button(gameContent, spriteBatch, gameContent.buttonFont);

                buttons[i].SetText($"{i + 2}x{i + 2}");
                buttons[i].ToggleOnClick = true;
                buttons[i].IsToggled = false;

                buttons[i].ToggleColor = Constants.GetColor("ButtonToggleColor");
                buttons[i].BackColor = Constants.GetColor("ButtonBackColor");
                buttons[i].BorderColor = Constants.GetColor("ButtonBorderColor");
                buttons[i].TextColor = Constants.GetColor("ButtonTextColor");

                buttons[i].Visible = true;
                buttons[i].Enabled = true;

                buttons[i].Click += Button_Click;
            }

            buttons[1].IsToggled = true;
            buttons[1].Enabled = false;
        }
        /// <summary>
        /// Initializes the puzzle select button
        /// </summary>
        private void InitializePuzzleSelectButton()
        {
            PuzzleSelectButton = new Button(gameContent, spriteBatch, gameContent.buttonFont);

            PuzzleSelectButton.SetText("Select Puzzle");
            PuzzleSelectButton.ToggleOnClick = false;

            PuzzleSelectButton.ToggleColor = Constants.GetColor("ButtonToggleColor");
            PuzzleSelectButton.BackColor = Constants.GetColor("ButtonBackColor");
            PuzzleSelectButton.BorderColor = Constants.GetColor("ButtonBorderColor");
            PuzzleSelectButton.TextColor = Constants.GetColor("ButtonTextColor");

            PuzzleSelectButton.Visible = true;
            PuzzleSelectButton.Enabled = true;

            PuzzleSelectButton.Click += CubeSelectWindow_Opening;
        }
        /// <summary>
        /// Initializes the button to toggle PB only view
        /// </summary>
        private void InitializePBButton()
        {
            PBToggleButton = new Button(gameContent, spriteBatch, gameContent.menuTitleFont);

            PBToggleButton.SetText("All Timys"); // The largest possible size.
            PBToggleButton.AutoSize = false;     // Keeps the button at its largest possible size forever
            PBToggleButton.SetText("PBs Only");
            PBToggleButton.ToggleOnClick = true;
            PBToggleButton.IsToggled = false;

            PBToggleButton.ToggleColor = Constants.GetColor("ButtonToggleColor");
            PBToggleButton.BackColor = Constants.GetColor("ButtonBackColor");
            PBToggleButton.BorderColor = Constants.GetColor("ButtonBorderColor");
            PBToggleButton.TextColor = Constants.GetColor("ButtonTextColor");

            PBToggleButton.Visible = true;
            PBToggleButton.Enabled = true;

            PBToggleButton.Click += TogglePBView;
        }

        /// <summary>
        /// Switches between regular and PB only view
        /// </summary>
        /// <param name="arg1">The button sending the event.</param>
        /// <param name="arg2">The index number of that button</param>
        private void TogglePBView(object arg1, long arg2)
        {
            bool value;
            if (PBToggleButton.IsToggled)
            {
                PBToggleButton.SetText("PBs Only");
                value = false;
                Log.Info("PB view toggled on");
            }
            else
            {
                PBToggleButton.SetText("All Times");
                value = true;
                Log.Info("PB view toggled off");
            }

            PBToggleButton.Location = new Vector2(360 - PBToggleButton.Width, PBToggleButton.Location.Y);

            AllTimes.UpdateFilter(new Filter(value, false));
            for (int i = 0; i < AllAverages.Length; i++)
            {
                AllAverages[i].UpdateFilter(new Filter(value, false));
            }
        }


        /// <summary>
        /// Initializes the stats containers.
        /// </summary>
        private void InitializeStatsContainers()
        {
            SessionStatistics = new StatsContainer(gameContent, spriteBatch, gameContent.menuTitleFont, gameContent.menuFont);

            SessionStatistics.Size = new System.Drawing.Size(360, 240);
            SessionStatistics.BackColor = Constants.GetColor("ContainerColor");
            SessionStatistics.TextColor = Constants.GetColor("StatsBoxTextColor");
            SessionStatistics.Title = "Session Statistics";
            SessionStatistics.DrawSeparatingStroke = true;

            SessionStatistics.Enabled = true;
            SessionStatistics.Visible = true;

            AllTimeStatistics = new StatsContainer(gameContent, spriteBatch, gameContent.menuTitleFont, gameContent.menuFont);

            AllTimeStatistics.Size = new System.Drawing.Size(360, 240);
            AllTimeStatistics.BackColor = Constants.GetColor("ContainerColor");
            AllTimeStatistics.TextColor = Constants.GetColor("StatsBoxTextColor");
            AllTimeStatistics.Title = "All Time Statistics";

            AllTimeStatistics.Enabled = true;
            AllTimeStatistics.Visible = true;

            {
                int[] vPos = { 0, 20, 50, 70, 100, 120, 150, 170 };
                string[] text1 = { "Best:", "Average:", "Mean 3:", "Best Mean 3:", "3 of 5:", "Best 3 of 5:", "10 of 12:", "Best 10 of 12:" };
                string[] text2 = { "Avg 20:", "Best Avg 20:", "Avg 50:", "Best Avg 50:", "Avg 100:", "Best Avg 100:", "Avg 1000:", "Best Avg 1000:" };
                string[] text3 = { "Best:", "Best Mean 3:", "Best 3 of 5:", "Best 10 of 12:", "Best Avg 20:", "Best Avg 50:", "Best Avg 100:", "Best Avg 1000:" };

                int column1 = 100;
                int column2 = SessionStatistics.Size.Width / 2 + column1;

                sessionTimeIndices = new int[16];
                allTimeIndices = new int[8];

                for (int i = 0; i < 8; i++)
                {
                    SessionStatistics.AddText(new Vector2(column1, vPos[i]), text1[i], HAlignment.Right, VAlignment.Top);
                    SessionStatistics.AddText(new Vector2(column2, vPos[i]), text2[i], HAlignment.Right, VAlignment.Top);

                    // Keeps track of the indicies where this text is stored, so it can be retrieved later
                    sessionTimeIndices[i] = SessionStatistics.AddText(new Vector2(column1 + 10, vPos[i]), "0.00", HAlignment.Left, VAlignment.Top);
                    sessionTimeIndices[i + 8] = SessionStatistics.AddText(new Vector2(column2 + 10, vPos[i]), "0.00", HAlignment.Left, VAlignment.Top);


                    AllTimeStatistics.AddText(new Vector2(SessionStatistics.Size.Width / 2, vPos[i]), text3[i], HAlignment.Right, VAlignment.Top);

                    allTimeIndices[i] = AllTimeStatistics.AddText(new Vector2(SessionStatistics.Size.Width / 2 + 10, vPos[i]), "0.00", HAlignment.Left, VAlignment.Top);

                }
            }
        }

        const int magicTabNumber = 5 + 8; // I don't know what this does any more
        /// <summary>
        /// Initializes the average selection tabs and session selection tab
        /// </summary>
        private void InitializeTabs()
        {
            SessionTab = new Tab(gameContent, spriteBatch, gameContent.menuTitleFont);

            SessionTab.BackColor = Constants.GetColor("TabBackgroundColor");
            SessionTab.ToggleColor = Constants.GetColor("ContainerColor");
            SessionTab.borderSize = 2;
            SessionTab.BorderColor = Constants.GetColor("TabBorderColor");
            SessionTab.TextColor = Constants.GetColor("TabTextColor");

            SessionTab.SetText("Current Session");

            SessionTab.Visible = true;
            SessionTab.Enabled = true;

            SessionTab.ToggleOnClick = false;
            SessionTab.IsToggled = true;

            currentAllTimeTab = 0;
            currentTabChanged = false;
            AllTimeTabs = new Tab[8];

            string[] text = new string[8] { "Best Times", "Mean 3", "3 of 5", "10 of 12", "Avg 20", "Avg 50", "Avg 100", "Avg 1000" };

            for (int i = 0; i < AllTimeTabs.Length; i++)
            {
                AllTimeTabs[i] = new Tab(gameContent, spriteBatch, gameContent.menuTitleFont);

                AllTimeTabs[i].BackColor = Constants.GetColor("TabBackgroundColor");
                AllTimeTabs[i].ToggleColor = Constants.GetColor("ContainerColor");
                AllTimeTabs[i].borderSize = 2;
                AllTimeTabs[i].BorderColor = Constants.GetColor("TabBorderColor");
                AllTimeTabs[i].TextColor = Constants.GetColor("TabTextColor");

                AllTimeTabs[i].SetText(text[i]);

                AllTimeTabs[i].Visible = true;
                AllTimeTabs[i].Enabled = true;

                AllTimeTabs[i].Click += AllTimeTabs_Click;
            }
        }
        /// <summary>
        /// Sets tab locations.  DOES NOT need to be called separately from SetEverythingLocation.
        /// </summary>
        private void SetTabLocations()
        {
            AllTimeTabs[0].Location = new Vector2((screenWidth - 370) + magicTabNumber + AllTimeTabs[0].borderSize, ScrollContainerHeight - AllTimeTabs[0].Height);
            AllTimeTabs[0].ZDepth = 0.4f;
            AllTimeTabs[0].IsToggled = true;
            for (int i = 1; i < 4; i++)
            {
                AllTimeTabs[i].Location = new Vector2(AllTimeTabs[i - 1].Location.X + AllTimeTabs[i - 1].Width + AllTimeTabs[i - 1].borderSize, ScrollContainerHeight - AllTimeTabs[0].Height);
                AllTimeTabs[i].ZDepth = 0.4f;
            }

            AllTimeTabs[4].Location = new Vector2((screenWidth - 370) + magicTabNumber + AllTimeTabs[0].borderSize, ScrollContainerHeight - AllTimeTabs[0].Height - AllTimeTabs[4].Height);
            AllTimeTabs[4].ZDepth = 0.5f;
            for (int i = 5; i < 8; i++)
            {
                AllTimeTabs[i].Location = new Vector2(AllTimeTabs[i - 1].Location.X + AllTimeTabs[i - 1].Width + AllTimeTabs[i - 1].borderSize, ScrollContainerHeight - AllTimeTabs[0].Height - AllTimeTabs[4].Height);
                AllTimeTabs[i].ZDepth = 0.5f;
            }
        }


        /// <summary>
        /// Initializes the scroll containers for current times, all times, and each average
        /// </summary>
        private void InitializeScrollContainers()
        {
            CurrentSession = new ScrollContainer(gameContent, spriteBatch, gameContent.menuFont, gameContent.menuFontBold);

            CurrentSession.BackColor = Constants.GetColor("ContainerColor");
            CurrentSession.DisplaySeparaterLines = true;
            CurrentSession.DisplayState = ContainerDisplayState.InvertButtons;

            CurrentSession.Enabled = true;
            CurrentSession.Visible = true;
            CurrentSession.UpdateCollisionDetection();
            CurrentSession.DisplayTime += StatsWindow_Opening;


            AllTimes = new ScrollContainer(gameContent, spriteBatch, gameContent.menuFont, gameContent.menuFontBold);

            AllTimes.BackColor = Constants.GetColor("ContainerColor");
            AllTimes.TimeOffset = 160;
            AllTimes.NumberOffset = 70;
            AllTimes.DisplaySeparaterLines = false;
            AllTimes.DisplayState = ContainerDisplayState.Dates;

            AllTimes.Enabled = true;
            AllTimes.Visible = true;
            CurrentSession.TimeChanged += TimeChanged;
            CurrentSession.TimeDeleted += TimeDeleted;
            AllTimes.DisplayTime += StatsWindow_Opening;


            AllAverages = new AverageDisplayScrollContainer[7];
            for (int i = 0; i < 7; i++)
            {
                AllAverages[i] = new AverageDisplayScrollContainer(gameContent, spriteBatch, gameContent.menuFont, gameContent.menuFontBold);

                AllAverages[i].BackColor = Constants.GetColor("ContainerColor");
                AllAverages[i].TimeOffset = 160;
                AllAverages[i].NumberOffset = 70;

                AllAverages[i].Enabled = false;
                AllAverages[i].Visible = false;
                AllAverages[i].DisplayTime += StatsWindow_Opening;
            }
        }

        /// <summary>
        /// Initializes the time stats display window and cube selection window
        /// </summary>
        private void InitializeDataWindows()
        {
            timeStatsWindow = new TimeDisplayWindow(gameContent, spriteBatch, new Time(), Vector2.Zero, System.Drawing.Size.Empty, false);
            timeStatsWindow.Enabled = false;
            timeStatsWindow.Visible = false;

            timeStatsWindow.Closing += StatsWindow_Closing;


            cubeSelectWindow = new CubeSelectWindow(gameContent, spriteBatch, importData.ListPuzzleNames().ToList(), Vector2.Zero, System.Drawing.Size.Empty);
            cubeSelectWindow.Enabled = false;
            cubeSelectWindow.Visible = false;

            cubeSelectWindow.Closing += CubeSelectWindow_Closing;
            cubeSelectWindow.CategoryAdded += CubeSelectWindow_CategoryAdded;
            cubeSelectWindow.CategorySelected += CubeSelectWindow_CategorySelected;
        }
        // End initialization functions


        /// <summary>
        /// Triggered when the user selects a cube from the selection window.
        /// </summary>
        /// <param name="arg1">The button sending the event.</param>
        /// <param name="arg2">The index number of the sender.</param>
        /// <param name="arg3">The name of the puzzle selected.</param>
        private void CubeSelectWindow_CategorySelected(object arg1, long arg2, string arg3)
        {
            Log.Info($"User selected puzzle {arg3} from the selection window");
            currentPuzzle = importData.GetPuzzleNumber(arg3);
            Button_Click(arg1, arg2);
        }
        /// <summary>
        /// Triggered when the user adds a custom puzzle using the cube selection window.
        /// </summary>
        /// <param name="arg1">The button sending the event.</param>
        /// <param name="arg2">The index number of the sender.</param>
        /// <param name="arg3">The name of the puzzle being added.</param>
        private void CubeSelectWindow_CategoryAdded(object arg1, long arg2, string arg3)
        {
            if (!importData.AddPuzzle(arg3))
            {
                Log.Warn($"User attempted to add already existing puzzle {arg3} using the selection window.");
                return;
            }
            Log.Info($"User added new puzzle {arg3} using the selection window");

            int newIndex = importData.GetPuzzleNumber(arg3);

            sessionId.Add(importData.GetSessionId(newIndex));

            
            // Sets up time and average storage for the new puzzle
            CurrentSessionTimes.Add(new List<Time>());
            CurrentSessionAverages.Add(new List<Average>[7]);

            for (int j = 0; j < 7; j++)
            {
                CurrentSessionAverages[newIndex][j] = new List<Average>();
            }

            AllTimeTimes.Add(importData.GetSortedTimes(sessionId[newIndex], TimeCap, newIndex));

            List<Time> newLastThousand;
            AllTimeAverages.Add(importData.GetSortedAverages(sessionId[newIndex], AverageCap, newIndex, out newLastThousand));
            LastThousandTimes.Add(newLastThousand);

            this.numberOfPuzzles = importData.ListPuzzles().Count();
            CubeSelectWindow_CategorySelected(arg1, arg2, arg3);
        }


        // Tab clicking and shifting
        /// <summary>
        /// Triggered when one of the average scrollbox selection tabs is clicked.
        /// </summary>
        /// <param name="arg1">The tab that was clicked.</param>
        /// <param name="arg2">The index number of the sender.</param>
        private void AllTimeTabs_Click(object arg1, long arg2)
        {
            HideAllTimeDisplays();
            for (int i = 0; i < AllTimeTabs.Length; i++)
            {
                if (AllTimeTabs[i].Index == arg2)
                {
                    currentAllTimeTab = i;
                    currentTabChanged = true;
                    return;
                }
            }
            Log.Error($"Attempted to click an invalid tab \"{arg1.ToString()}\".");
        }

        /// <summary>
        /// Shuffles the order of the two rows of average tabs so that the row with the active tab is always the bottom one
        /// </summary>
        /// <param name="tab">The currently active tab</param>
        private void AdjustAllTimeTabOrder(int tab)
        {
            for (int i = 0; i < AllTimeTabs.Length; i++)
            {
                AllTimeTabs[i].IsToggled = false;
            }
            AllTimeTabs[tab].IsToggled = true;

            if (tab < 4)
            {
                for (int i = 0; i < 4; i++)
                {
                    AllTimeTabs[i].ZDepth = 0.4f;
                    AllTimeTabs[i].Location = new Vector2(AllTimeTabs[i].Location.X, ScrollContainerHeight - AllTimeTabs[0].Height);
                }
                for (int i = 4; i < 8; i++)
                {
                    AllTimeTabs[i].ZDepth = 0.5f;
                    AllTimeTabs[i].Location = new Vector2(AllTimeTabs[i].Location.X, ScrollContainerHeight - AllTimeTabs[0].Height - AllTimeTabs[4].Height);
                }
            }
            else if (tab < 8)
            {
                for (int i = 0; i < 4; i++)
                {
                    AllTimeTabs[i].ZDepth = 0.5f;
                    AllTimeTabs[i].Location = new Vector2(AllTimeTabs[i].Location.X, ScrollContainerHeight - AllTimeTabs[0].Height - AllTimeTabs[4].Height);
                }
                for (int i = 4; i < 8; i++)
                {
                    AllTimeTabs[i].ZDepth = 0.4f;
                    AllTimeTabs[i].Location = new Vector2(AllTimeTabs[i].Location.X, ScrollContainerHeight - AllTimeTabs[4].Height);
                }
            }
            else
            {
                Log.Warn($"Attempted to select invalid tab {tab}.");
            }

            HideAllTimeDisplays();
            if (tab == 0)
            {
                AllTimes.Visible = true;
                AllTimes.Enabled = true;
            }
            else
            {
                AllAverages[tab - 1].Visible = true;
                AllAverages[tab - 1].Enabled = true;
            }
        }
        /// <summary>
        /// Hides the all times and all averages displays
        /// </summary>
        private void HideAllTimeDisplays()
        {
            AllTimes.Visible = false;
            AllTimes.Enabled = false;

            for (int i = 0; i < AllAverages.Length; i++)
            {
                AllAverages[i].Visible = false;
                AllAverages[i].Enabled = false;
            }
        }
        // End tab stuff


        // Opening and closing of popup windows
        /// <summary>
        /// Called when a time displayed in a scrollbox is clicked, and opens the stats window corresponding to that time.
        /// </summary>
        /// <param name="sender">The scrollbox whose time was clicked.</param>
        /// <param name="time">The time that was clicked.</param>
        private void StatsWindow_Opening(object sender, Time time)
        {
            if (sender is ScrollContainer && !(sender is AverageDisplayScrollContainer))
            {
                Log.Debug($"Opening editable stats window for time \"{time.ToString()}\".");
                timeStatsWindow.CanEdit = true;
            }
            else
            {
                Log.Debug($"Opening non-editable stats window for time \"{time.ToString()}\".");
                timeStatsWindow.CanEdit = false;
            }

            statsTimeViewed = time;
            timeStatsWindow.SetTime(time.Copy());
            timeStatsWindow.RefreshText();

            statsDisplaying = true;
            ToggleEverythingEnableState(false);

            timeStatsWindow.Enabled = true;
            timeStatsWindow.Visible = true;

        }
        /// <summary>
        /// Triggered when the time stats window is closing
        /// </summary>
        /// <param name="arg1"></param>
        private void StatsWindow_Closing(object arg1)
        {
            Log.Debug("Closing stats window.");

            if (arg1 is TimeDisplayWindow)
            {
                TimeDisplayWindow tdWindow = (TimeDisplayWindow)arg1;
                if (tdWindow.ShouldDeleteTime)
                {
                    if (statsTimeViewed.TextColor == Constants.GetColor("TimeTextCurrentColor"))
                    {
                        TimeDeleted(statsTimeViewed);
                    }
                    else
                    {
                        importData.DeleteOldTime(statsTimeViewed);
                        OldTimeDeleted(statsTimeViewed);
                    }
                }
                else if (tdWindow.CanEdit)
                {
                    if (statsTimeViewed.TextColor == Constants.GetColor("TimeTextCurrentColor"))
                    {
                        TimeChanged(tdWindow.GetTime(), statsTimeViewed);
                    }
                    else
                    {
                        importData.ChangeOldTime(tdWindow.GetTime(), statsTimeViewed);
                        OldTimeChanged(tdWindow.GetTime(), statsTimeViewed);
                    }
                }
            }

            statsDisplaying = false;
            ToggleEverythingEnableState(true);

            timeStatsWindow.Enabled = false;
            timeStatsWindow.Visible = false;
        }


        /// <summary>
        /// Triggered when the custom puzzle selection window is opening.
        /// </summary>
        /// <param name="arg1">The button clicked to cause this event.</param>
        /// <param name="arg2">The index number of the sender.</param>
        private void CubeSelectWindow_Opening(object arg1, long arg2)
        {
            Log.Debug("Cube selection window opening.");
            statsDisplaying = true;
            ToggleEverythingEnableState(false);

            cubeSelectWindow.Enabled = true;
            cubeSelectWindow.Visible = true;
        }
        /// <summary>
        /// Triggered when the custom puzzle selection window is closing.
        /// </summary>
        /// <param name="arg1"></param>
        private void CubeSelectWindow_Closing(object arg1)
        {
            Log.Debug("Cube selection window closing.");
            statsDisplaying = false;
            ToggleEverythingEnableState(true);

            cubeSelectWindow.Enabled = false;
            cubeSelectWindow.Visible = false;
        }
        // End popup window stuff


        /// <summary>
        /// Toggles the enable state of everything in the program (for when a stat window is displayed, so that nothing else can be clicked).
        /// </summary>
        /// <param name="value">The truth value to set the enable state to.</param>
        private void ToggleEverythingEnableState(bool value)
        {
            PBToggleButton.Enabled = value;

            foreach (Button b in buttons)
            {
                b.Enabled = value;
            }

            SessionStatistics.Enabled = value;
            AllTimeStatistics.Enabled = value;

            CurrentSession.Enabled = value;
            AllTimes.Enabled = value;

            foreach (AverageDisplayScrollContainer a in AllAverages)
            {
                a.Enabled = value;
            }

            SessionTab.Enabled = value;

            foreach (Tab a in AllTimeTabs)
            {
                a.Enabled = value;
            }

            PBToggleButton.Enabled = value;
            PuzzleSelectButton.Enabled = value;
        }


        // Timey-wimey stuff
        /// <summary>
        /// Triggered when a time is changed somehow (+2, DNF, comments changed, etc.).
        /// </summary>
        /// <param name="newTime">The Time including the new changes.</param>
        /// <param name="oldTime">The Time without the new changes.</param>
        private void TimeChanged(Time newTime, Time oldTime)
        {
            int index = CurrentSessionTimes[currentPuzzle].IndexOf(oldTime);
            if (index != -1)
            {
                CurrentSessionTimes[currentPuzzle][index] = newTime;
                Log.Info($"Time \"{oldTime.ToString()}\" sucessfully updated to \"{newTime.ToString()}\".");
            }
            else
            {
                Log.Warn($"Time \"{oldTime.ToString()}\" not found while attempting to update to \"{newTime.ToString()}\".");
            }

            UpdateDisplays();
        }
        /// <summary>
        /// Triggered when a time from a past session is changed somehow (+2, DNF, comments changed, etc.).
        /// </summary>
        /// <param name="newTime">The Time including the new changes.</param>
        /// <param name="oldTime">The Time without the new changes.</param>
        private void OldTimeChanged(Time newTime, Time oldTime)
        {
            int index = AllTimeTimes[currentPuzzle].IndexOf(oldTime);
            if (index != -1)
            {
                AllTimeTimes[currentPuzzle][index] = newTime;
                Log.Info($"Old time \"{oldTime.ToString()}\" sucessfully updated to \"{newTime.ToString()}\".");
            }
            else
            {
                Log.Warn($"Old time \"{oldTime.ToString()}\" not found while attempting to update to \"{newTime.ToString()}\".");
            }

            UpdateDisplays();
        }
        /// <summary>
        /// Triggered when a time should be deleted
        /// </summary>
        /// <param name="oldTime">The Time to be deleted.</param>
        private void TimeDeleted(Time oldTime)
        {
            if (CurrentSessionTimes[currentPuzzle].Remove(oldTime))
            {
                Log.Info($"Time {oldTime.ToString()} sucessfully deleted.");
            }
            else
            {
                Log.Warn($"Time {oldTime.ToString()} not found while attempting to delete.");
            }

            UpdateDisplays();
        }
        /// <summary>
        /// Triggered when a time from a past session should be deleted
        /// </summary>
        /// <param name="oldTime">The Time to be deleted.</param>
        private void OldTimeDeleted(Time oldTime)
        {
            if (AllTimeTimes[currentPuzzle].Remove(oldTime))
            {
                Log.Info($"Old time {oldTime.ToString()} sucessfully deleted.");
            }
            else
            {
                Log.Warn($"Old time {oldTime.ToString()} not found while attempting to delete.");
            }

            UpdateDisplays();
        }
        /// <summary>
        /// Adds a new Time of the specified number of milliseconds to the current puzzle's session times.
        /// </summary>
        /// <param name="milliseconds">The timespan for the Time to add.</param>
        private void AddTime(int milliseconds)
        {
            Time newTime = new Time(milliseconds, false, false, importData.GetPuzzleName(currentPuzzle), Constants.GetColor("TimeTextCurrentColor"),
                                    null, scramble, "", Path.Combine(importData.GetCurrentFolderPath(currentPuzzle), $"{sessionId[currentPuzzle]}.csv"));
            CurrentSessionTimes[currentPuzzle].Add(newTime);

            UpdateDisplays();
            Log.Info($"Time of {milliseconds} milliseconds added successfully.");
        }

        /// <summary>
        /// Checks if the given time is faster than every other time achieved before the given date.
        /// </summary>
        /// <param name="elapsedMilliseconds">The time to check.</param>
        /// <param name="latestPossibleDate">The cutoff date.</param>
        /// <returns></returns>
        public bool IsPB(long elapsedMilliseconds, DateTime latestPossibleDate)
        {
            if ((AllTimeTimes[currentPuzzle].Count() == 0 || elapsedMilliseconds < AllTimeTimes[currentPuzzle][0].Milliseconds) && (CurrentSessionTimes[currentPuzzle].Count() == 0 || elapsedMilliseconds == importData.GetBest(CurrentSessionTimes[currentPuzzle], latestPossibleDate).Milliseconds))
            {
                return true;
            }
            return false;
        }
        // End timey-wimey stuff


        // Occasional updates to all on-screen times and statistics (not every frame)
        /// <summary>
        /// Updates all visible displays of times and statistics on the screen: the session and all time scrollboxes, and the session and all time statistics.
        /// </summary>
        private void UpdateDisplays()
        {
            importData.UpdateCurrentSessionFile(CurrentSessionTimes[currentPuzzle], sessionId[currentPuzzle], currentPuzzle);

            UpdatePBBackgroundColors(); // Must happen first so that the changes are reflected visually

            UpdateScrollBoxes();
            UpdateSessionStatistics();
            UpdateAllTimeStatistics();
        }
        /// <summary>
        /// Updates the background color of every time, determining whether each is a PB.
        /// </summary>
        public void UpdatePBBackgroundColors()
        {
            Time oldTime;
            for (int i = 0; i < CurrentSessionTimes[currentPuzzle].Count(); i++)
            {
                oldTime = CurrentSessionTimes[currentPuzzle][i];
                oldTime.BackColor = IsPB(oldTime.ComparisonMilliseconds, oldTime.DateRecorded) ? (Color?)Constants.GetColor("TimeBoxPBColor") : null;
            }
            Log.Debug("PB background colors updated.");
        }
        /// <summary>
        /// Updates the scrollboxes containing all times and all averages
        /// </summary>
        private void UpdateScrollBoxes()
        {
            CurrentSession.ChangeData(CurrentSessionTimes[currentPuzzle]);

            // Dangerous, since updates made to these times will not propogate.  This still works because AllTimes does not ever change data.
            AllTimes.ChangeData(DataProcessing.MergeArrays(AllTimeTimes[currentPuzzle].ToArray(), DataProcessing.MergeSort(CurrentSessionTimes[currentPuzzle].ToArray())).ToList());
            AllTimes.UpdateFilter();

            CurrentSessionAverages[currentPuzzle] = importData.UpdateSessionAverages(LastThousandTimes[currentPuzzle], CurrentSessionTimes[currentPuzzle], AllTimeAverages[currentPuzzle]);
            for (int i = 0; i < AllAverages.Count(); i++)
            {
                AllAverages[i].ChangeData(DataProcessing.MergeArrays(AllTimeAverages[currentPuzzle][i].ToArray(), DataProcessing.MergeSort(CurrentSessionAverages[currentPuzzle][i].ToArray())).ToList());
            }
            Log.Debug("All time and all averages scrollboxes updated.");
        }
        /// <summary>
        /// Updates the session statistics box
        /// </summary>
        private void UpdateSessionStatistics()
        {
            List<Average>[] sessionExclusiveAverages = importData.UpdateSessionExclusiveAverages(CurrentSessionTimes[currentPuzzle], AllTimeAverages[currentPuzzle]);

            //Update Session Stats
            if (CurrentSessionTimes[currentPuzzle].Count() > 0)
            {
                Time best = importData.GetBest(CurrentSessionTimes[currentPuzzle]);

                SessionStatistics.EditText(sessionTimeIndices[0], best.DNF ? "DNF" : DataProcessing.ConvertMillisecondsToShortString(best.Milliseconds));
                SessionStatistics.EditText(sessionTimeIndices[1], DataProcessing.ConvertMillisecondsToShortString(new Average(CurrentSessionTimes[currentPuzzle], Color.White, null, false).Milliseconds));
            }
            else
            {
                SessionStatistics.EditText(sessionTimeIndices[0], "0.00");
                SessionStatistics.EditText(sessionTimeIndices[1], "0.00");
            }


            for (int i = 0; i < 7; i++)
            {
                if (CurrentSessionTimes[currentPuzzle].Count() >= maxLengths[i])
                {
                    Average current = new Average(CurrentSessionTimes[currentPuzzle].GetRange(CurrentSessionTimes[currentPuzzle].Count() - maxLengths[i], maxLengths[i]), Color.White, null, i == 1 || i == 2);

                    SessionStatistics.EditText(sessionTimeIndices[2 * i + 2], current.DNF ? "DNF" : DataProcessing.ConvertMillisecondsToShortString(current.Milliseconds));
                    SessionStatistics.EditText(sessionTimeIndices[2 * i + 3], DataProcessing.ConvertMillisecondsToShortString(importData.GetBest(sessionExclusiveAverages[i]).Milliseconds));
                }
                else
                {
                    SessionStatistics.EditText(sessionTimeIndices[2 * i + 2], "0.00");
                    SessionStatistics.EditText(sessionTimeIndices[2 * i + 3], "0.00");
                }
            }

            Log.Debug("Session statistics updated.");
        }
        /// <summary>
        /// Updates the all time statistics box
        /// </summary>
        private void UpdateAllTimeStatistics()
        {
            if (AllTimeTimes[currentPuzzle].Count() > 0 || CurrentSessionTimes[currentPuzzle].Count() > 0)
            {
                Time temp = new Time();
                if (AllTimeTimes[currentPuzzle].Count() > 0)
                {
                    temp = AllTimeTimes[currentPuzzle][0];
                    AllTimeStatistics.EditColor(allTimeIndices[0], Constants.GetColor("TimeTextDefaultColor"));
                }
                if (CurrentSessionTimes[currentPuzzle].Count() > 0)
                {
                    if (AllTimeTimes[currentPuzzle].Count() == 0 || importData.GetBest(CurrentSessionTimes[currentPuzzle]).ComparisonMilliseconds < temp.ComparisonMilliseconds)
                    {
                        temp = importData.GetBest(CurrentSessionTimes[currentPuzzle]);
                        AllTimeStatistics.EditColor(allTimeIndices[0], Constants.GetColor("TimeTextCurrentColor"));
                    }
                }

                AllTimeStatistics.EditText(allTimeIndices[0], temp.DNF ? "DNF" : DataProcessing.ConvertMillisecondsToString(temp.Milliseconds));
            }
            else
            {
                AllTimeStatistics.EditText(allTimeIndices[0], "0.00");
            }

            for (int i = 0; i < 7; i++)
            {
                if (AllTimeAverages[currentPuzzle][i].Count() > 0 || CurrentSessionAverages[currentPuzzle][i].Count() > 0)
                {

                    Average temp = new Average();
                    if (AllTimeAverages[currentPuzzle][i].Count() > 0)
                    {
                        temp = AllTimeAverages[currentPuzzle][i][0];
                    }
                    if (CurrentSessionAverages[currentPuzzle][i].Count() > 0)
                    {
                        if (AllTimeAverages[currentPuzzle][i].Count() == 0 || CurrentSessionAverages[currentPuzzle][i][0].Milliseconds < temp.Milliseconds)
                        {
                            temp = CurrentSessionAverages[currentPuzzle][i][0];
                        }
                    }

                    AllTimeStatistics.EditText(allTimeIndices[i + 1], temp.DNF ? "DNF" : DataProcessing.ConvertMillisecondsToString(temp.Milliseconds));
                    AllTimeStatistics.EditColor(allTimeIndices[i + 1], temp.TextColor);
                }
                else
                {
                    AllTimeStatistics.EditText(allTimeIndices[i + 1], "0.00");
                }
            }

            Log.Debug("All time statistics updated.");
        }
        // End occasional full-display updates


        /// <summary>
        /// Called when one of the cube selection buttons has been clicked, so that currentPuzzle needs to be changed.
        /// </summary>
        /// <param name="sender">The button (or selection window) that was clicked.</param>
        /// <param name="index">The index of the sender.</param>
        private void Button_Click(object sender, long index)
        {
            for (int i = 0; i < buttons.Length; i++)
            {
                buttons[i].Enabled = true;
                buttons[i].IsToggled = false;
            }
            PuzzleSelectButton.IsToggled = false;
            PuzzleSelectButton.SetText("Select Puzzle");

            if (index == cubeSelectWindow.Index)
            {
                PuzzleSelectButton.IsToggled = true;
                SetPuzzleSelectText(importData.GetPuzzleName(currentPuzzle));
                for (int i = 0; i < buttons.Length; i++)
                {
                    if (currentPuzzle == importData.GetPuzzleNumber($"{i + 2}x{i + 2}"))
                    {
                        buttons[i].Enabled = false;
                        buttons[i].IsToggled = true;
                        PuzzleSelectButton.IsToggled = false;
                        PuzzleSelectButton.SetText("Select Puzzle");
                        break;
                    }
                }
            }
            else
            {
                for (int i = 0; i < buttons.Length; i++)
                {
                    if (buttons[i].Index == index)
                    {
                        buttons[i].Enabled = false;
                        currentPuzzle = importData.GetPuzzleNumber($"{i + 2}x{i + 2}");
                        break;
                    }
                }
            }

            PuzzleSelectButton.Location = new Vector2((screenWidth - PuzzleSelectButton.Width) / 2, PuzzleSelectButton.Location.Y);
            scramble = ScrambleGenerator.GenerateScramble(importData.GetPuzzleType(currentPuzzle));
            UpdateDisplays();
        }

        /// <summary>
        /// Displays the puzzle select button text, appending an ellipsis if necessary to keep size down.
        /// </summary>
        /// <param name="text">The text to be displayed.</param>
        private void SetPuzzleSelectText(string text)
        {
            int maxWidth = Math.Min(screenWidth - 2 * (360 + 40), 6 * buttons[0].Width + 5 * 40); // We never want this button to be wider than the others combined

            PuzzleSelectButton.SetText(text);
            text += "...";
            while (PuzzleSelectButton.Width > maxWidth)
            {
                text = text.Remove(text.Length - 4, 1);
                PuzzleSelectButton.SetText(text);
            }
        }


        /// <summary>
        /// UnloadContent will be called once per game and is the place to unload
        /// game-specific content.
        /// </summary>
        protected override void UnloadContent()
        {
            Log.System("Unloading Content (unused).");
        }


        /// <summary>
        /// Allows the game to run logic such as updating the world,
        /// checking for collisions, gathering input, and playing audio.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Update(GameTime gameTime)
        {
            // Pulls new IO states for the program to compare to last tick's
            MouseState newMouseState = Mouse.GetState();
            KeyboardState newKeyboardState = Keyboard.GetState();

            if (!this.IsActive)
            {
                if (oldWindowFocus)
                {
                    oldWindowFocus = false;
                }
                else
                {
                    return;
                }
            }
            else
            {
                oldWindowFocus = true;
            }

            // We either update stats or everything else, but not both at once
            if (statsDisplaying)
            {
                timeStatsWindow.Update(newMouseState, oldMouseState, newKeyboardState, oldKeyboardState, gameTime, oldWindowFocus);
                cubeSelectWindow.Update(newMouseState, oldMouseState, newKeyboardState, oldKeyboardState, gameTime, oldWindowFocus);
            }
            else
            {
                UpdateTimer(newKeyboardState, oldKeyboardState, gameTime);

                PBToggleButton.Update(newMouseState, oldMouseState);

                // Prevents switching categories while timing
                if (!timing)
                {
                    // Update the selection buttons
                    PuzzleSelectButton.Update(newMouseState, oldMouseState);
                    foreach (Button a in buttons)
                    {
                        a.Update(newMouseState, oldMouseState);
                    }
                }

                // Update the containers
                CurrentSession.Update(newMouseState, oldMouseState, gameTime);
                AllTimes.Update(newMouseState, oldMouseState, gameTime);

                foreach (AverageDisplayScrollContainer a in AllAverages)
                {
                    a.Update(newMouseState, oldMouseState, gameTime, a.GetLengthofAverages());
                }

                // Update the tabs
                SessionTab.Update(newMouseState, oldMouseState);

                foreach (Tab a in AllTimeTabs)
                {
                    a.Update(newMouseState, oldMouseState);
                }

                if (currentTabChanged)
                {
                    AdjustAllTimeTabOrder(currentAllTimeTab);
                    currentTabChanged = false;
                }

            }
            oldMouseState = newMouseState;
            oldKeyboardState = newKeyboardState;
            base.Update(gameTime);
        }

        // Stores the time the user has typed.
        string numbersTyped = "";
        // We use this to trigger a time add after drawing the stopped timer, to avoid it looking like the timer ticks an extra hundredth if there is latency adding the time.
        int AddTimeFrameDelay = 0;
        /// <summary>
        /// Performs timer-specfic updates, and allows for typing times into the timer.
        /// </summary>
        /// <param name="newKeyboardState">The current keyboard state.</param>
        /// <param name="oldKeyboardState">The keyboard state from last tick.</param>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        private void UpdateTimer(KeyboardState newKeyboardState, KeyboardState oldKeyboardState, GameTime gameTime)
        {
            // Timer logic
            if (newKeyboardState.IsKeyDown(Keys.Space))
            {
                numbersTyped = "";

                // Finishing timing
                if (timing)
                {
                    timer.Stop();
                    timing = false;
                    resetCooldown = true;

                    AddTimeFrameDelay = 2;
                    //AddTime((int)timer.ElapsedMilliseconds);

                    scramble = ScrambleGenerator.GenerateScramble(importData.GetPuzzleType(currentPuzzle));
                }

                // Ready to time
                if (!resetCooldown)
                {
                    timer.Reset();
                    spaceDownTime += gameTime.ElapsedGameTime.TotalMilliseconds;
                    timerColor = Constants.GetColor("TimerColor");
                    if (spaceDownTime >= 80)
                    {
                        timerColor = Constants.GetColor("TimerStandby");
                    }
                    if (spaceDownTime >= TimerDelay)
                    {
                        timerColor = Constants.GetColor("TimerReady");
                    }
                }
            }

            // Starting timer
            if (!newKeyboardState.IsKeyDown(Keys.Space))
            {
                resetCooldown = false;
                timerColor = Constants.GetColor("TimerColor");

                if (spaceDownTime >= TimerDelay)
                {
                    timing = true;
                    timer.Start();
                }
                spaceDownTime = 0;
            }

            // Typing a time into the timer display
            if (!timing && !newKeyboardState.IsKeyDown(Keys.Space))
            {
                for (int i = (int)Keys.D0; i <= (int)Keys.D9; i++)
                {
                    if (newKeyboardState.IsKeyUp((Keys)i) && oldKeyboardState.IsKeyDown((Keys)i) && numbersTyped.Length < 8)
                    {
                        numbersTyped += (i - (int)Keys.D0).ToString();
                        break;
                    }
                }

                // Saving the typed-in time
                if (oldKeyboardState.IsKeyDown(Keys.Enter) && newKeyboardState.IsKeyUp(Keys.Enter))
                {
                    if (numbersTyped != "")
                    {
                        Log.Info("New time being added by keyboard entry.");
                        AddTime((int)DataProcessing.ConvertStringToMilliseconds(numbersTyped));
                    }
                    numbersTyped = "";
                }
                // Deleting digits from the typed-in time
                else if (oldKeyboardState.IsKeyDown(Keys.Back) && newKeyboardState.IsKeyUp(Keys.Back))
                {
                    if (numbersTyped.Length > 0)
                    {
                        numbersTyped = numbersTyped.Remove(numbersTyped.Length - 1);
                    }
                }
            }
        }

        /// <summary>
        /// Switches between displaying the internal timer's time or the typed-in time as necessary.
        /// </summary>
        /// <returns></returns>
        private string DisplayTimerTime()
        {
            if (numbersTyped != "")
            {
                return DataProcessing.ConvertToDisplayString(numbersTyped);
            }
            else
            {
                return DataProcessing.ConvertMillisecondsToString(timer.ElapsedMilliseconds);
            }
        }



        /// <summary>
        /// This is called when the game should draw itself.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Draw(GameTime gameTime)
        {
            // This ensures that whenever we want to add a time, there is a one-frame delay so that the timer's final time can render without latency
            if (AddTimeFrameDelay == 2) { --AddTimeFrameDelay; }
            else if (AddTimeFrameDelay == 1)
            {
                AddTime((int)timer.ElapsedMilliseconds);
                AddTimeFrameDelay = 0;
            }

            // Sets the background
            GraphicsDevice.Clear(Constants.GetColor("BackgroundColor"));



            string displayTime = DisplayTimerTime();

            spriteBatch.Begin(SpriteSortMode.BackToFront, BlendState.AlphaBlend, SamplerState.LinearWrap);


            if (!statsDisplaying)
            {
                //Draw the timer             
                Vector2 space = gameContent.timerFont.MeasureString(displayTime);
                int maxWidth = screenWidth - 2 * (360 + 20);

                float scale = Math.Min(1.0f, maxWidth / space.X);
                spriteBatch.DrawString(gameContent.timerFont, displayTime, new Vector2((screenWidth - space.X * scale) / 2, (screenHeight - space.Y * scale) / 2), timerColor, 0.0f, Vector2.Zero, scale, SpriteEffects.None, 0.0f);


                //Draw the scramble, one line at a time
                string[] scrambleLines = DataProcessing.DisplayString(scramble, screenWidth - 800, gameContent.scrambleFont);
                gameContent.scrambleFont.LineSpacing = 0;
                bool continueDrawing = true;
                for (int i = 0; i < scrambleLines.Length && continueDrawing; i++)
                {
                    Vector2 lineLength = gameContent.scrambleFont.MeasureString(scrambleLines[i]);

                    // Adds an ellipsis if the window size is too small to display the entire scramble
                    if ((i + 1) * (lineLength.Y) + 40 >= (screenHeight - space.Y * scale) / 2 - 40 && i + 1 != scrambleLines.Length)
                    {
                        scrambleLines[i] = scrambleLines[i].Remove(scrambleLines[i].LastIndexOf(' ')) + "...";
                        continueDrawing = false;
                    }

                    spriteBatch.DrawString(gameContent.scrambleFont, scrambleLines[i], new Vector2((screenWidth - lineLength.X) / 2, i * (lineLength.Y) + 40), Constants.GetColor("TimerColor"));
                }
            }

            //Draw the selection buttons at the bottom
            foreach (Button b in buttons)
            {
                b.Draw();
            }
            PBToggleButton.Draw();
            PuzzleSelectButton.Draw();

            //Draw the containers
            SessionStatistics.Draw();
            AllTimeStatistics.Draw();

            CurrentSession.Draw();
            AllTimes.Draw();

            foreach (AverageDisplayScrollContainer a in AllAverages)
            {
                a.Draw();
            }

            //Draw the tabs
            SessionTab.Draw();

            foreach (Tab a in AllTimeTabs)
            {
                a.Draw();
            }

            // Draw the stats windows
            timeStatsWindow.Draw();
            cubeSelectWindow.Draw();


            spriteBatch.End();
            base.Draw(gameTime);
        }


        /// <summary>
        /// Triggers when the game is exiting.
        /// </summary>
        /// <param name="sender">The object that triggered the exit.</param>
        /// <param name="e">Event parameters.</param>
        private void Game1_Exiting(object sender, EventArgs e)
        {
            loggingTimer.Restart();        
            // Update the cache for easy loading next time
            for (int i = 0; i < numberOfPuzzles; i++)
            {
                importData.SaveCache(TimeCap, AverageCap, i, AllTimeTimes[i], CurrentSessionTimes[i], AllTimeAverages[i], CurrentSessionAverages[i], LastThousandTimes[i]);
            }
            loggingTimer.Stop();

            Log.System($"Rebuilt cache in {loggingTimer.ElapsedMilliseconds} milliseconds.");
            Log.System("Exiting.");
        }
    }
}
