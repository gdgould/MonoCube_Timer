using Microsoft.Xna.Framework;
using System.Collections.Generic;
using System.Text;

namespace MonoCube_Timer
{
    static class Constants
    {
        //IMPORTANT: Floats are only precise to 6 decimal digits

        //Used for the layers of drawing within a sprite: range: +0.000000 to +0.000009 (10 layers)
        //Text is assigned (0 * SpriteLevelDepth) by default, everything else is (1 * SpriteLevelDepth) or more.
        public const float SpriteLevelDepth = 0.000001f;


        //Constant used to bring a toggled control to the front of other controls of the same type.
        public const float ToggleLevelDepth = -0.00001f;


        //User-controlled depth settings.  Best practice is to set depth values as (n * UserLevelDepth)
        public const float UserLevelDepth = 0.0001f;

        public const double ScrollSpeed = 0.5d;

        public const double InitialKeyDelay = 600;  // Measured in milliseconds
        public const double KeyRepeatDelay = 25;    // Measured in milliseconds
        public const int CornerSize = 4;

        public const double CursorFlashTimespan = 500;  // Measured in milliseconds


        // Misc
        public const string AllowedFilenameChars = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ 1234567890-=!@#$%^&()_+;[]{}'";



        public static Dictionary<string, Color> Colors;

        /// <summary>
        /// Loads an XML file containing a color scheme.
        /// </summary>
        /// <param name="xml">The text of the XML file.</param>
        /// <returns></returns>
        public static bool LoadColorScheme(string xml)
        {
            xml = xml.Replace("\r", "").Replace("\n", "");

            if (!RemoveProlog(ref xml))
            {
                Log.Error($"Attempted to load invalid color scheme (Specification has missing or mis-located prolog).  Reverting to defaults.\n\n{xml}.");
                return false;
            }

            XMLElement document = new XMLElement();
            if (ParseElements(xml, "document", out document))
            {
                if (LoadParsedIntoDictionary(document))
                {
                    return true;
                }
            }
            return false;
        }

        /// <summary>
        /// Removes the prolog from an XML file.
        /// </summary>
        /// <param name="xml">The text of the XML file.</param>
        /// <returns></returns>
        private static bool RemoveProlog(ref string xml)
        {
            int prologStart = -1;
            int prologEnd = -1;
            for (int i = 0; i < xml.Length - 1; i++)
            {
                if (xml[i] == '<' && xml[i + 1] == '?')
                {
                    prologStart = i;
                    break;
                }
            }
            for (int i = prologStart + 2; i < xml.Length - 1; i++)
            {
                if (xml[i] == '?' && xml[i + 1] == '>')
                {
                    prologEnd = i + 2;
                    break;
                }
            }

            if (prologStart == -1 || prologEnd == -1)
            {
                return false;
            }
            else
            {
                xml = xml.Remove(prologStart, prologEnd - prologStart);
                return true;
            }
        }

        /// <summary>
        /// Parses the given XML object.  Returns an XMLElement with Parsed == true if the element contains nested sub-elements, and false otherwise.
        /// </summary>
        /// <param name="xml">The XML object to be parsed.</param>
        /// <param name="name">The name of the object being parsed.</param>
        /// <returns></returns>
        private static bool ParseElements(string xml, string name, out XMLElement output)
        {
            output = new XMLElement();

            /* Reading Modes:
             *   0: Indicates looking for a start tag
             *   1: Indicates reading a start tag
             *   2: Indicates reading content (looking for an end tag)
             *   3: Indicates reading an end tag
             *   4: Indicates packing the collected data into an XMLElement
             */

            xml = xml.Trim();
            if (!(xml[0] == '<' && xml[xml.Length - 1] == '>'))
            {
                output = new XMLElement(name, xml);
                return true;
            }

            int state = 0;
            int nesting = 0;
            int position = 0;

            int contentStart = 0;
            int contentEnd = 0;

            List<XMLElement> children = new List<XMLElement>();
            StringBuilder childName = new StringBuilder();
            StringBuilder childNameEnd = new StringBuilder();

            while (position < xml.Length)
            {
                switch (state)
                {
                    case 0:
                        if (xml[position] == '<')
                        {
                            state = 1;
                            childName.Clear();
                        }
                        ++position;
                        break;
                    case 1:
                        if (xml[position] == '>')
                        {
                            state = 2;
                            nesting = 0;
                            contentStart = position + 1;
                        }
                        else
                        {
                            childName.Append(xml[position]);
                        }
                        ++position;
                        break;

                    case 2:
                        if (nesting == 0 && xml[position] == '<' && xml[position + 1] == '/')
                        {
                            state = 3;
                            childNameEnd.Clear();
                            contentEnd = position;
                            ++position;
                        }
                        else if (xml[position] == '<')
                        {
                            if (xml[position + 1] == '/')
                            {
                                --nesting;
                            }
                            else
                            {
                                ++nesting;
                            }
                        }
                        ++position;
                        break;

                    case 3:
                        if (xml[position] == '>')
                        {
                            state = 4;
                            --position;
                        }
                        else
                        {
                            childNameEnd.Append(xml[position]);
                        }
                        ++position;
                        break;

                    case 4:
                        if (childName.ToString() != childNameEnd.ToString())
                        {
                            Log.Error($"Attempted to load invalid color scheme (Specification has start and end tags with unmatched names).  Reverting to defaults.\n\n{xml}.");
                            return false;
                        }
                        else
                        {
                            XMLElement temp = new XMLElement();
                            if (!ParseElements(xml.Substring(contentStart, contentEnd - contentStart), childName.ToString(), out temp))
                            {
                                return false;
                            }

                            children.Add(temp);
                        }
                        state = 0;
                        ++position;
                        break;

                    default:
                        Log.Error($"Impossible state reached while parsing xml color scheme.  Reverting to defaults.  State \"{state}\".\n\n{xml}.");
                        break;

                }
            }

            if (state != 0)
            {
                Log.Error($"Attempted to load invalid color scheme (XML element ended while partway through parsing something).  Reverting to defaults.\n\n{xml}.");
                return false;
            }
            else
            {
                output = new XMLElement(name, children);
                return true;
            }
        }

        /// <summary>
        /// Loads the color specifications from a parsed XML color scheme into the color dictionary.
        /// </summary>
        /// <param name="document">The parsed XML color scheme.</param>
        /// <returns></returns>
        private static bool LoadParsedIntoDictionary(XMLElement document)
        {
            if (document.Parsed && document.PContents.Count == 1 && document.PContents[0].Name == "monoCubeTimer")
            {
                Colors = new Dictionary<string, Color>();
                for (int i = 0; i < document.PContents[0].PContents.Count; i++)
                {
                    Color parsedColor = new Color();
                    if (ColorFromString(document.PContents[0].PContents[i].UContents, out parsedColor))
                    {
                        Colors.Add(document.PContents[0].PContents[i].Name.ToLower(), parsedColor);
                    }
                    else
                    {
                        Log.Warn($"Found invalid color \"{document.PContents[0].PContents[i].UContents}\" for tag \"{document.PContents[0].PContents[i].Name}\" while loading from xml document.  This color will be reverted to the default.");
                    }
                }
                return true;
            }
            else
            {
                return false;
            }
        }

        /// <summary>
        /// Gets the color corresponding to the given tag.
        /// </summary>
        /// <param name="colorTag">The name of the color to be retrieved.</param>
        /// <returns></returns>
        public static Color GetColor(string colorTag)
        {
            if (Colors.ContainsKey(colorTag.ToLower()))
            {
                return Colors[colorTag.ToLower()];
            }
            else
            {
                switch (colorTag.ToLower())
                {
                    case "backgroundcolor": return Color.Black;
                    case "containercolor": return new Color(60, 60, 60, 255);

                    case "tabbackgroundcolor": return new Color(30, 30, 30, 255);
                    case "tabbordercolor": return Color.White;
                    case "tabtextcolor": return Color.White;
                    case "statsboxtextcolor": return Color.White;

                    case "buttonbackcolor": return new Color(100, 100, 100, 255);
                    case "buttontogglecolor": return new Color(130, 130, 130, 255);
                    case "buttontextcolor": return new Color(255, 60, 30, 255);
                    case "buttonbordercolor": return new Color(200, 200, 200, 255);

                    case "timercolor": return Color.White;
                    case "timerstandby": return new Color(242, 239, 80, 255);
                    case "timerready": return new Color(70, 190, 70, 255);

                    case "timetextdefaultcolor": return Color.White;
                    case "timetextcurrentcolor": return new Color(255, 70, 70, 255);
                    case "timetexthybridcolor": return new Color(170, 170, 255, 255);

                    case "numbertextcolor": return new Color(230, 230, 230, 255);
                    case "datetextcolor": return Color.White;
                    case "commenticoncolor": return Color.White;

                    case "plus2textcolor": return new Color(160, 160, 160, 255);
                    case "plus2hovercolor": return new Color(200, 100, 100, 255);
                    case "plus2togglecolor": return new Color(160, 255, 255, 255);

                    case "xtextcolor": return new Color(255, 70, 70, 255);
                    case "xhovercolor": return new Color(120, 70, 70, 255);

                    case "timeboxdefaultcolor": return new Color(90, 90, 90, 255);
                    case "timeboxpbcolor": return new Color(90, 130, 90, 255);
                    case "timeboxdividercolor": return new Color(220, 220, 220, 255);

                    case "textboxtextcolor": return Color.White;

                    case "childbordercolor": return Color.White;
                    case "childbackgroundcolor": return new Color(30, 30, 30, 255);
                    case "childxnormalcolor": return new Color(200, 0, 0, 255);
                    case "childxhovercolor": return new Color(230, 60, 60, 255);

                    default:
                        throw new System.Exception($"Unrecognized colorTag \"{colorTag}\" accessed.");
                }
            }
        }

        /// <summary>
        /// Converts a string representation of a color into its Color equivalent.
        /// </summary>
        /// <param name="s">The string to convert.</param>
        /// <param name="c">The output Color.</param>
        /// <returns>Whether the conversion was successful.</returns>
        private static bool ColorFromString(string s, out Color c)
        {
            c = Color.Black;
            string[] split = s.Split(' ');

            if (split.Length != 4)
            {
                return false;
            }

            int value = 0;

            int r = -1;
            int g = -1;
            int b = -1;
            int a = -1;

            for (int i = 0; i < 4; i++)
            {
                if (!int.TryParse(split[i].Split(':')[1], out value))
                {
                    return false;
                }
                if (value < 0 || value > 255)
                {
                    return false;
                }

                switch (split[i][0])
                {
                    case 'R':
                        r = value;
                        break;
                    case 'G':
                        g = value;
                        break;
                    case 'B':
                        b = value;
                        break;
                    case 'A':
                        a = value;
                        break;
                    default:
                        return false;
                }
            }

            if (r == -1 || g == -1 || b == -1 || a == -1)
            {
                return false;
            }
            else
            {
                c = new Color(r, g, b, a);
                return true;
            }
        }

        private class XMLElement
        {
            public string Name { get; set; }
            public string UContents { get; set; }
            public List<XMLElement> PContents { get; set; }

            // Determines if the element contains raw text or more elements:
            public bool Parsed { get; }

            public XMLElement()
            {
                this.Name = "";
                this.UContents = "";
                this.PContents = new List<XMLElement>();
                this.Parsed = false;
            }
            public XMLElement(string name)
            {
                this.Name = name;
                this.UContents = "";
                this.PContents = new List<XMLElement>();
                this.Parsed = false;
            }
            public XMLElement(string name, string contents)
            {
                this.Name = name;
                this.UContents = contents;
                this.PContents = new List<XMLElement>();
                this.Parsed = false;
            }
            public XMLElement(string name, List<XMLElement> contents)
            {
                this.Name = name;
                this.UContents = "";
                this.PContents = contents;
                this.Parsed = true;
            }

            public override string ToString()
            {
                string returnVal = "";
                returnVal += "<" + this.Name + ">";
                if (Parsed)
                {
                    returnVal += "\n";
                    for (int i = 0; i < this.PContents.Count; i++)
                    {
                        returnVal += PContents[i].ToString() + "\n";
                    }
                }
                else
                {
                    returnVal += UContents;
                }

                returnVal += "</" + this.Name + ">";

                return returnVal;
            }
        }
    }
}