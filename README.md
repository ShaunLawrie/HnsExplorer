# HNS Explorer
A simple GUI for exploring the data in the Windows Host Network Service.  

The Host Network Service is the system underpinning container networking on Windows:  
https://docs.microsoft.com/en-us/virtualization/windowscontainers/container-networking/architecture

This was created while I've been learning more about the HNS and how to debug container networking issues on Windows Kubernetes nodes.

## How To Use

### Search
Entering something like a container ID in the text entry field and hitting enter will do a primitive search for all HNS activities that reference it.
![image](https://user-images.githubusercontent.com/13159458/164613193-aa8c04ff-9eda-474a-ba20-82c5494b51b7.png)

### Dump to JSON
The "Dump to JSON" button will dump all the data collected by the tool to a massive JSON file.

### Packet Capture
Packet capture for these virtual adaptors can be done with the pktmon tool https://docs.microsoft.com/en-us/windows-server/networking/technologies/pktmon/pktmon  
The "Packet Capture" button just sets up a basic capture using pktmon and converts the output to something that can be opened in Wireshark.  
![image](https://user-images.githubusercontent.com/13159458/164383733-767b3e46-cf75-4955-8946-2d78f22e7494.png)

### Windows Performance Recorder Trace
Launch a WPR trace using a cut down version of the trace format at https://github.com/MicrosoftDocs/Virtualization-Documentation/blob/main/windows-server-container-tools/wpr-profiles/HcsTraceProfile.wprp  
![image](https://user-images.githubusercontent.com/13159458/235348746-13ca7366-9279-4c07-8022-58af36686d67.png)
![image](https://user-images.githubusercontent.com/13159458/235348700-ff5da3f4-bb5a-4d1f-bd29-ef016f03501d.png)
![image](https://user-images.githubusercontent.com/13159458/235348751-12615954-d280-4afb-b0fe-ab9a7b47ea90.png)

### Reload HNS Data
The "Reload HNS Data" button will reload the data from the HNS/HCS apis.

## Build Instructions

To build install the dotnet 6 SDK https://dotnet.microsoft.com/en-us/download/dotnet/6.0

This will generate a single exe output 
```
dotnet publish -c Release -r win-x64 -p:PublishSingleFile=true -p:EnableCompressionInSingleFile=true --self-contained
```
