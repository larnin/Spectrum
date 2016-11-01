#### __!! ServerMod only work on the current stable version 4491 !!__  

ServerMod is a plugin that adds some commands to the server, accessible by the host and players.
All the commands are prefixed by '!'.
If you are a client and you have the plugin, you can use your commands by remplacing the '!' key to '%' 

Commands have 3 permission levels:
* __HOST__: Only the host can use the command
* __ALL__: All players can use the command
* __LOCAL__: Client-side command

Some commands with __ALL__ permission level can also be used when you are a client.  

__Playlist managing commands can't be used on trackmogrify mode (trackmogrify don't use playlist).__ 

# Command list (alphabetized):

#### Auto:
Permission: __HOST__  
Use: !auto  
Toggle the server auto mode.  
When auto mode is activated, the server will automatically jump to the next level when all players finish.  
If a level lasts longer than 15 minutes, the server continues to the next level.  
If the playlist ends, the server goes back to the lobby automatically.  
Auto mode doesn't work with Trackmogrify.

#### Autospec:
Permission: __LOCAL__  
Use: !autospec  
Toggle automatic spectating (useful when you go AFK or use auto mode).  
If you are the host and no players are online, the server will automatically return to the lobby.

#### Countdown:
Permission: __HOST__  
Use: !countdown  
Starts the default end-of-race countdown (60 seconds).  
&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;!countdown [time]  
Starts the end-of-race countdown with a time between 10 and 300 seconds.  
&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;!countdown stop  
Stops the current countdown (including the default game-initiated countdown).

#### Del:
Permission: __HOST__  
Use: !del [index]  
Removes the map at the entered index for the current playlist.  
The next map has an index of 1.

#### ForceStart:
Permission: __HOST__  
Use: !forcestart  
Forces the game to start regardless of the ready states of players in the lobby. Use with caution!

#### Help:
Permission: __ALL__ (can also be used as client)  
Use: !help  
Shows all the available commands.  
Commands with "(H)" can only be used by the host.  
Commands with "(L)" can only be used by the local player (the one who has the plugin).  
&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;!help [command name]  
Shows the help message of the specified command.

#### Level:
Permission: __ALL__  
Use: !level [keyword]  
Shows all levels in the host’s library that contain the entered keyword.  
&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;!level [filter]  
Shows all levels that match the filter.  
See bellow for more information about filters. 

#### List:
Permission: __ALL__ (can also be used as client)  
Use: !list  
Shows all connected clients and their IDs
    
#### Play:
Permission: __HOST__  
Use: !play [name]  
&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;!play [filter]  
Adds a level to the playlist in the first position (the next level to be played)  
If more than one level matches the name exactly, the first one is added.  
If only one level contains the name, it will be added to the playlist.  
If more than one level contains the name, it will display the matching levels (like !level).  
The filter works exactly like the list command.


#### Playlist:
Permission: __ALL__  
Use: !playlist  
Shows the 10 next levels in the playlist.  
The first one is the current level.

#### Scores:
Permission: __ALL__ (can also be used as client)  
Use: !scores  
Shows the in-game “Show Scores” list in the chat. In racing modes, it shows the distance to the finish for each player. In Stunt, it shows scores in eV, and in Reverse Tag, it shows each player’s bubble possession time.

#### Server:
Permission: __ALL__ (without parameters) - (can also be used as client)  
&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;__HOST__ (with parameters)  
Use: !server  
Shows the current server name.  
&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;!server private [password]  
Sets the server private with the given password.  
&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;!server public  
Sets the server public.  
&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;!server [name]  
Changes the name of the server.
    
#### Shuffle:
Permission: __HOST__  
Use: !shuffle  
Shuffles the current playlist.  Already-played levels will also be shuffled.  
This command resets the playlist index, placing the current level at the beginning of the playlist.

#### Spec:
Permission: __HOST__  
Use: !spec [id]  
&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;!spec [player name]  
Forces a player to spectate the game.

#### TimeLimit
Permission: __HOST__  
Use: !timelimit [value]  
Works like the official command, it changes the max time for the next reverse tag maps.  
The value must be between 30 and 1800 seconds (30 minutes)

# Filters
Commands __level__ and __play__ can use filters to allow you to search maps.
* __-name__ or __-n__ : Select maps that contains theses words on it's name.
* __-author__ or __-a__ : Only show you maps form the entred author 
* __-mode__ or __-m__ : Select map on one particular gamemode. If not specified, it use the current gamemode
    * Valid gamemodes are : sprint, challenge, tag, soccer, style, stun  
    _(Soccer mode don't work perfectly)_
* __-index__ or __-i__ : Use the map at the specified index (if you have 7 map on the result without that filter, -index 4 use the 4th map from theses 7 maps)  
The index filter can be written multiple times to select many maps.
* __-all__ : Only for the !play command, if you use this filter, all the found maps are added on the playlist  

#### Examples
__!play -a snowstate -m sprint -all__ : Adds all the maps that snowstate have created on the sprint gamemode to the playlist.  
__!list -name epic level__ : List all the levels on the current gamemode that contains "epic level" on their name. It's equivalent as __!list epic level__  
__!play -name up -mode challenge -index 1__ : Add the first level from challenge mode found that contains "up" in it's name.

# Author contacts :
Steam : http://steamcommunity.com/id/larnin/  
Discord : Nico#5480 (https://discord.gg/0SlqqvzfIbi6zhbY)

