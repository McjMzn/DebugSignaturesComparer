using System;
using System.Linq;

namespace Signet.ComparerCli
{
    class Program
    {
        static void Main(string[] args)
        {
            PrintHeader();
            if (args.Count() == 0 || args.Any(a => a == "h" || a == "help" || a == "-h" || a == "--help"))
            {
                PrintHelp();
                return;
            }

            var debugSignaturesComparer = new DebugSignaturesComparer();
            debugSignaturesComparer.AddItems(args.Distinct());

            PrintReadings(debugSignaturesComparer);
        }

        static void PrintReadings(DebugSignaturesComparer comparer)
        {
            Console.ForegroundColor = ConsoleColor.Yellow;
            comparer.FailedReadings.ForEach(failedReading =>
            {
                Console.WriteLine($"Failed to read signature from file: {failedReading.File}.{Environment.NewLine}Error message: {failedReading.Error}{Environment.NewLine}");
            });
            Console.ResetColor();

            comparer.ReadingsBySignature.Keys.ToList().ForEach(signature =>
            {
                var files = comparer.ReadingsBySignature[signature];

                Console.WriteLine($"Signature: {signature}");
                Console.WriteLine("Files:");

                Console.ForegroundColor = files.Count > 1 ? ConsoleColor.Green : ConsoleColor.Gray;
                files.ForEach(file => Console.WriteLine($"  {file.File}"));
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
                var margin = new string(' ', (windowWidth - text.Length - 1) / 2);
                return $"{margin}{text}{margin}{Environment.NewLine}";
            };

            var border = $"{new string('_', windowWidth)}{Environment.NewLine}";

            var signetVersion = typeof(DebugSignaturesReader).Assembly.GetName().Version;
            var header = $"{border}{FormatLine("SigNET - Debug Signatures Comparer CLI")}{FormatLine(signetVersion.ToString())}{FormatLine("https://github.com/McjMzn/DebugSignaturesComparer")}{border}";
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
