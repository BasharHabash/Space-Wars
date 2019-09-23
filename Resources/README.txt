----------------------------------------------------------------------------------------------------
Authors:			Bashar Al Habash, Cole Perschon
Date:				November 17, 2017
----------------------------------------------------------------------------------------------------
																					Version      1.2
																	                           (PS9)
Initial Design Thoughts:
----------------------------------------------------------------------------------------------------
	1.) The database was desgined to be as split as possible so that desired information can be
		isolated.
		We have 4 tables:
		Games
		Players
		Scores
		Accuracies

		They are structured as follows:
		+-----------------------Game------------------------+  
		+ ID: INT-(PK,NN,AI) | Duration: VARCHAR(1024)-(NN) +
		+---------------------------------------------------+
		(ID is an autoincremented value identifying the game id, whereas Duration is a TIMESPAN.)

		+---------------------Players----------------+
		+ ID: INT-(PK,NN) | Name: VARCHAR(1024)-(NN) +
		+--------------------------------------------+
		(ID is the Ship.ID combined the HashCode of its' Game's Duration, to allow for searching players
		from a specified game.
		Once a proper ID for a player is provided, the rest of the information can be received via Name.)

		+---------------------Scores-----------------+
		+ Name: VARCHAR(1024)-(NN) | Score: INT-(NN) +
		+--------------------------------------------+
		(Name is connected with Score here.)

		+---------------------Accuracies----------------------+
		+ Name: VARCHAR(1024)-(NN) | Accuracy: VARCHAR(4)-(NN)+
		+-----------------------------------------------------+
		(Name is connected with Accuracy here.
		Accuracy is determined within Exit() in Program.cs)


	2.) Variables projFired, and projHit were added to Ship.cs. This was to track the amount of
		projectiles fired, and amount hit for each Ship.



Obsticles we faced:
----------------------------------------------------------------------------------------------------

	1.) Finding a way to implement the database saving function on the console application exit.
		We found a remedy for this from a website outlined above the following funciton within
		the code.


	2.) N/A



		Additional Notes:
----------------------------------------------------------------------------------------------------

	1.) Can become buggy with AIClients... Not all Ships being uploded to the database

	
	2.) Data! Woot!



++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++
++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++



                                                                                    Version      1.1
																	       (Inital release - Server)
Initial Design Thoughts:
----------------------------------------------------------------------------------------------------
	1.) So we first started on the connections, we added the two functions to our NetworkController, 
		the ServerAwaitingClientLoop and AcceptNewClient. We then moved to make our server code.
		In the server we made sure a client can connect to the server by just displaying client connected 
		using the new methods on our NetworkController. We then moved on to complete the handshake 
		by getting the name and the world size and making sure the client resizes accordingly. Receive
		and other starting information, unique ID, 0 velocity and adding it to the world. The world of the
		server is separate from the client. And we send the data of the world and name with sb appending.


	2.) We then moved on to try to display a ship onscreen, for this we made an update method that keeps
		sending informations of all the object in the world and updated to all the new changes done to them.
		We used the Json serialize to send an information of an object to the client. We then tried to mess 
		around by displaying different objects making sure they all work. Then we tried to handle the 
		commands coming back from the client. In our handle data, we get the commands and store them
		in a string, check the commands it has and have corresponding flags that are unique for each ship.
		then moved the ship with constant velocity at first with flag check and made sure it works.


	3.) We then made the ships spawn randomly using the world size and also wrap around which are simple.
		Next, we focused on the motion mechanics, things got a little complicated here. All the mechanics
		are in update with helper methods to compute the gravity and thrust, the only two forces. And 
		methods for rotating right and left with a certain degree. In update we have three main foreach loops
		one for star, ship and projectile. Star loops appends the star information and handles ship and 				
		projectile collision with the star by adding them to dead list for ships and projectile. Ship loop 					
		handles the ship movement, gravity is applied in all situations but in thrust we add the thrust force 				
		and make adjustments to the location and direction in each condition. On firing (space bar) it creates 			
		a new projectile and sets its initial conditions and then calculated its velocity, making sure it is always 			
		faster than the ship, then adding projectiles to world. We also handle the ship collision in the ship 				
		loop for efficiency and calculate the hp and add the ships that scores to a list, and dead projectiles to 			
		a list. Then append the ship information. Finally the projectile foreach loop only with the projectiles 				
		dying if they exit the world size and append its information.


	4.) Lastly we have some adjustments, we remove the dead ship and projectile and update the scores and 
		send all the appended info to the client. We also made update run every TimePerFrame using a 
		stopwatch, and have and XML reader that reads all the settings for the server; world size, Time per 
		per frame projectile delay, … etc. We also added a counter timer in ships for the projectile delay and
		another for the re spawn delay in ship properties. We also for ships that die have a rest ship to 
		original settings and random location but keeping id and score. And finally added a game mode.



Obsticles we faced:
----------------------------------------------------------------------------------------------------

	1.) We had an extra Sun property making the client unable to read the send sb message and thus not
		displaying the ships.


	2.) We were having a lot of lag and our memory would go really high and this was because we forgot to 
		add the stopwatch for the time per frame.


	3.) We had a problem with multiple clients controlling each other and not being able to switch controls
		between clients, fixed this by making the command flags a property of each ship.


	4.) We had a problem with the projectile sticking to the world only when adding a new client, a problem 
		with our server and also the projectiles were firing too fast thus we added a projectile delay property


	5.) We had a problem with projectile dying and ships dying thus making list for dead projectiles and dead
		ships and another for scoring ships.



		Additional Notes:
----------------------------------------------------------------------------------------------------

	1.) The game mode we implemented is activated via settings.xml. The game mode is that the star will change 
		its position to a random place and if the ship dies by hitting the star its score will go down by one point.


	2.) Ai Clients sometimes do not respawn...

	
	3.) No Unit Testing...


	4.) General inefficiency due to disconnected clients remaining in-game after they exit...



++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++
++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++



																					Version      1.0
																	  (Inital release - Client Only)
Initial Design Thoughts:
----------------------------------------------------------------------------------------------------
	1.) We began with collecting code from lectures and labs to give us a head start and a better 
		understanding of this assignment. Network Controller was the first thing we worked on; 
		we made sure that we were pleased with it and that it is works perfectily and independantly 
		from the SpaceWar game, Lecture 20 was a great help in this 

	2.) We then made the classes: Ship, Projectile and Star using json, after some research and 
		getting a better understanding of how json works it was simple to do those classes with
		each having properties and constructors, with public getter and private setter. Also,
		the Vector2D class which also uses json was giving to us by the Prof. Next was the World
		class, we made dictionaries for ship star and projectile and made them public as we were
		using the getters and setter of them.

	3.) Then we made the Resourses project, which contains all the graphics that we used in the 
		project and this README :) as well as all the refrences.

	4.) Lastly was the View project; this is were the bulk of the work is, we decided to makes the
		user interface with the server and name connect button, which was simple. Then we decided to
		to make two panels, on for the game and the other for the scoreboard. The first milestone was
		to make sure that we are able to connect to the surver using just the user interface using
		the network controller. In the game panel we had the drawers for the ship, projectiles and
		the sun and with help from lab 10 and the network diagram we got the on paint working and it
		was comunicating with the surver using the process message method. At this point we had 
		everything displaying so nect we worked on controlling and keyevents and send a message to the
		surver so ships can move and shoot. Using alive and removing projectile and ships things looked
		good. The scorepanel was next, we passed in the world so we can lock it and avoid errors and made
		the drawers, one for the name and score and another for the healthbar, using the ships properties
		we made the name and score easily and for the health bar we used two rectangles and ships hp
		to control the width, we also had an offset for both so when multiple players are in th game
		they all show up on the panel.


Obsticles we faced:
----------------------------------------------------------------------------------------------------

	1.) We faced a problem with modification error that was resolved by using the lock. In the lab we 
		were taught about it so it came to mind. We locked the world in three places to make sure we 
		dont get this error. We locked both the on paint methods in both panels and the Process message
		method, that way when all the modifications being done are locked.

	2.) Our ships, star and projectiles were all flashing, this took us a while to fix because we didnt 
		what was causing this. It turned out to be an easy fix, as we forgot to set the DoubleBuffer 
		to true.

	3.) Also the sun ship and were all showing up in the corner, this was a stupid mistake that costed 
		us time. We were passing in the wrong coordinates :( but we didnt even think we made that mistake,
		thus taking long time to catch it.

	4.) The ships and projectile were not doing anything after we implemented the code, the arrows would just
		keep contolling the textbox of the name, this was a focus problem, we fixed this by disabling the all
		the textboxes and buttons.

	5.) The projectiles and sun would just stick to the sun and not disapear, this was because we forgot to 
		remove them after they are not alive, we forgot that there was an alive property thus we didnt know what
		to do at the start :')


		Additional Notes:
----------------------------------------------------------------------------------------------------

	1.) We made the world dictionaries public because it would be useless to make it private and have public
		getter and setters.

	2.) We didnt remove the sun when it dies because we want the properties to stay on the score panel thus we 
		did not paint the ship when it is dead.

	3.) The controls help function is disabled after connecting, that is because we want the focus of the keys
		to be on the panels and not on the button.

	4.) We used an list of array of images to hold all the same colors of ship and projectile, and we made the 
		health bar color the same as the players ship and ship projectile color that was each player can easily 
		identify his ship with his health bar.