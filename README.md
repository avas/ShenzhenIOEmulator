# ShenzhenIOEmulator
This project is a programmable and hackable C# implementation of the [Shenzhen I/O](https://www.zachtronics.com/shenzhen-io/) game.

## Goal of this project
If you want to:
* Automate verification and stats calculation of the level solution (e.g. for some online leaderboard)
* Push game's limits, e.g. try your solution without code size limits
* Use code generation to adjust code parameters and try it (or just brute force the solution, e.g. to find the optimal solution for some puzzles like 'Security Nightmare')
...then this project might be for you - feel free to watch it or post any feature requests 😊

However, there are things that won't be in this project:
* It won't become a game. There is no point to do so, because such a game would be just an inferior clone of the original.
* It won't have any GUI for building the workspace (at least not now) - I just don't see the purpose of such UI.

## Disclaimer
1. Shenzhen I/O is a registered trademark of [Zachtronics LLC](https://www.zachtronics.com/), and I have absolutely no relation with Zachtronics nor Zach Barth.
2. This project does not use the code of Shenzhen I/O in any way - this is just an attempt to reverse-engineer the game's engine.
3. Please do not use this engine for building any Shenzhen I/O-like games. In the end, you will only be able to make games at most like Shenzhen I/O or (maybe, after some modification) TIS-100, but who would play yet another clone if they could play an original?
4. This project is still in active development, so many things might not be implelmented yet.

## Current state
What currently works:
* Parser of the assembly language
* All commands available in Shenzhen I/O

To be implemented:
* Some form of runtime to execute these commands (keep track of instruction pointer, handle conditional execution, track execution count and execution time of each command)
* Signal propagation between simple I/O ports of components (simple I/O nets)
* Same thing for XBus ports
* MCxxxx controllers - facades that would tokenize and parse the code, handle its execution and contain all ther registers and I/O ports
* Other components (this would be much simpler...)
* Test inputs and outputs that would simply send predefined data or collect actual data and compare it with predefined expected data;
* Workspace - a facade that would allow to programmatically build the level (add components, link them together, load code, load test inputs and outputs...), run it and keep track of its state
* Loader for Shenzhen I/O levels to extract test inputs/outputs and test cases with data of these inputs/outputs;
* Loader for Shenzhen I/O solutions - just to simplify tests of this engine;
* ...

## How can I help?
If you know C# and can program using it:
1. Review the code! I would be happy to hear any feedback.
2. If you feel that something is terribly wrong in the code, please feel free to post an issue.
3. Just feel free to implement any of things from the 'To be implemented' list and create a PR.
4. Feel like some test cases for unit/integration tests are missing? Please don't hesitate to post a PR or an issue stating what's wrong.

If you are not a programmer (or don't know C# well), then you can help too!
1. Please contact me and send your level solutions. Any solutions will do - the dirtier and hackier your solutions are, the better! I promise to only use these solutions to verify that this engine works just like the engine of the original game - I won't redistribute them.
2. ...