# Debug Signatures Comparer
[![MIT License](https://img.shields.io/apm/l/atomic-design-ui.svg?)](https://github.com/tterb/atomic-design-ui/blob/master/LICENSEs)
[![HitCount](http://hits.dwyl.com/McjMzn/DebugSignaturesComparer.svg)](http://hits.dwyl.com/McjMzn/DebugSignaturesComparer)
[![Github All Releases](https://img.shields.io/github/downloads/McjMzn/DebugSignaturesComparer/total.svg?style=flat)]()

Read files' debug signatures. Check if executable matches the debugging symbols (DLL matches PDB) or if NuGet package matches the symbols package (NUPKG matches SNUPKG). Either programatically, or with CLI or with GUI application!

## Debug Signatures Library
Available as [NuGet package](https://www.nuget.org/packages/Vrasoft.DebugSignatures/), contais utility classes that can be used to check and compare debug signatures of files. Supports Portable Executables, Program Databases and ZIP Archives.

```c#
 // Portable Executables
 var dllReading = DebugSignaturesReader.ReadFromPortableExecutable("library.dll");
 var exeReading = DebugSignaturesReader.ReadFromPortableExecutable("program.exe");
  
 // Program Databases
 var pdbReading = DebugSignaturesReader.ReadFromProgramDatabase("program.pdb");
 
 // Archives
 var zipReadings = DebugSignaturesReader.ReadFromArchive("archive.zip");
 var nupkgReadings = DebugSignaturesReader.ReadFromArchive("Vrasoft.DebugSignatures.1.0.0.nupkg");
 var snupkgReadings = DebugSignaturesReader.ReadFromArchive("Vrasoft.DebugSignatures.1.0.0.snupkg");
```

## Debug Signatures Comparer CLI
Compare debug signatures directly from command line interface.

## Debug Signatures Comparer GUI
Simple Drag-and-Drop application allowing to easily check if files have matching debug signature.
