using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System;

// General Information about an assembly is controlled through the following 
// set of attributes. Change these attribute values to modify the information
// associated with an assembly.
[assembly: AssemblyTitle( "Fluggo Base Library" )]
[assembly: AssemblyDescription( "Covers basic exceptions and utilities and the concept of resources and resource providers." )]

#if DEBUG
[assembly: AssemblyConfiguration( "DEBUG" )]
#else
[assembly: AssemblyConfiguration( "RELEASE" )]
#endif

[assembly: AssemblyCompany( "Fluggo Productions" )]
[assembly: AssemblyProduct( "Fluggo" )]
[assembly: AssemblyCopyright( "Copyright © 2006 Brian J. Crowell. All rights reserved." )]
[assembly: AssemblyTrademark( "" )]
[assembly: AssemblyCulture( "" )]
[assembly: CLSCompliant(true)]

// Setting ComVisible to false makes the types in this assembly not visible 
// to COM components.  If you need to access a type in this assembly from 
// COM, set the ComVisible attribute to true on that type.
[assembly: ComVisible( false )]

// The following GUID is for the ID of the typelib if this project is exposed to COM
[assembly: Guid( "508ee5c5-2c11-4bfe-ac11-680df5f71a7f" )]

// Version information for an assembly consists of the following four values:
//
//      Major Version
//      Minor Version 
//      Build Number
//      Revision
//
// You can specify all the values or you can default the Revision and Build Numbers 
// by using the '*' as shown below:
[assembly: AssemblyVersion( "1.0.0.0" )]
[assembly: AssemblyFileVersion( "1.0.0.0" )]
