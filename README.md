# Unity - Asset Dependency Graph 

This project provides a basic Asset Dependency Graph for Unity using the new [GraphView](https://docs.unity3d.com/2019.2/Documentation/ScriptReference/Experimental.GraphView.GraphView.html) api.

![](Images/Example.png?raw=true)

## Install instructions
Simply copy the [AssetDependencyGraph](Assets/Editor/AssetDependencyGraph.cs) C# script into your existing project

## Usage

The Asset Dependency Graph Window can be opened via the `Window > Analysis >Asset Dependency Graph` file menu

![](Images/Usage.png?raw=true)

Once the window is open:
1. Select the root asset you want to inspect in the Project window
2. Click the `Explore Asset` button in the graph window

Any questions? Ask [@peanutbuffer](https://twitter.com/PeanutBuffer)

## Tested against
2019.2, 2019.1, 2018.4, 2018.3
