This is a small hobby project I made to try out creating a Waveform Collapse-based system for generating data. Simply enough, the program (made in Unity) can generate simplstic maps based on rules between different tile types. There is a Windows 64-bit build in the /builds/ folder, contained inside a .zip file. No installation required, just run the Waveform-Collapse-Demo.exe file.

This project is not very well optimized, as it was not the main objective of the project. Its good enough to generate 100x100 maps within decent times on medium desktop machines. Have not tested anything on laptops.

Press ESCAPE to exit the application.

Tile types follow the following rules:
Name : Can only border
Deep Water : Deep Water, Shallow Water
Shallow Water : Deep Water, Shallow Water, Sand
Sand : Shallow Water, Sand, Grass, Desert
Grass : Sand, Grass, Trees
Trees : Grass, Trees, Mountains
Mountains : Trees, Mountains
Desert : Sand, Desert

By default, the probability of each border pair is determined by a weight-system for each tile type. Modifying these weights will change how much of each border-pair you will encounter. Each tile type also has a Proportion Weight, that influences about how much of each TileType there should be on the map.
Deep Water : 10 Proportion Weight
* Deep Water neighbor : 1 Weight
* Shallow Water neighbor : 2 Weight
Shallow Water : 20 Proportion Weight
* Deep Water neighbor : 1 Weight
* Shallow Water neighbor : 2 Weight
* Sand neighbor : 1 Weight
Sand : 5 Proportion Weight
* Shallow Water neighbor : 20 Weight
* Sand neighbor : 10 Weight
* Grass neighbor : 30 Weight
* Desert neighbor : 1 Weight
Grass : 40 Proportion Weight
* Sand neighbor : 1 Weight
* Grass neighbor : 2 Weight
* Trees neighbor : 2 Weight
Trees : 20 Proportion Weight
* Grass neighbor : 1 Weight
* Trees neighbor : 2 Weight
* Mountains neighbor : 1 Weight
Mountains : 10 Proportion Weight
* Trees neighbor : 2 Weight
* Mountains neighbor : 1 Weight
Desert : 10 Proportion Weight
* Sand neighbor : 1 Weight
* Desert neighbor : 2 Weight

An examples from the list above:
* There should be about twice as many Grass tiles compared to Trees tiles.
* Sand tiles have a bigger influence on its neighbors than other tiles, since some of their weights are in the double digits.
* Mountains should be more likely to neighbor Trees than other Mountains
* While Deserts are very unlikely to neighbor Sand, Desert tiles are more likely to neighbor other Desert tiles.