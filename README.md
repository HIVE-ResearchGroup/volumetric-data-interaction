# Volumetric Data Interaction
Interaction with volumetric data such as the arbitrary slicing with a handheld tablet.

## Used Technology

### Unity 2019.2.21f1
This Unity version is needed to allow the usage of SRWorks.

### ParrelSync v1.5.1 (pre-release)
https://github.com/VeriorPies/ParrelSync

ParrelSync is a Unity editor extension under the MIT License.
It is used to allow Unity to run mulitiple instances on the same machine. Multiple instances are synced automatically.

For installation, a .unitypackage file can be downloaded and installed over Assets > Import Package.
Use ParrelSync > Clones Manager and click "add new clone".
"Open in new Editor", will open the cloned instance in another window.

### MLAPI v12.1.7
As Unity's UNet is marked as deprecated (https://docs.unity3d.com/Manual/UNet.html), it is recommended to use the new new Multiplayer and Networking
Solution (MLAPI, https://docs-multiplayer.unity3d.com/) which is currently under development.

The Unity MLAPI (Mid level API) is a framework that simplifies building networked games in Unity and licensed under the MIT License.

#### Installation
As MLAPI can only be installed using an url through the package manager from 2019.4+, 
the installation was executed by importing the .unitypackage file with Assets > Import Package.
The correct import file was downloaded from the GitHub repository (https://github.com/Unity-Technologies/com.unity.netcode.gameobjects/releases).
The MLAPI is displayed as MLAPI Profiler in the menu point Window.

Another option would have been the import from disk using the package manager in this version. Due to errors which indicated missing references, 
this option has been abandoned.

#### Tutorial
When it comes to tutorials, there may be some differences in naming.
e.g.
NetworkManager = NetworkingManager
NetworkObject = NetworkedObject
NetworkTransform = NetworkedTransform

To test the usage of MLAPI the first tutorial from a youtube series was implemented (https://www.youtube.com/watch?v=Dux5xGidEdc) in the Scene MLAPI Tutorial.

Unity itself also provides two tutorials for a helloworld application involving MLAPI (https://docs-multiplayer.unity3d.com/docs/tutorials/helloworld/helloworldintro).

The tutorial can be tested using the already installed ParrelSync functionality.