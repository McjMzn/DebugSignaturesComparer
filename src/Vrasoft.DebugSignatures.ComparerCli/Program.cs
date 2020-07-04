using System;
using System.Linq;
using Vrasoft.DebugSignatures;

namespace Vrasoft.DebugSignatures.ComparerCli
{
    class Program
    {
        static void Main(string[] args)
        {

            PrintHeader();

            var debugSignaturesComparer = new DebugSignaturesComparer();
            debugSignaturesComparer.ProcessingError += (sender, message) =>
            {
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.WriteLine(message);
                Console.ResetColor();
            };

            debugSignaturesComparer.AddFiles(args.Distinct());

            
            if (debugSignaturesComparer.Readings.Count == 0)
            {
                PrintHelp();
            }
            else
            {
                PrintReadings(debugSignaturesComparer);
            }
        }

        static void PrintReadings(DebugSignaturesComparer comparer)
        {
            comparer.ReadingsBySignature.ToList().ForEach(group =>
            {
                Console.WriteLine($"Signature: {group.Key}");
                Console.WriteLine("Files:");

                Console.ForegroundColor = group.Count() > 1 ? ConsoleColor.Green : ConsoleColor.Gray;
                group.ToList().ForEach(file => Console.WriteLine($"  {file.File}"));
                Console.ResetColor();

                Console.WriteLine();
            });
        }

        static void PrintHeader()
        {
            int windowWidth;
            try { windowWidth = Console.WindowWidth; } catch { windowWidth = 80; }
            
            Func<string, string> FormatLine = text =>
            {
                var margin = new String(' ', (windowWidth - text.Length - 1) / 2);
                return $"{margin}{text}{margin}{Environment.NewLine}";
            };

            var border = $"{new String('_', windowWidth)}{Environment.NewLine}";

            var header = $"{border}{FormatLine("Debug Signatures Comparer CLI")}{ FormatLine("1.0.3")}{ FormatLine("https://github.com/McjMzn/DebugSignaturesComparer")}{ border}";
            Console.WriteLine(header);
        }

        static void PrintHelp()
        {
            Console.WriteLine("To check debug signatures please enter any number of command line arguments being paths to either:");
            Console.WriteLine("  - DLL file");
            Console.WriteLine("  - EXE file");
            Console.WriteLine("  - PDB file");
            Console.WriteLine("  - NuGet package");
            Console.WriteLine("  - ZIP archive");
            Console.WriteLine("  - Directory");
        }
    }
}
