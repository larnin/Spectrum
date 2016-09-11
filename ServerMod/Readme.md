/!\ ServerMod only work on the current stable version 4491 /!\

ServerMod is a plugin that adds some commands to the server, accessible by the host and players.
All the commands are prefixed by '!'.

Commands have 3 permission levels:
    * HOST: Only the host can use the command
    * ALL: All players can use the command
    * LOCAL: Client-side command
Some commands with "ALL" permission level can also be used when you are a client.

Command list (alphabetized):
Auto:
    Permission: HOST
    Use: !auto
    Toggle the server auto mode.
    When auto mode is activated, the server will automatically jump to the next level when all players finish.
    If a level lasts longer than 15 minutes, the server continues to the next level.
    If the playlist ends, the server goes back to the lobby automatically.
    Auto mode doesn't work with Trackmogrify.

Autospec:
    Permission: LOCAL
    Use: !autospec
    Toggle automatic spectating (useful when you go AFK or use auto mode).
    If you are the host and no players are online, the server will automatically return to the lobby.

Countdown:
    Permission: HOST
    Use: !countdown
    Starts the default end-of-race countdown (60 seconds).
         !countdown [time]
    Starts the end-of-race countdown with a time between 10 and 300 seconds.
         !countdown stop
    Stops the current countdown (including the default game-initiated countdown).

Del:
    Permission: HOST
    Use: !del [index]
    Removes the map at the entered index for the current playlist.
    The next map has an index of 1.

ForceStart:
    Permission: HOST
    Use: !forcestart
    Forces the game to start regardless of the ready states of players in the lobby. Use with caution!

Help:
    Permission: ALL (can also be used as client)
    Use: !help
    Shows all the available commands.
    Commands with "(H)" can only be used by the host.
    Commands with "(L)" can only be used by the local player (the one who has the plugin).
         !help [command name]
    Shows the help message of the specified command.

Level:
    Permission: ALL
    Use: !level [keyword]
    Shows all levels in the host’s library that contain the entered keyword.
    
List:
    Permission: ALL (can also be used as client)
    Use: !list
    Shows all connected clients and their IDs
    
Name:
    Permission: LOCAL
    Use: !name [new name]
    Changes your chat name.
    
Play:
    Permission: HOST
    Use: !play [name]
    Adds a level to the playlist in the first position (the next level to be played).
    If more than one level matches the name exactly, the first one is added.
    If only one level contains the name, it will be added to the playlist.
    If more than one level contains the name, it will display the matching levels (like !level).
    
Playlist:
    Permission: ALL
    Use: !playlist
    Shows the 10 next levels in the playlist.
    The first one is the current level.

Scores:
    Permission: ALL (can also be used as client)
    Use: !scores
    Shows the in-game “Show Scores” list in the chat. In racing modes, it shows the distance to the finish for each player; in Stunt, it shows scores in eV, and in Reverse Tag, it shows each player’s bubble possession time.

Server:
    Permission: ALL (without parameters) - (can also be used as client) 
           HOST (with parameters)
    Use: !server
    Shows the current server name.
         !server private [password]
    Sets the server private with the given password.
         !server public
    Sets the server public.
         !server [name]
    Changes the name of the server.
    
Shuffle:
    Permission: HOST
    Use: !shuffle
    Shuffles the current playlist.  Already-played levels will also be shuffled.
    This command resets the playlist index, placing the current level at the beginning of the playlist.

Spec:
    Permission: HOST
    Use: !spec [id]
         !spec [player name]
    Forces a player to spectate the game.

Author contacts :
Steam : http://steamcommunity.com/id/larnin/
Discord : Nico#5480 (https://discord.gg/0SlqqvzfIbi6zhbY)

