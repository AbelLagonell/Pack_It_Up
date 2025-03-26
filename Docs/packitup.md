# Main Game Summary
The players are set in the middle of the heist and are tasked to complete it. Get as much money as you can and get out.
But there is an enemy in the midst one of them is an Informant for the police and will set trackers on the bags that the other players make 
After a time the cops will show up, signaling the scramble to move the bags out of the places where the players have it and get out.
Score will be tracked at the end and then the player's will know if they won or if the informant won.

# Phase 1
## Game State Variables:
- Entity:
  - Health (int)
  - Speed (float)
  - Detained (bool)
- Bag(s):
  - Monetary Value (int)
  - Max Weight (float)
  - Current Weight (float)
  - Tracked (bool)
- Items
  - Monetary Value (float)
  - Weight (float)
- Timer left (Before Cops show up)
- Weapon 
  - Damage (int)
- Score (int)
- Win State (Informant win or robber win)


## Player interaction with states:
### Player
They all have a bag at the start which then increases in Monetary value as a they steal items from around the map.
Once they reach the max amount of weight in their bag they cannot carry anymore.
They can increase the time left by doing task

### Player (Informant)
They can also steal items from the map but have an extra ability to mark bags for tracking by putting a tracker in them.
This tracker can also be placed on other bags to track them as well. 
Any bag that this player carries will be automatically tracked.
They can set off the silent alarm to decrease the amount of time

## Strategies to win Game
Players will want to collect the most money in their bags so that they can get away with the most money 
Players with the informant role will win if the money that they tracked is greater than the money stolen
Players can also call a meeting to try to detain who they think is the informant
Players with the informant role will win if they arrest everyone

# Phase 2
## Features
- Players will be able to collect items and money from various parts of the map until their bag gets full
- Players will be able to change out bags that they have so that they can continue getting more items
- Players will be able to delay the cops showing up by doing different tasks around the map
- Players will be able to shoot at other players/NPCs
- Players will be able to call a group up to discuss and try to find the informant and try to detain them
- Informants will be able to track bags as they see them on a cooldown to make them count against the other players.
- Informants will be able to rush the cops by doing various different tasks, like setting off the silent alarm.
- Informants will be able to arrest other players taking them out of the game
- Civilians, NPCs, will be put in a section of the map that the players will have to control to make sure they don't impede the players
- Civilians will have a chance of fighting back and pressing either the silent alarm or actively fighting the players as they collect money.
- Dead Civilians will speed up the timer by a factor not a set amount
- Once the cops show up the players are directed to LEAVE, getting their bags into a designated location and trying to avoid the cops as they storm the building.
- Cops will come in from the main entrance and direct themselves to the different rooms
- Cops can take bags from the players once they spot one.
- Cops can shoot the players if they see them
- Cops will continuously show up in the map as they try to kill players.
- Score will be counted once all players are out of the location, bag's monetary value will be counted up and that is the score for the players
- Score for the Informant is based on the monetary value of the bags taken
- Score will be increased for the informant for each bag that the cops take that is tracked otherwise the bag will not be counted.


