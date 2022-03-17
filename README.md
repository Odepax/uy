Uy
====

![NuGet](https://img.shields.io/nuget/v/Uy?label=NuGet&logo=nuget)

Installation
----

Install [Uy](https://www.nuget.org/packages/Uy/) from NuGet.

Overview
----

This is a work in progress, which one shall avoid using in production, if not at all.

Source Code Structure
----

<!-- CCF5F0F0-3BED-4746-9D80-F5495579332E -->

Unlike what is the default in the C# world, directories in the source code of the main assembly are not supposed to provide sub-namespaces. The assembly is flattened into a single `Uy` namespace, while preserving a nice organization in the solution explorer IDE's side bar.

This decision is enforced by `Uy.Tests.SingleNamespaceTests.SingleNamespace()`.
