using System.Reflection;

namespace System.Diagnostics;

#if DEBUG
public static class DiagnosticsExtensions
{
    public static (MethodBase Method, string ClassName) CallingMethodInfo(this object @this)
    {
        int frameCount = 2;

        MethodBase methodInfo;
        string className;

        do
        {
            frameCount++;

            // walk back up the stack to the calling method... this method > request > caller
            StackFrame? stackFrame = new StackTrace().GetFrame(frameCount);
        
            // get the calling method...
            methodInfo = stackFrame?.GetMethod()!;

            // lastly the class the method belongs to...
            className = methodInfo!.ReflectedType!.Name;

            // skipping compiler-generated delegates and anonymous methods...
        } while (className.StartsWith("<>c") || methodInfo.Name.Contains("<.cctor>"));

        // we're done!
        return new (methodInfo, className);
    }
}
#endif
