# Oxalis Coding Test

## 1. Overview
The test comes in two parts: the client is a simplified prototype of the classic Space Invaders game, where only the basic gameplay is implemented; the server is a JavaScript application that supports simple HTTP message exchange. See  <a href="https://www.youtube.com/watch?v=MU4psw3ccUI">Classic Space Invaders</a>  for gameplay references.


The test consists of fixing and refactoring the existing source code as well as writing additional code to achieve the tasks outlined below. Client-specific tasks focus on C# code and Unity engine, Generalist tasks touch basic network code, and Server tasks aim at testing use of JavaScript and backend architecture. In both applications, refactoring the existing code is the first step. It is up to you to determine the right balance between refactoring and developing.


No task described below is considered mandatory: choose what you wish to complete based on how much time you find appropriate investing, and technologies you are comfortable with. If you would rather not touch JavaScript code, all client side tasks can still be completed, and vice versa. Only the Generalist section tasks require working with both applications, but once again, you are free to skip any task you would rather not do. 


Think of the test as a production development project. We want your version of a tidy code solution that can grow and stay easy to manage. Show you understand good practices in programming and game development. Think about how classes relate and how to make the design better. If you can, explain your thoughts and goals in comments.

## 2. Client-specific tasks:

2.1. Currently, invaders' lines break when descending down closer to the player. Make sure invaders’ are moving correctly throughout each round.

2.2. Make invaders shoot at the player. Being hit by a projectile must lead to immediate Game Over or number of lives remaining reduced, if you decide to introduce such counter.

2.3. Introduce different invader types. Make them differ in visuals and behaviour in any way you like, maybe shoot differently, or have more health.

2.4. Assign points value to invaders (or individual invader types, if you completed the previous task), and track current score in the UI. Reset the score between rounds.

2.5. Allow configuring player weapon tiers: define score values required to unlock the next weapon, and switch the player to it automatically when the score is reached. Feel free to come up any weapon behaviour you like. Examples of such are a fast-shooting machine gun-like armament, wider 'AoE' projectile, or a short-burst laser that deals damage instantly but cools down for longer than other weapons.

## 3. Generalist tasks:


3.1. Retrieve round time from the server during game sessions, show it in the UI, and trigger game over when the round is over. You can find a draft of the score retrieval method in the GameController class.

3.2. Implement player name input and submit the provided name to the server app. Support entering it upon starting a new game, and forbid progressing to gameplay until a name is submitted.

3.3. If invaders firing at the player is supported, make the client app ask the server for the current invaders’ aggression level. Look for the method in gameInstance.js. This value should control how many invaders shoot at the player ship at once. At the beginning of a round, it is set to return 0, which means that invaders cannot shoot. If the value is 3, then maximum three invader ships can shoot at the same time.

3.4. Currently, client code combines Unity Web Requests and .Net HTTP classes. Refactor it so that only one solution is in use.

## 4. Server tasks:


4.1. Reconsider how network routes are declared and implemented. Consider optimizing or refactoring how client and server communicate.

4.2. Support starting a new round when the client has lost, won, or the timer expired.

4.3. Support accepting player score from clients and store them in a leaderboard data structure. Choose any file storage format you prefer for the purpose. Player name and score must be saved, and ordered leaderboard retrieval allowed on request.

4.4. Support starting the game client app from the server upon booting with a -autostart parameter. When the parameter is specified, launch the game client automatically, and track its process metrics (CPU load, memory consumption, ping). Flush the metrics into a CSV file. Provide a method for retrieving the file over HTTP. You will need to use Unity editor to produce a client build.

## 5. Time limit & submission

Feel free to take as much time as you need for the test, but please keep track of how long you spend on each task and share that with us along with your test results. When you are ready to submit the test, send us the link to your repository with the entire project (both applications). Include your notes, if any, as a readme file.
