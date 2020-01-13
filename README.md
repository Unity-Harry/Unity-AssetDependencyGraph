# Unity - Asset Dependency Graph 
[![openupm](https://img.shields.io/npm/v/com.harryrose.assetdependencygraph?label=openupm&registry_uri=https://package.openupm.com)](https://openupm.com/packages/com.harryrose.assetdependencygraph/)

This project provides a basic Asset Dependency Graph for Unity using the new [GraphView](https://docs.unity3d.com/2019.2/Documentation/ScriptReference/Experimental.GraphView.GraphView.html) API.

![](Images~/Example.png?raw=true)

## Install instructions

### Install via OpenUPM

The package is available on the [openupm registry](https://openupm.com). It's recommended to install it via [openupm-cli](https://github.com/openupm/openupm-cli).

```
openupm add com.harryrose.assetdependencygraph
```

### Install via git url
1. Close Unity and open the `Packages/manifest.json` file
2. Add `"com.harryrose.assetdependencygraph": "https://github.com/Unity-Harry/Unity-AssetDependencyGraph.git",` to the `dependencies` section

## Usage

The Asset Dependency Graph Window can be opened via the `Window > Analysis >Asset Dependency Graph` file menu

![](Images~/Usage.png?raw=true)

Once the window is open:
1. Select the root asset you want to inspect in the Project window
2. Click the `Explore Asset` button in the graph window

Any questions? Ask [@peanutbuffer](https://twitter.com/PeanutBuffer)

## Tested against
2019.2, 2019.1, 2018.4, 2018.3
