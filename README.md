# MapGen

MapGen is a tool that can be used to generate a 1v1 2D  map for a strategy turn-based game.

You can tweak the generation parameters to find the perfect map. Those parameters are listed below.

## In-game pictures

# ![menu](https://imgur.com/7mczWSj.png)

<p align="center">
  <img src="https://imgur.com/wPfww7Q.png">
</p>

# ![map3](https://imgur.com/7tqPITp.png)
# ![map2](https://imgur.com/lDv6BGu.png)
# ![map4](https://imgur.com/k8bIZes.png)
 
## Download and play the game
 
 If you want the game's build or simply play it on your browser, check this: https://defu.itch.io/mapgen
 
## Generation Options

### Basic

*Seed*: Change this value to generate a different map with the same other options.

*Width*: Map's width in tiles.

*Depth*: Map's depth in tiles.

### Seed Count

The number of tiles that will initally place of each type before the generation process is applied. The higher they are, the higher the amount of tiles of that type will be on the map.

### Resource Count

*Allied*: The number resources that each player starts with.

*Neutral*: The number of resource per each player that are placed on the map.

*Optimize Bases Placement*: Reduces the time used to calculate the best spots to put the player bases  by putting them on suboptimum spots. Warning! Disabling this option will  highly increase the time invested on generating big dimensions maps.
 
### Misc. Options

*Roads*: Enable/Disable roads

*Resources*: Enable/Disable resources

*Auto-adjus*t: If checked, the values above will be generated randomly according to the maps dimensions.

*Exposition*: If checked, a new map will be generated every five seconds.
 
## Build Requirments
 
In order to build or modify this source code you will need the following programs:
 
 - Unity™ 2017.3.1f1
 - Visual Studio 2017
 
## Build steps
 
 1. Clone this repository using git: git clone https://github.com/TFGProcGen/MapGen
 
 2. Move the project folder that has just been created  to your Unity Projects' folder
 
 3. Open Unity and choose the project *MapGen*
 
 4. Click *File->Build Settings->Choose operative System->Build*
 
## About us

This game is part of my bachelor thesis about procedural content generation in Ingeniería de Tecnologías de Telecomunicación on the University of Vigo.
