using log4net;

namespace ExpanseWatcher
{
    public static class Logging
    {
        public static ILog Log { get { return LogManager.GetLogger("ExpenseWatcher"); } }
    }
}
