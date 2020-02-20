// @ Windows 10, .NET Core 3.1.2, Visual Studio 2019 (16.4.5)

using System;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Runtime.Loader;
using Microsoft.CodeAnalysis.CSharp.Scripting;
using Microsoft.CodeAnalysis.Scripting;

static class Program
{
    static readonly ScriptOptions Options =
        ScriptOptions.Default.WithReferences(typeof(string).GetTypeInfo().Assembly, typeof(Console).GetTypeInfo().Assembly);

    static void Main()
    {
        if (Debugger.IsAttached)
        {
            Console.Title = "I will crash your debugger!";
            Console.Write("Press Enter to kill the debugger (after about 30 loop iterations). ");
        }
        else
        {
            Console.Title = "I am a working program indeed.";
            Console.Write("Please run me under VS debugger. Press Enter if you still wish to proceed. ");
        }
        Console.ReadLine();

        for (var C = 0; ; C++)
        {
            Clash(C);
        }
    }

    //[MethodImpl(MethodImplOptions.NoInlining)]
    static void Clash(int num)
    {
        const string code = @"public static void M(int num) => System.Console.WriteLine(num);";
        var eval = CSharpScript.Create(code, Options);
        var compilation = eval.GetCompilation();

        // To make it more bizarre...
        // If you turn this using-statement into using-declaration variable, it will throw "bad IL format".
        using (var ms = new MemoryStream())
        //using var ms = new MemoryStream();
        {
            compilation.Emit(ms);
            ms.Seek(0, SeekOrigin.Begin);
            var assemblyLoadContext = new AssemblyLoadContext(null, isCollectible: true); // Note: Changing 'true' to 'false' seems to *fix* the crashing issue (but it is unwanted, because memory is blown up then).
            var assembly = assemblyLoadContext.LoadFromStream(ms);
            assembly.GetType("Submission#0").GetMethod("M").Invoke(null, new object[] { num });
            assemblyLoadContext.Unload();
        }

        // Uncomment these lines of code to make debugger crash faster.
        //GC.Collect();
        //GC.WaitForPendingFinalizers();
        //GC.Collect();
    }
}
