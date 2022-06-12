using log4net;

namespace ExpenseWatcher
{
    public static class Logging
    {
        public static ILog Log { get { return LogManager.GetLogger("ExpenseWatcher"); } }
    }
}
