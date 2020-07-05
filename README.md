# Debug Signatures Comparer
[![MIT License](https://img.shields.io/apm/l/atomic-design-ui.svg?)](https://github.com/tterb/atomic-design-ui/blob/master/LICENSEs)
![publish-build](https://github.com/McjMzn/DebugSignaturesComparer/workflows/publish-build/badge.svg)

Read files' debug signatures. Check if executable matches the debugging symbols (DLL matches PDB) or if NuGet package matches the symbols package (NUPKG matches SNUPKG). Either programatically, or with CLI or with GUI application!

## Debug Signatures Library
![Platform](https://img.shields.io/badge/.NET%20Standard-2.0-blue)
[![Nuget](https://img.shields.io/nuget/v/Vrasoft.DebugSignatures)](https://www.nuget.org/packages/Vrasoft.DebugSignatures/)

Available as [NuGet package](https://www.nuget.org/packages/Vrasoft.DebugSignatures/), contais utility classes that can be used to check and compare debug signatures of files. Supports Portable Executables, Program Databases and ZIP Archives.

#### Reading Debug Signature
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
#### Comparing Debug Signatures
```c#
// With bare DebugSignaturesReader
var dllSignature = DebugSignaturesReader.ReadFromPortableExecutable("library.dll").DebugSignature;
var pdbSignature = DebugSignaturesReader.ReadFromPortableExecutable("library.pdb").DebugSignature;
var matching = dllSignature == pdbSignature;

// With DebugSignaturesComparer's static method
var matching = DebugSignaturesComparer.AreMatching("library.dll", "library.pdb");

// With instance of DebugSignaturesComparer
var comparer = new DebugSignaturesComparer();
comparer.AddFile("library.dll");
comparer.AddFile("library.pdb");
var matching = comparer.AllMatching;
```

## Debug Signatures Comparer CLI
![Platform](https://img.shields.io/badge/.NET%20Core-3.0-blue)
![Platform](https://img.shields.io/badge/OS-Windows%20|%20Linux%20|%20MacOS-lightgrey)

Compare debug signatures directly from command line interface.
![cli-image-1](https://i.imgur.com/K6NlOxY.png "Debug Signatures Comparer CLI")

## Debug Signatures Comparer GUI
![Platform](https://img.shields.io/badge/.NET%20Core-3.0%20WPF-blue)
![Platform](https://img.shields.io/badge/OS-Windows-lightgrey)

Simple Drag-and-Drop application allowing to easily check if files have matching debug signature.
![cli-image-1](https://i.imgur.com/3CxTfea.png "Debug Signatures Comparer GUI")
