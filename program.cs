// @ Windows 10, .NET Core 3.1.2, Visual Studio 2019 (16.4.5)

using System;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.Loader;
using Microsoft.CodeAnalysis.CSharp.Scripting;
using Microsoft.CodeAnalysis.Scripting;

static class Program
{
    static readonly ScriptOptions Options =
        ScriptOptions.Default.WithReferences(typeof(string).GetTypeInfo().Assembly, typeof(Console).GetTypeInfo().Assembly);

    static void Main()
    {
        if (!Debugger.IsAttached) { Console.WriteLine("Please run me under VS debugger."); Console.ReadLine(); return; }
        for (int C = 0; ; C++)
        {
            Clash(C);
        }
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    static void Clash(int num)
    {
        const string code = @"public static void M(int num) => System.Console.WriteLine(num);";
        var eval = CSharpScript.Create(code, Options);
        var compilation = eval.GetCompilation();
        using (var ms = new MemoryStream())
        {
            compilation.Emit(ms);
            ms.Seek(0, SeekOrigin.Begin);
            var assemblyLoadContext = new AssemblyLoadContext(null, true);
            var assembly = assemblyLoadContext.LoadFromStream(ms);
            assembly.GetType("Submission#0").GetMethod("M").Invoke(null, new object[] { num });
            assemblyLoadContext.Unload();
        }
    }
}
