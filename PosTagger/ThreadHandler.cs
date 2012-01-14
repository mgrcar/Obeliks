using System;
using System.Threading;
using System.Windows.Forms;
using Latino;

public static class ThreadHandler
{
    public class AbortedByUserException : Exception
    { 
        public AbortedByUserException() : base("Aborted by user.")
        {
        }
    }

    private static bool mAborted
        = false;

    public static void Reset()
    {
        mAborted = false;
    }

    public static void Abort(Thread thread, int timeoutMs)
    {
        // try to abort nicely
        mAborted = true;
        DateTime start = DateTime.Now;
        while (thread.IsAlive && (DateTime.Now - start).TotalMilliseconds < timeoutMs)
        {
            Application.DoEvents();
        }
        if (thread.IsAlive)
        {
            // we should not get here...
            thread.Abort();
        }
    }

    public static void AbortCheckpoint()
    {
        if (mAborted)
        {
            throw new AbortedByUserException();
        }
    }
}