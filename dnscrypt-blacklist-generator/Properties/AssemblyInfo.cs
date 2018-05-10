using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

// General Information about an assembly is controlled through the following
// set of attributes. Change these attribute values to modify the information
// associated with an assembly.
[assembly: AssemblyTitle("dnscrypt-blacklist-generator")]
[assembly: AssemblyDescription("Utility to download and generate a blacklist (and/or whitelist) of domains for dnscrypt-proxy from curated lists of malicious or advertising domains.")]
[assembly: AssemblyConfiguration("")]
[assembly: AssemblyCompany("williampuckering.com")]
[assembly: AssemblyProduct("dnscrypt-blacklist-generator")]
[assembly: AssemblyCopyright("Copyright © 2018 William Puckering")]
[assembly: AssemblyTrademark("")]
[assembly: AssemblyCulture("")]

// Setting ComVisible to false makes the types in this assembly not visible
// to COM components.  If you need to access a type in this assembly from
// COM, set the ComVisible attribute to true on that type.
[assembly: ComVisible(false)]

// The following GUID is for the ID of the typelib if this project is exposed to COM
[assembly: Guid("0f5c7fc0-2ceb-4201-92ea-a8cb01b22b09")]

// Version information for an assembly consists of the following four values:
//
//      Major Version
//      Minor Version
//      Build Number
//      Revision
//
// You can specify all the values or you can default the Build and Revision Numbers
// by using the '*' as shown below:
// [assembly: AssemblyVersion("1.0.*")]
[assembly: AssemblyVersion("1.0.0.0")]
[assembly: AssemblyFileVersion("1.0.0.0")]
[assembly: log4net.Config.XmlConfigurator(Watch = true)]
