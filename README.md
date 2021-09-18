# SigNET - Debug Signatures Comparer
[![MIT License](https://img.shields.io/apm/l/atomic-design-ui.svg?)](https://github.com/tterb/atomic-design-ui/blob/master/LICENSEs)
![Test, Build, Publish](https://github.com/McjMzn/DebugSignaturesComparer/workflows/test-build-publish/badge.svg)

Read files' debug signatures. Check if executable matches the debugging symbols (DLL matches PDB) or if NuGet package matches the symbols package (NUPKG matches SNUPKG). Either programatically, or with CLI or with GUI application!

## Debug Signatures Library
![Platform](https://img.shields.io/badge/.NET%20Standard-2.0-blue)
[![Nuget](https://img.shields.io/nuget/v/SigNET)](https://www.nuget.org/packages/SigNET/)

Available as [NuGet package](https://www.nuget.org/packages/SigNET/), contais utility classes that can be used to check and compare debug signatures of files. Supports Portable Executables, Program Databases and ZIP Archives.

#### Reading Debug Signature
```c#
var debugSignaturesReader = new DebugsignaturesReader();
List<DebugSignatureReading> readings;

 // Portable Executables
 readings = debugSignaturesReader.Read("library.dll");
 readings = debugSignaturesReader.Read("program.exe");
   
 // Program Databases
 readings = debugSignaturesReader.Read("program.pdb");
 
 // Archives
 readings = debugSignaturesReader.Read("archive.zip");
 readings = debugSignaturesReader.Read("nuget.package.1.0.0.nupkg");
 readings = debugSignaturesReader.Read("symbols.package.1.0.0.snupkg");
```
#### Comparing Debug Signatures
```c#
// With bare DebugSignaturesReader
var debugSignaturesReader = new DebugsignaturesReader();
var dllSignature = debugSignaturesReader.Read("library.dll").Single().DebugSignature;
var pdbSignature = debugSignaturesReader.Read("library.pdb").Single().DebugSignature;
var matching = dllSignature == pdbSignature;

// With DebugSignaturesComparer's static method
var matching = DebugSignaturesComparer.AreMatching("library.dll", "library.pdb");

// With instance of DebugSignaturesComparer
var comparer = new DebugSignaturesComparer();
comparer.AddItem("library.dll");
comparer.AddItem("library.pdb");
var matching = comparer.ReadingsMatched;
```

## Debug Signatures Comparer CLI
![Platform](https://img.shields.io/badge/.NET-5.0-blue)
![Platform](https://img.shields.io/badge/OS-Windows%20|%20Linux%20|%20MacOS-lightgrey)

Compare debug signatures directly from command line interface.
![cli-image-1](https://i.imgur.com/K6NlOxY.png "SigNET - Debug Signatures Comparer CLI")

## Debug Signatures Comparer GUI
![Platform](https://img.shields.io/badge/.NET-5.0%20WPF-blue)
![Platform](https://img.shields.io/badge/OS-Windows-lightgrey)

Simple Drag-and-Drop application allowing to easily check if files have matching debug signature.
![gui-image-1](https://i.imgur.com/XKZ2b7B.png "SigNET - Debug Signatures Comparer GUI #1")
![gui-image-2](https://i.imgur.com/fUw0JyA.png "SigNET - Debug Signatures Comparer GUI #2")
