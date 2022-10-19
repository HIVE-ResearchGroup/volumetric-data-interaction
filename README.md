# Volumetric Data Interaction in AR Using a Handheld Touch-Sensitive Tablet
This prototypical implementation allows the interaction with volumetric data using a handheld tablet in a mixed reality environment. It can be placed along Milgram's reality virtuality contiunuum. While it was developed for AR, it can also be used in VR.

Keywords: `Augmented Reality`, `Volumetric Data`, `Surface Data`, `Touch Input`, `Spatial Input`, `Tangible User Interface`, `Cutting plane`

## Table of Content
* [Features](#section-1)
  * [Geometric Transformations](#section-1.1)
  * [Exploration Interactions](#section-1.2)
* [Structure](#section-2)
  * [Used Technologies](#section-2.1)
  * [Used Hardware](#section-2.2)
* [Installation](#section-3)
  * [Building client apk](#section-3.1) 
* [Configurations](#section-4)
  * [Change host IP](#section-4.1)
  * [Change model](#section-4.2)
  * [Change cutting plane image path](#section-4.3)
  * [Change tablet sensibility](#section-4.4)
  * [Set tracker in Unity](#section-4.5)

The basis of this application was developed in the course of a master thesis which is available at the HIVE website: <br />
https://hive.fh-hagenberg.at/wp-content/uploads/2022/10/Master-Thesis_v.1.23.pdf

The basic concept, design, and implementation are described in the thesis. A quantitative study was conducted which evaluated the different functionalities and summed up an outlook over different feature changes.

## <a name="section-1"></a>Features
This prototypical implementation offers spatial and exploration features to handle and investigate a three dimensional data set.

#### Geometric transformation features
* Repositioning
* Reorientating
* Resizing

#### Exploration features
* Temporary removing parts using a cutting plane
* Permanent removing parts
* Creating snapshots
* Explore snapshot neighbours

Videos about the usage of the prototype with a `HTC Vive Pro Eye` in combination with a `Samsung Galaxy Tab S6 Lite` are available at: <br />
https://youtube.com/playlist?list=PLHJfKKOlMnontcmOoD_-RYay4I73gc9zQ <br />

###  <a name="section-1.1"></a>Geometric Transformations
The user can start the `selection` of an object by tapping the left side of the screen when in the main menu. This creates a selection ray which is used to intersect object. A double tap when intersecting selects the object. When an object is selected actions such as repositioning and resizing can be executed. 
In the following gif the user performs a touch hold action on the screen to start a `mapping` process. While the hold action is active all movements, including rotations, of the tablet are simulated. When the touch hold is stopped, the mapping of the tablets position and rotation changes stops.

<div align="center">
  <image src="https://github.com/HIVE-ResearchGroup/volumetric-data-interaction/blob/main/Gifs/Select%20and%20Map.gif"/>
</div>
<br />

After the selection of an object, it can also be `rotated` using the touch interface. The orientation of the tablet is essential for the choice of the rotation axis.
When the tablet is held horizontally, the object is rotated around x-axis while the rotation is executed around x-axis when the tablet is positioned up right.
The common pinch gesture is utilised to `resize` the selected object.

<div align="center">
  <image src="https://github.com/HIVE-ResearchGroup/volumetric-data-interaction/blob/main/Gifs/Rotate%20and%20Resize.gif"/>
</div>
<br />

###  <a name="section-1.2"></a>Exploration Interactions
The `exploration mode` can be entered when returning to the main mode and tapping the right instead of the left side. A cutting plane is added in a short distance before the handheld tablet. This allows for the tablet to be used to position a cut within the three dimensional model. The following gif shows how a `snapshot` is created by positioning the cutting plane within the object and using a swipe outside gesture to place the intersection image. The swipe direction indicates the position the cutting image should be positioned in within the user's environment. Snapshots are, such as the three dimensional model itself, object which can be selected, repositioned and resized.

<div align="center">
  <image src="https://github.com/HIVE-ResearchGroup/volumetric-data-interaction/blob/main/Gifs/Select%20and%20Map.gif"/>
</div>
<br />

Snapshots are placed in the users environment depending on the swipe direction. If the swipe is executed to the left, it is positioned on the left side. If the swipe is exectuted to the right, it is the other way around. The `snapshot alignment` feature can be used to allow the user an overview over all taken snapshots (up to 5 images). The user performs a pinch gesture, same as would be done to zoom out or minimise an object. This gesture aligns all snapshots around the handheld tablet or, if this is already the case, repositions them on their original place within the user's environment. 

<div align="center">
  <image src="https://github.com/HIVE-ResearchGroup/volumetric-data-interaction/blob/main/Gifs/Snapshot%20Alignment.gif"/>
</div>
<br />

When a snapshot has been selected a `semi-transparent black plane` is positioned within the model to visualise the position of the cutting plane when creating the intersecting image (snapshot). This visualises the context of the queried two dimensional data. The selected snapshot is also displayed on the tablet overlay. The user can `inspect the neighbouring` internal structure by moving this plane for- and backward. This is done by tilting the tablet to the left and right when a snapshot is active. The original snapshot does not change but the neighbouring slice is displayed on the tablet overlay instead of the original image. This allows for a direct comparison when lifting the tablet with the new structure image next to the original snapshot. The function can be exited, such as all others, by performing an inwards swipe.

<div align="center">
  <image src="https://github.com/HIVE-ResearchGroup/volumetric-data-interaction/blob/main/Gifs/Snapshot%20Neighbour%20Inspection.gif"/>
</div>
<br />

Instead of cutting the model only temporarily when creating a snapshot, the model can also be `cut permanently`. The tablet with the cutting plane needs to be positioned within the model, best in the middle of the cutting plane. A double tap executes the cut, removes all surfaces which have been intersected by the handheld tablet and the intersection image is placed upon the cutting surface to simulate the internal structure to the outside.

<div align="center">
  <image src="https://github.com/HIVE-ResearchGroup/volumetric-data-interaction/blob/main/Gifs/Permanent%20Cutting.gif"/>
</div>
<br />

All cuts and snapshots can be `reset` by shaking the tablet. If a snapshot is selected when the tablet is shaken, only the one snapshot is removed, otherwise all snapshots are removed. If there are no snapshot to be removed, the model is reset to its original state.

<div align="center">
  <image src="https://github.com/HIVE-ResearchGroup/volumetric-data-interaction/blob/main/Gifs/Reset.gif"/>
</div>


## <a name="section-2"></a>Structure
The prototype utilises two applications, the `client` which is deployed on the tablet, and the `host` which runs on the PC. Both need to be in the same network to communicate. In case the communication is not working it is best to check if the host IP is correct and if the PC has the firewall turned off.

This repository has the version for the client application on the `client` branch. The version for the host is on the `main` branch. The client has its own branch as the application on the client should not need to be further developed. It has some dead code which once belonged to the host but is not relevant anymore.

### <a name="section-2.1"></a>Used Technologies
* Unity 2019.2.21f1 (version needed to allow SRWorks)
* SteamVR 1.22.13 (needed for HMD and PC connection)
* SRWorks 0.9.7.1 (needed to allow AR mode)

### <a name="section-2.2"></a>Used Hardware
As a tablet a Samsung Galaxy Tab can be used (tried with S7 and S6 lite). The android version should be `android 11`. Unity cannot compile the apk for `android 12` so the android apk cannot be used for such devices. No workaround (e.g. gradle changes) to make andoid 12 work succeeded up to now. 

## <a name="section-3"></a>Installation
For the prototype to work the computer needs to run SteamVR, SRWorks, and have the Unity Host scene running. In addition, the correct model needs to be in the scene and the configurations for the model name and volumetric data folder need to work (see section [`Change model`](#section-4.2)).
The tablet needs to have the client scene apk.

To start the prototype, first the host scene needs to be started before the client app is opened. If it is the other way around the client does not have a host to connect to.

### <a name="section-3.1"></a>Building client apk
The client apk is the compiled application which can be deployed on the tablet. To creat it, you need to open Unity and navigate to `File > Build Settings`. Make sure the `Scenes/Client` scene in the top, and the `Android` platform in the bottom left, are selected. If the platform needed to be changed, also click the `Switch Platform` button on the bottom right. Then you can click `Build`. If popups appear during compilation press `Ignore all`. The created apk then needs to be saved to the tablet (e.g. in the `Download\client` folder), on which it can be installed and run by opening it. 

## <a name="section-4"></a>Configurations
There are multiple things which can be configured for the prototype.

### <a name="section-4.1"></a>Change host IP
The host IP address needs to be updated depending on the PC, sometimes after every restart of the PC. The variable `HOST_IP` can be changed in the `Constants/ConfigurationConstants.cs` file. To get the IPv4 address of the PC the command line needs to be opened (e.g. type `cmd` in search bar). The command `ipconfig` returns all port addresses. Choose the IPv4 address of the WLAN.
As this variable is needed by the client application, it needs to be changed in the `client` branch. The Unity project needs to be rebuilt and the new apk file needs to be deployed on the tablet.

### <a name="section-4.2"></a>Change model
To change the model, the surface model in the unity scene and the path for the volumetric data file need to be replaced.
The name surface model needs to be equal to the value of `ModelName` variable in the `Constants\StringConstants.cs` script.
The path for the internal structure (volumetric data slices folder) is to be changed in the `Constants/ConfigurationConstants.cs` file. The model constructor in `Model.cs` uses the `ConfigurationConstants.X_STACK_PATH_LOW_RES` variable which should be changed. It is best to exchange this call with a more generic variable which holds a constant.

### <a name="section-4.3"></a>Change cutting plane image path
All calculated cutting plane images are saved in form of a bitmap and a png to a folder. This folder path can be changed using the `IMAGES_FOLDER_PATH` in the `Constants/ConfigurationConstants.cs` file.

### <a name="section-4.4"></a>Change tablet sensibility
The thresholds for the configuration of the tablet sensibilities can be changed in the `SpatialInput` and `TouchInput` scripts. To do this either an input field can be implemented on the client UI to change the threshold during run time. The other option is the trial and error way. After the thresholds are changed, the application needs to be rebuilt and redeployed to the tablet. This process can be bothersome as multiple configurations need to be tried.

### <a name="section-4.5"></a>Set tracker in Unity 
The tracking device in Unity might need to be set after starting the host application. When the host scene is running, go to `DontDestroyOnLoad > Player > SteamVRObjects > Tracker` and set the `Index` of the `Steam VR_Tracked Object` script to any device that causes the game object position in the inspector to move slightly.

