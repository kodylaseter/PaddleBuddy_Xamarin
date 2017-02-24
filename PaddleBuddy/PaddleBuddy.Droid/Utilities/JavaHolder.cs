namespace PaddleBuddy.Droid.Utilities
{
    /// <summary>
    /// credit to cheesebaron
    /// https://gist.github.com/Cheesebaron/9876783
    /// </summary>
    public class JavaHolder : Java.Lang.Object
    {
        public readonly object Instance;

        public JavaHolder(object instance)
        {
            Instance = instance;
        }
    }
}