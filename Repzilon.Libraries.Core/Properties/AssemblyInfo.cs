//
//  AssemblyInfo.cs
//
//  Author:
//       René Rhéaume <repzilon@users.noreply.github.com>
//
// Copyright (C) 2023 René Rhéaume
//
// This Source Code Form is subject to the terms of the 
// Mozilla Public License, v. 2.0. If a copy of the MPL was 
// not distributed with this file, You can obtain one at 
// https://mozilla.org/MPL/2.0/.
//
using System;
using System.Reflection;
using System.Runtime.InteropServices;

#region General Information
// Information about this assembly is defined by the following attributes. 
// Change them to the values specific to your project.
#endregion
[assembly: AssemblyTitle("Repzilon.Libraries.Core")]
[assembly: AssemblyDescription("")]
#if DEBUG
[assembly: AssemblyConfiguration("Debug")]
#else
[assembly: AssemblyConfiguration("Release")]
#endif
[assembly: AssemblyCompany("(none)")]
[assembly: AssemblyProduct("")]
[assembly: AssemblyCopyright("(C) 2022-2023 René Rhéaume <repzilon@users.noreply.github.com>. Released under the MPL2.")]
[assembly: AssemblyTrademark("")]
[assembly: AssemblyCulture("")]

#region Interoperability Attributes
#endregion
[assembly: CLSCompliant(true)]
[assembly: ComVisible(false)]

#region Assembly Versioning
// The assembly version has the format "{Major}.{Minor}.{Build}.{Revision}".
// The form "{Major}.{Minor}.*" will automatically update the build and revision,
// and "{Major}.{Minor}.{Build}.*" will update just the revision.
#endregion
[assembly: AssemblyVersion("1.0.0.0")]

#region Deprecated way for Assembly Signing
// The following attributes are used to specify the signing key for the assembly, 
// if desired. See the Mono documentation for more information about signing.
//[assembly: AssemblyDelaySign(false)]
//[assembly: AssemblyKeyFile("")]
#endregion
