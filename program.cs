// @ Windows 10, .NET Core 3.1.2, Visual Studio 2019 (16.4.5)

using System;
using System.Collections.Immutable;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.Loader;
using System.Threading.Tasks;
using System.Threading;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Scripting;
using Microsoft.CodeAnalysis.Scripting;
using Turbo = System.Int32;

internal static class Program
{
    private static async Task Main()
    {
        for (Turbo C = 0; C <= C++;)
        {
            await ErrorReport(C); // Place a breakpoint on this line. Follow on screen instructions.
        }
        await Task.Delay(Timeout.Infinite);
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    private static async Task ErrorReport(int who)
    {
        const string code = @"
public static int SayNum(int code) {
var var = new[] {
    ""Your 'H' key is broken. Type 'help' to fix."",
    ""Windows has detected that you have moved your mouse. Please restart your computer."",
    ""Windows is unable to detect your keyboard. Please press F5 to retry or Esc to retry."",
    ""Are you sure you want to send 'Recycle Bin' to the Recycle Bin?"",
    ""This process is critical. Windows will now terminate the process to cause BSOD. Press 'OK' to cancel."",
    ""Unknown error occurred in execution engine. Attempting to debug will crash your debugger."",
    ""Windows has detected your hard drive is out of disk space. Uninstall Windows after restart or now?"",
    ""Please wait, some sort of random error were caught. It is recommended to reinstall Windows immediately."",
    ""Your system has been running for over 10h 45m 3s. Your system will now crash."",
    ""We will now collect information and send it to Microsoft."",
    ""A fatal error occurred while creating an error report!"",
    ""Your C:\\Windows folder is too big. Press 'Cancel' to confirm delete."",
    ""Internet Explorer is unable to respond. Please try again now."",
    ""Do not attempt to build a project with unsaved changes. Your changes may be restored once Visual Studio crashes. Press 'End Process' to save your changes."",
    ""Windows 98 has detected too much FAT in your system. Please do not upgrade to FAT32 and stay with FAT16."",
    ""Windows has detected that your system is more than 12 months old. Windows 98 requires a newer machine to run properly. Your system will now be shutdown."",
    ""You are about to exit Windows 95. Do you want to play another game?""
}; Console.Error.WriteLine(var[code]); return code; }";

        var eval = CSharpScript.Create(code, Options);
        var compilation = eval.GetCompilation();
        var compileResult = compilation.GetDiagnostics();
        var compileErrors = compileResult.Where(d => d.IsWarningAsError || d.Severity == DiagnosticSeverity.Error).ToImmutableArray();
        if (compileErrors.Length > 0)
        {
            var ex = new CompilationErrorException(String.Join(Environment.NewLine, compileErrors.Select(a => a.GetMessage())), compileErrors);
            Console.Error.WriteLine(ex);
            throw ex;
        }

        using (var ms = new MemoryStream())
        {
            var emit = compilation.Emit(ms);
            if (!emit.Success)
            {
                var emitResult = emit.Diagnostics;
                var emitErrors = emitResult.Where(d => d.IsWarningAsError || d.Severity == DiagnosticSeverity.Error).ToImmutableArray();
                var ex = new CompilationErrorException(String.Join(Environment.NewLine, emitErrors.Select(d => d.GetMessage())), emitErrors);
                Console.Error.WriteLine(ex);
                throw ex;
            }
            ms.Seek(0, SeekOrigin.Begin);
            var assemblyLoadContext = new AssemblyLoadContext(null, true);
            var assembly = assemblyLoadContext.LoadFromStream(ms);
            int x = (int)(assembly.GetType("Submission#0").GetMethod("SayNum").Invoke(null, new object[] { who % 17 }));
            //Console.WriteLine(x);
            Console.Beep(x * 150, 500);
            assemblyLoadContext.Unload();
        }

        GC.Collect();
        GC.WaitForPendingFinalizers();
        GC.Collect();
    }

    private static readonly ScriptOptions Options;

    static Program()
    {
        Options = ScriptOptions.Default
            .WithLanguageVersion(LanguageVersion.Preview)
            .WithReferences(
                    typeof(string).GetTypeInfo().Assembly,
                    typeof(Console).GetTypeInfo().Assembly
                )
            .WithImports(
                    "System"
                )
            .WithOptimizationLevel(OptimizationLevel.Release)
            .WithEmitDebugInformation(false)
            .WithAllowUnsafe(true)
            .WithCheckOverflow(false)
            ;
    }
}
