using System.Runtime.CompilerServices;

namespace Kvitretest
{
    internal class MyDebug
    {
        public static void WriteLine(string message = "", [CallerMemberName] string? callerFunc = null, [CallerFilePath] string? callerFile = null, [CallerLineNumber] int callerLineNumber = 0)
        {
#if DEBUG
            string strCallerLineNumber = callerLineNumber.ToString().PadLeft(4, '_');
            string? strCallerFile = System.IO.Path.GetFileName(callerFile);

            if (message is null)
            {
                message = "";
            }
            else
            {
                // Json could contain { and } which gives unexpected stuff for the format string.
                message = message.Replace("{", "{{");
                message = message.Replace("}", "}}");
            }
            // Environment.NewLine
            //Console.WriteLine("_L_{0:D4}_: (" + callerFile + " in " + callerFunc + ") || " + message.ToString(), callerLineNumber);
            Console.WriteLine("_L_{0}_: ({1} @ {2}) || " + message,
                strCallerLineNumber, strCallerFile, callerFunc);
#endif
        }

        public static void WriteLine(int? message = null, [CallerMemberName] string? callerFunc = null, [CallerFilePath] string? callerFile = null, [CallerLineNumber] int callerLineNumber = 0)
        {
#if DEBUG
            string strCallerLineNumber = callerLineNumber.ToString().PadLeft(4, '_');
            string? strCallerFile = System.IO.Path.GetFileName(callerFile) ?? "";

            // Environment.NewLine
            //Console.WriteLine("_L_{0:D4}_: (" + callerFile + " in " + callerFunc + ") || " + message.ToString(), callerLineNumber);
            Console.WriteLine("_L_{0}_: ({1} @ {2}) || " + message.ToString(),
                strCallerLineNumber, strCallerFile, callerFunc);
#endif
        }
    }
}
