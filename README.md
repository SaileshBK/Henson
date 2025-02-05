# Henson
![Henson Logo](https://cdn.discordapp.com/attachments/700554123086528614/1080372541815791657/hensonlogo.png)

Henson (**H**euristic and **E**xtensible **N**ation**S**tates **O**rganizer for 
**N**ations) is a puppet manager for NationStates. The core principles for 
Henson are:
1. **To be easy to use.** If one can play NationStates, they should be able to 
easily learn and understand how to use Henson.
2. **To be powerful.** Henson should be able to enable NationStates players with 
large amounts of puppets for various reasons (raiding/defending, card farming, 
or just a general obsession with the game) to do awesome things that may be 
limited for them by the website’s interface.
3. **To be rules-compliant.** Henson should ideally follow all API and website 
restrictions at all times, including complying with rate-limits, restricted 
actions, and prohibited actions. That means not only ensuring that the tool 
complies with all restrictions if used correctly, but eliminating the 
possibility of abuse by safeguarding the program from performing illegal actions 
if used maliciously.
4. **To be fun.** Henson should never take the fun out of NationStates, no matter 
what players use it for.

Got questions? DM me on Discord at nota.name or telegram me on NationStates 
at https://www.nationstates.net/nation=notanam.

## Installation (Windows)
1. Go to the [releases page](https://github.com/NotAName320/Henson/releases) and 
download the latest version.
2. Extract and run! It should work without having to install anything, but you 
may need the latest version of the .NET 6.0 Desktop Runtime, which you can 
download [here](https://dotnet.microsoft.com/en-us/download/dotnet/6.0).

## Installation (Linux)
1. First, make sure you have the latest version of the .NET runtime or SDK 
installed on your system, which you can do via your package manager. **DO NOT 
INSTALL IT VIA SNAP, IT'S BROKEN AND IT SUCKS.**
2. Go to the [releases page](https://github.com/NotAName320/Henson/releases) 
and download the latest version.
3. Extract the files to a directory of your choice, then run 
`chmod +x ./Henson` in that directory.
4. Run `./Henson` or double click the Henson file. Enjoy!

## Installation (MacOS)
1. Install the latest version of the .NET runtime or SDK, which you can find
[here](https://dotnet.microsoft.com/en-us/download). Do not install the arm64
version, even if you have a Mac with an Apple Silicon processor.
2. Go to the [releases page](https://github.com/NotAName320/Henson/releases) and
download the latest `osx-x64` version. (Once again, avoid the arm64 version.)
3. Extract the files to a directory of your choice, then run
`chmod +x ./Henson` in that directory.
4. Run `./Henson`. You'll likely get a series of errors that various files
are from unidentified developers. In System Preferences, under Privacy & Security, you should see a prompt about this. Click 'Open Anyway' and then
run `./Henson` again. You may need to repeat this process a few times.

## Updating Henson
Simply drag the new files on top of the old ones and replace them. The program 
will automatically update the settings file and database with new features!

## Contributing
Simply pull the repository from GitHub and open the solution with your IDE of 
choice.

For consistency's sake, please follow some general style guidelines
- Braces on new line
- No space between keyword and opening parenthesis (e.g. `if(condition)`)
- Avoid long lines, no hard limit across the repository (yet)

By making a pull request, you agree to release your code to my copyright to be 
licensed under GPLv3.

## Acknowledgements
Thanks to:
- The people listed in the 
[NOTICE](https://github.com/NotAName320/Henson/blob/main/Henson/NOTICE) file 
for creating awesome open-source software
- sweeze for creating Swarm and thus figuring out a lot of dumb HTTP site 
stuff for me
- rootabeta for being an early adopter of Henson and giving me lots of 
suggestions on how to improve it
- qekitor because of dumb raid chat challenges on 3/4/23

_**"If I have seen further, it is by standing on the shoulders of giants."**_ -Isaac Newton

## Screenshot
![hensonss](https://github.com/NotAName320/Henson/assets/27144780/523942d1-5ae7-441c-9bfd-681c535e17a8)

