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
  <image src="Gifs/Select%20and%20Map.gif"/>
</div>
<br />

After the selection of an object, it can also be `rotated` using the touch interface. The orientation of the tablet is essential for the choice of the rotation axis.
When the tablet is held horizontally, the object is rotated around x-axis while the rotation is executed around x-axis when the tablet is positioned up right.
The common pinch gesture is utilised to `resize` the selected object.

<div align="center">
  <image src="Gifs/Rotate%20and%20Resize.gif"/>
</div>
<br />

###  <a name="section-1.2"></a>Exploration Interactions
The `exploration mode` can be entered when returning to the main mode and tapping the right instead of the left side. A cutting plane is added in a short distance before the handheld tablet. This allows for the tablet to be used to position a cut within the three dimensional model. The following gif shows how a `snapshot` is created by positioning the cutting plane within the object and using a swipe outside gesture to place the intersection image. The swipe direction indicates the position the cutting image should be positioned in within the user's environment. Snapshots are, such as the three dimensional model itself, object which can be selected, repositioned and resized.

<div align="center">
  <image src="Gifs/Select%20and%20Map.gif"/>
</div>
<br />

Snapshots are placed in the users environment depending on the swipe direction. If the swipe is executed to the left, it is positioned on the left side. If the swipe is exectuted to the right, it is the other way around. The `snapshot alignment` feature can be used to allow the user an overview over all taken snapshots (up to 5 images). The user performs a pinch gesture, same as would be done to zoom out or minimise an object. This gesture aligns all snapshots around the handheld tablet or, if this is already the case, repositions them on their original place within the user's environment. 

<div align="center">
  <image src="Gifs/Snapshot%20Alignment.gif"/>
</div>
<br />

When a snapshot has been selected a `semi-transparent black plane` is positioned within the model to visualise the position of the cutting plane when creating the intersecting image (snapshot). This visualises the context of the queried two dimensional data. The selected snapshot is also displayed on the tablet overlay. The user can `inspect the neighbouring` internal structure by moving this plane for- and backward. This is done by tilting the tablet to the left and right when a snapshot is active. The original snapshot does not change but the neighbouring slice is displayed on the tablet overlay instead of the original image. This allows for a direct comparison when lifting the tablet with the new structure image next to the original snapshot. If the plane `cannot be moved further` for- or backwards, a red cross is rendered on the tablet overlay. The function can be exited, such as all others, by performing an inwards swipe.

<div align="center">
  <image src="Gifs/Snapshot%20Neighbour%20Inspection.gif"/>
</div>
<br />

Instead of cutting the model only temporarily when creating a snapshot, the model can also be `cut permanently`. The tablet with the cutting plane needs to be positioned within the model, best in the middle of the cutting plane. A double tap executes the cut, removes all surfaces which have been intersected by the handheld tablet and the intersection image is placed upon the cutting surface to simulate the internal structure to the outside.

<div align="center">
  <image src="Gifs/Permanent%20Cutting.gif"/>
</div>
<br />

All cuts and snapshots can be `reset` by shaking the tablet. If a snapshot is selected when the tablet is shaken, only the one snapshot is removed, otherwise all snapshots are removed. If there are no snapshot to be removed, the model is reset to its original state.

<div align="center">
  <image src="Gifs/Reset.gif"/>
</div>


## <a name="section-2"></a>Structure
The prototype consists of a single Unity application with different scenes for the `client` and the `host`. The `Client` scene needs to be deployed to a tablet, while the `Host` scene is deployed on a PC. Both devices need to be in the same network to communicate. In case of communication problems, check your firewall settings.

### <a name="section-2.1"></a>Used Technologies
* Unity 2021.3.33f1
* SteamVR (tested with 2.3.5, needed for HMD and PC connection)

### <a name="section-2.2"></a>Used Hardware
Any Android based device will suffice (tested with Android 12 and 13), but for development a Samsung Galaxy Tab was used (S7 and S6 lite).

## <a name="section-3"></a>Installation
SteamVR needs to be installed and running. In Unity inside the `Host` scene, a model has to be setup (see section [`Change model`](#section-4.2)).  
The tablet needs to run the client APK.

### <a name="section-3.1"></a>Building Client APK
Open Unity and navigate to `File > Build Settings`. Make sure the `Scenes/Client` scene appears in the top and is activated with index 0 (disable all other scenes to be safe). Select `Android` as the platform in the bottom left. If the platform needs to be changed, also click the `Switch Platform` button on the bottom right.  
Click `Build` and wait for it to finish (select a build folder if needed). If popups appear during compilation press `Ignore all`.  
The created APK then needs to be saved on the tablet (e.g. in the `Download` folder), on which it can be installed and run by opening it. Check if your Android device allows sideloading apps.

## <a name="section-4"></a>Configurations
There are multiple things which can be configured for the prototype.

### <a name="section-4.1"></a>Host IP
To run the client, you need to know the hosts IP-address.  
To show the IPv4 addresses on an Windows PC, open up any command line (Win + R and type `cmd`) and run `ipconfig`. Choose the address which belongs to the network where both the host and the client are connected to.  
Enter the address in the client to connect.

### <a name="section-4.2"></a>Change Model
The default sewing machine model will serve as a template for new models.  
Inside the model script, the folder with the cutting plane images has to be specified (see section [`Change Cutting Plane Image Path`](#section-4.3)).  
Drag the new model-prefab into the `ModelManager` inside the `Host` scene and make sure it is the only active child of `ModelManager`. In the `ModelManager`, drag and drop the model into the `Model Manager` script onto the empty `Model` field.

### <a name="section-4.3"></a>Change Cutting Plane Image Path
All calculated cutting plane images must be saved as PNGs and be ordered alphabetically for Unity to read them in the right order.  
Set the path of the folder to the model prefab as described in section [`Change Model`](#section-4.2).

### <a name="section-4.4"></a>Change Tablet Sensitivity
The thresholds for the configuration of the tablet sensitivities can be changed in the `SpatialInput` and `TouchInput` scripts. To do this either an input field can be implemented on the client UI to change the threshold during run time. The other option is the trial and error way. After the thresholds are changed, the application needs to be rebuilt and redeployed to the tablet. This process can be bothersome as multiple configurations need to be tried.