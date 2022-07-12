using System;

namespace MonoCube_Timer
{
    static class ScrambleGenerator
    {
        public static string GenerateScramble(WCAPuzzle cubeSize)
        {
            switch (cubeSize)
            {
                case WCAPuzzle.Null:
                    return "";

                case WCAPuzzle.Two:
                    return Generate3x3Scramble(12);

                case WCAPuzzle.Three:
                    return Generate3x3Scramble(25);

                case WCAPuzzle.Four:
                    return Generate5x5Scramble(40);

                case WCAPuzzle.Five:
                    return Generate5x5Scramble(60);

                case WCAPuzzle.Six:
                    return Generate7x7Scramble(80);

                case WCAPuzzle.Seven:
                    return Generate7x7Scramble(100);

                default:
                    return "";
            }
        }
        public static string GenerateScramble(WCAPuzzle cubeSize, int length)
        {
            switch (cubeSize)
            {
                case WCAPuzzle.Null:
                    return "";

                case WCAPuzzle.Two:
                    return Generate3x3Scramble(length);

                case WCAPuzzle.Three:
                    return Generate3x3Scramble(length);

                case WCAPuzzle.Four:
                    return Generate5x5Scramble(length);

                case WCAPuzzle.Five:
                    return Generate5x5Scramble(length);

                case WCAPuzzle.Six:
                    return Generate7x7Scramble(length);

                case WCAPuzzle.Seven:
                    return Generate7x7Scramble(length);

                default:
                    return "";
            }
        }

        private static string Generate3x3Scramble(int length)
        {
            string[] moveset = { "R", "L", "U", "D", "F", "B" };
            string[] oppositeMoveset = { "L", "R", "D", "U", "B", "F" };
            string[] modifiers = { "", "'", "2" };

            string scramble = "";

            int previous2Move = -1;
            int previousMove = -1;

            Random r = new Random();
            int rand = 0;

            for (int i = 0; i < length; i++)
            {
                do
                {
                    rand = r.Next(0, 6);
                } while (rand == previousMove || (rand == previous2Move && moveset[previousMove] == oppositeMoveset[rand]));
                previous2Move = previousMove;
                previousMove = rand;

                scramble += moveset[rand] + modifiers[r.Next(0, 3)] + " ";
            }

            return scramble.Trim(' ');
        }

        private static string Generate5x5Scramble(int length)
        {
            string[] moveset = { "R", "L", "U", "D", "F", "B", "Rw", "Lw", "Uw", "Dw", "Fw", "Bw" };
            string[] oppositeMoveset = { "L", "R", "D", "U", "B", "F", "Lw", "Rw", "Dw", "Uw", "Bw", "Fw" };
            string[] modifiers = { "", "'", "2" };

            string scramble = "";

            int previous2Move = -1;
            int previousMove = -1;

            Random r = new Random();
            int rand = 0;

            for (int i = 0; i < length; i++)
            {
                do
                {
                    rand = r.Next(0, 12);
                } while (rand % 6 == previousMove % 6 || (rand % 6 == previous2Move % 6 && moveset[previousMove % 6] == oppositeMoveset[rand % 6]));
                previous2Move = previousMove;
                previousMove = rand;

                scramble += moveset[rand] + modifiers[r.Next(0, 3)] + " ";
            }

            return scramble.Trim(' ');
        }

        private static string Generate7x7Scramble(int length)
        {
            string[] moveset = { "R", "L", "U", "D", "F", "B", "Rw", "Lw", "Uw", "Dw", "Fw", "Bw", "3Rw", "3Lw", "3Uw", "3Dw", "3Fw", "3Bw" };
            string[] oppositeMoveset = { "L", "R", "D", "U", "B", "F", "Lw", "Rw", "Dw", "Uw", "Bw", "Fw", "3Lw", "3Rw", "3Dw", "3Uw", "3Bw", "3Fw" };
            string[] modifiers = { "", "'", "2" };

            string scramble = "";

            int previous2Move = -1;
            int previousMove = -1;

            Random r = new Random();
            int rand = 0;

            for (int i = 0; i < length; i++)
            {
                do
                {
                    rand = r.Next(0, 18);
                } while (rand % 6 == previousMove % 6 || (rand % 6 == previous2Move % 6 && moveset[previousMove % 6] == oppositeMoveset[rand % 6]));
                previous2Move = previousMove;
                previousMove = rand;

                scramble += moveset[rand] + modifiers[r.Next(0, 3)] + " ";
            }

            return scramble.Trim(' ');
        }
    }
}