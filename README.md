# Sakuga-Engine
Sakuga Engine is (another) fighting game engine project of mine. The goal is to make a simple and versatile framework to create my own anime fighting games.
## Yeah, but why creating a new one?
My previous attempt (AFF) was functional, but I made a lot of dumb decisions which made adding new features a pain in the ass... I'm making this one to remove as much tech debt from AFF I can and trying new ideas and be more thoughtful about how I make stuff.
### And port it to Godot 4!
Godot 3 lacks a lot of features and has a lot of problems that Godot 4 solves, with some quality of life improvements that I really miss in the older version.
I was planning to migrate since a while, so let's see how it goes.
## Features
### Included
- 2D 1v1 anime-style fighting game
- Rollback netcode out of the box
- Supports 3D sprites and 3D models
- Robust state system
- Stances
- Projectiles
- Pseudorandom number generator
- AI (in progress)
- An example character
### For the future
- Game modes
- Puppets
- Cinematics
- Default Steamworks support
- An original character
### Notes
- Made completely in C#
- Currently using Godot 4.6.1 .NET
- It's strongly advised to know the basics of Godot to use it properly
## Frequently Answered Questions
### Godot complains about a lot of scripts missing. What should I do?
Make sure you are using the .NET version of Godot 4.
### Do I need to learn GDScript to use it?
Sakuga Engine is completely written in C# so there's no GDScript code to interact with for the core engine functionality. Also, Sakuga's philosohpy is to allow anyone to make a basic fighting game without writing a single line of code. That being said, it's recommended to know C# if you have bigger ambitions for it.
### Can I make a 3D game with it?
No, Sakuga Engine is only for 2D traditional fighting games.
### Can i make a tag fighter?
No, Sakuga Engine is only for 1v1 games at the moment.
### Can I use it for my fan game?
Sakuga Engine is free to use for any commercial or non-commercial project.
### Can you add feature X Y Z...
My main goal is to have the core functionality in a good state and add features necessary for my own projecs. Suggestions and requests are welcome, but keep in mind that they might not be a priority (or added at all). If you want a feature Sakuga doesn't have, consider to include it yourself.
### How can I contribute?
You can fork this repository and add features if you want. If I consider your pull request valuable I'll include it into the engine eventually.
### Unity version when?
There _is_ a repository for Unity but it's outdated at the moment. Gonna update it in the near future when I find the time and patience.
### How do I make a character?
You can check the Wiki to find a starting guide and some extra material about the engine in the link below.

[Wiki](https://github.com/NoisyChain/Sakuga-Engine/wiki)

[Join the Discord server!](https://discord.gg/2denEKZFeX)
