#region Usings

using System.Reflection;

#endregion

[assembly: AssemblyCompany("FSSCP")]
[assembly: AssemblyProduct("FSOLauncher")]
[assembly: AssemblyCopyright("Copyright © FreeSpace Source Code Project 2014")]
[assembly: AssemblyTrademark("")]
[assembly: AssemblyCulture("")]

#if DEBUG

[assembly: AssemblyConfiguration("Debug")]
#else
[assembly: AssemblyConfiguration("Release")]
#endif

[assembly: AssemblyVersion("0.3.0.0")]