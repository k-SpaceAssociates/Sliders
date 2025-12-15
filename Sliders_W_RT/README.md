This project makes a .exe file that checks to see if the OS needs the .NET 9.0 Desktop Runtime installed first and installs if needed.
Next, it has the Sliders.msi file from the Sliders project embedded in it, so it also does that install.
For systems with Windows 11 the .NET 9.0 Desktop Runtime is already installed, so it just installs the Sliders.msi file.

Note: To keep the Control Panel "Programs and Features" entry for this installer from making multiple entries always keep all the revisions in sync for
Sliders, Sliders_INSTALLER and Sliders_2_W_RT projects. See below for a detailed example:
In SLiders.csproj file:
	  <PropertyGroup>
	      <Version>1.0.2.0</Version>
	      <AssemblyVersion>1.0.2.0</AssemblyVersion>
	      <FileVersion>1.0.2.0</FileVersion>
	  </PropertyGroup>

In Sliders_INSTALLER package.wxs file:
	  <Package 
	      Name="Sliders_Installer"
	      Manufacturer="k-Space Associates, Inc."
	      Version="1.0.2.0"
	      UpgradeCode="C776C6FC-D996-4032-B72F-4EB2785793F4"
	  >

In Sliders_W _RT Bundle.wxs file:
	  <Bundle Name="Sliders_W_RT"
          Version="1.0.2.0" 
          Manufacturer="k-Space Associates" 
          UpgradeCode="1a3a217d-1fbd-49bb-9f84-8731f60e463a" 
          IconSourceFile="kSA.ico"
	  >


Note for other bootstrapper projects: 

1. There was a quirk that I found. The nuget package WixToolset.Util.wixext was needed but when it was added, 
only the reference WixBalExtension was automatically done. I had to manually add the WixUtilExtension by going into the same folder used for 
WixBalExtension (C:\Program Files (x86)\WiX Toolset v3.11\bin) and selecting the WixUtilExtension.dll file to add the reference to the project file.
2. I downloaded the .NET 9.0 Desktop Runtime installer (windowsdesktop-runtime-9.0.11-win-x64.exe) and added it as 
SourceFile="windowsdesktop-runtime-9.0.11-win-x64.exe" to the ExePackage section in the Bundle.wxs file in this project folder so that others
downloading this from the repository will have the needed file.
3. Build dependencies: Make sure to build kSADataStreamViewer, then Sliders, then Sliders_INSTALLER, then kSA_RMAT_2_W_RT file. By default Visual Studio
got confused and tried to build kSA_RMAT_2_W_RT before Sliders_INSTALLER which caused a build failure.
4. Extensions in Visual Studio that I specifically added for this solution: 
   a. Heat Wave for VS 2022 (for the installer project)
   b. Wix toolset v3 extensions for Visual Studio
   c. Wix v3 Visual Studio 2022 Extension