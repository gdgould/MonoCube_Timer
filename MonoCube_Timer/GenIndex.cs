namespace MonoCube_Timer
{
    abstract class GenIndex
    {
        private static long counter = 0;
        public static long getNewIndex()
        {
            return counter++;
        }
    }
}
