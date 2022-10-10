# Volumetric Data Interaction in AR Using a Handheld Touch-Sensitive Tablet
This prototypical implementation allows the interaction with volumetric data using a handheld tablet in a mixed reality environment. It can be placed along Milgram's reality virtuality contiunuum. While it was developed for AR, it can also be used in VR.

Keywords: `Augmented Reality`, `Volumetric Data`, `Surface Data`, `Touch Input`, `Spatial Input`, `Tangible User Interface`, `Cutting plane`

## Table of Content
* [Master thesis](#section-1)
  * [Features](#section-1.1)
  * [Tutorials](#section-1.2)
* [Structure](#section-2)
  * [Used Technologies](#section-2.1)
* [Installation](#section-3)
* [Configurations](#section-4)
  * [Change host IP](#section-4.1)
  * [Change model](#section-4.2)
  * [Change cutting plane image path](#section-4.3)
  * [Change tablet sensibility](#section-4.4)

## <a name="section-1"></a>Master thesis
The basis of this application was developed in the course of a master thesis which is available at the HIVE website: <br />
https://hive.fh-hagenberg.at/wp-content/uploads/2022/10/Master-Thesis_v.1.23.pdf

The basic concept, design, and implementation are described in the thesis. A quantitative study was conducted which evaluated the different functionalities and summed up an outlook over different feature changes.

### <a name="section-1.1"></a>Features
The following features are already described in the master thesis and functional since July 2022.

#### Spatial features
* Repositioning
* Reorientating
* Resizing

#### Exploration features
* Temporary removing parts using a cutting plane
* Permanent removing parts
* Creating snapshots
* Explore snapshot neighbours

## <a name="section-1.2"></a>Tutorials
Videos about the usage of the prototype are available at: <br />
https://youtube.com/playlist?list=PLHJfKKOlMnontcmOoD_-RYay4I73gc9zQ <br />
In these videos a `HTC Vive Pro Eye` was used in combination with a `Samsung Galaxy Tab S6 Lite`.

## <a name="section-2"></a>Structure
The prototype utilises two applications, the `client` which is deployed on the tablet, and the `host` which runs on the PC. Both need to be in the same network to communicate. In case the communication is not working it is best to check if the host IP is correct and if the PC has the firewall turned off.

This repository has the version for the client application on the `client` branch. The version for the host is on the `main` branch. The client has its own branch as the application on the client should not need to be further developed. It has some dead code which once belonged to the host but is not relevant anymore.

### <a name="section-2.1"></a>Used Technologies
* Unity 2019.2.21f1 (version needed to allow SRWorks)
* SteamVR 1.22.13 (needed for HMD and PC connection)
* SRWorks 0.9.7.1 (needed to allow AR mode)


## <a name="section-3"></a>Installation
For the prototype to work the computer needs to run SteamVR, SRWorks, and have the Unity Host scene running. In addition, the correct model needs to be in the scene and the configurations for the model name and volumetric data folder need to work (see section [`Change model`](#section-4.2)).
The tablet needs to have the client scene apk.

To start the prototype, first the host scene needs to be started before the client app is opened. If it is the other way around the client does not have a host to connect to.

## <a name="section-4"></a>Configurations
There are multiple things which can be configured for the prototype.

### <a name="section-4.1"></a>Change host IP
The host IP address needs to be updated depending on the PC. The PC used for development changed IP address after every restart. The variable `HOST_IP` can be changed in the `Constants/ConfigurationConstants.cs` file.
As this variable is needed by the client application, it needs to be changed in the `client` branch. The Unity project needs to be rebuilt and the new apk file needs to be deployed on the tablet.

### <a name="section-4.2"></a>Change model
To change the model, the surface model in the unity scene and the path for the volumetric data file need to be replaced.
The name surface model needs to be equal to the value of `ModelName` variable in the `Constants\StringConstants.cs` script.
The path for the internal structure (volumetric data slices folder) is to be changed in the `Constants/ConfigurationConstants.cs` file. The model constructor in `Model.cs` uses the `ConfigurationConstants.X_STACK_PATH_LOW_RES` variable which should be changed. It is best to exchange this call with a more generic variable which holds a constant.

### <a name="section-4.3"></a>Change cutting plane image path
All calculated cutting plane images are saved in form of a bitmap and a png to a folder. This folder path can be changed using the `IMAGES_FOLDER_PATH` in the `Constants/ConfigurationConstants.cs` file.

### <a name="section-4.4"></a>Change tablet sensibility
The thresholds for the configuration of the tablet sensibilities can be changed in the `SpatialInput` and `TouchInput` scripts. To do this either an input field can be implemented on the client UI to change the threshold during run time. The other option is the trial and error way. After the thresholds are changed, the application needs to be rebuilt and redeployed to the tablet. This process can be bothersome as multiple configurations need to be tried.

