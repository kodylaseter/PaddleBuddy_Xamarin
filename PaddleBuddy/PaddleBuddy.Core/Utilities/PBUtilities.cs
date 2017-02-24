using System.Threading;

namespace PaddleBuddy.Core.Utilities
{
    public class PBUtilities
    {
        private static int newObjectId = int.MinValue;

        public static int GetNextId()
        {
            return Interlocked.Increment(ref newObjectId);
        }
    }
}
