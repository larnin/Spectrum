Spectrum.Plugins.ServerMod is a plugin that adds some commands to the server, accessible by the host and players.  
All the commands are prefixed by '!'.  
If you are a client and you have the plugin, you can use some of your commands by replacing the '!' key to '%'.

Commands have 3 permission levels:
* __HOST__: Only the host can use the command
* __ALL__: All players can use the command
* __LOCAL__: Client-side command

Some commands with __ALL__ permission level can also be used as a client.  

__Playlist managing commands can't be used on trackmogrify mode (trackmogrify doesn't use playlists).__

# Command list (alphabetized):

#### Auto:
Permission: __HOST__  
Use: !auto  
Toggle the server auto mode.  
When auto mode is activated, the server will automatically jump to the next level when all players finish.  
If a level lasts longer than 15 minutes, the server continues to the next level.  
The maximum time on a level can be configured with the `autoMaxTime` settings.  
If the playlist ends, the server shuffles the playlist automatically.  
Auto mode doesn't work with Trackmogrify.  
At the end of a map, if votes are enabled, players can choose between the 3 next maps (or restart the current) on the playlist the one they wants to play next.  
If there are not enough players, the server will wait for players to join.  
The minimum players is configurable with the `autoMinPlayers` setting.


#### Autospec:
Permission: __All__ (can also be used as client)  
Use: !autospec  
Toggles automatic spectating (useful when you go AFK or use auto mode).  
If the setting `autoSpecReturnToLobby` in the settings file is `true`: If you are the host and no players are online, the server will automatically return to the lobby.  
By default, `autoSpecReturnToLobby` is `false`.  

Permission: __HOST__  
Use: !autospec [player name]  
Toggles automatic spectating for a specific player.  

#### Clear:
Permission: __HOST__  
Use: !clear  
Removes everything on the playlist.

#### Countdown:
Permission: __HOST__  
Use: !countdown  
Starts the default end-of-race countdown (60 seconds).  
&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;!countdown [time]  
Starts the end-of-race countdown with a time between 10 and 300 seconds.  
&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;!countdown stop  
Stops the current countdown (including the default game-initiated countdown).

#### Date
Permission: __ALL__ (can also be used as client)  
Use: !date  
Shows the date and time of the owner of the plugin.

#### Del:
Permission: __HOST__  
Use: !del [index]  
Removes the map at the entered index from the current playlist.  
The next map has an index of 1.

#### Dels:
Permission: __HOST__  
Use: !dels [indexStart] [indexEnd]
Removes the maps between the entered start and end indexes from the current playlist.  
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
Permission: __ALL__  (can also be used as client)  
Use: !level [keyword]  
Shows all levels in the host’s library that contain the entered keyword.  
&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;!level [filter]  
Shows all levels that match the filter.  
See below for more information about filters.  
If this command is used as a client with __%level__, it search on your library.

#### List:
Permission: __ALL__ (can also be used as client)  
Use: !list  
Shows all connected clients and their IDs

#### Load:
Permission: __HOST__  
Use: !load  
Shows all the available playlists.  
&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;!load [playlist name]  
Removes all the levels on the current playlist and load a new one.  
If you are not on the lobby, the current level is added at the start of the new playlist.

#### Play:
Permission: __HOST__  
&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;__ALL__ (if players can add maps)  
Use: !play [name]  
&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;!play [filter]  
Adds a level to the playlist in the first position (the next level to be played)  
If more than one level matches the name exactly, the first one is added.  
If only one level contains the name, it will be added to the playlist.  
If more than one level contains the name, it will display the matching levels (like !level).  
The filter works exactly like the list command.  
If the `play` setting is true, players can add levels directly, one at a time.  
If the `playVote` setting is true, players can vote for multiple levels at once: `!play <level>` will act as an alias for `!vote y play <level>`  
`playVote` overrides `play`. The host will keep the default functionality for the `!play` command, always.

#### Players:  
Permission: __ALL__ (can also be used as client)  
Use: !players [player search query]  
Shows all players in the game that match the search query.

#### Playlist:
Permission: __ALL__  
Use: !playlist  
Shows the 10 next levels in the playlist.  
The first one is the current level.

#### Plugin:
Permission: __ALL__  
Use: !plugin  
Shows all the players that have server mod installed and the version.  

#### Rip:
Permission: __ALL__  
Use: !rip  
Prints a losing sentence.

#### Save:
Permission: __HOST__  
Use: !save [name]  
Save the current playlist on a file on the game playlist directory.  
It can be loaded again with the __!load__ command, or o=in the lobby.  

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

#### Settings:
Permission: __HOST__  

* `!settings reload`  
Reloads the settings from file.  
* `!settings summary`  
View the value of all settings.  
* `!auto` Settings
  * `!settings autoVote <true/false>`  
Whether or not players can vote for the next map at the end of a level in auto mode  
Default: False  
  * `!settings autoShuffle <true/false>`  
Whether or not the playlist should be shuffled when it finishes in auto mode  
Default: True  
  * `!settings autoUniqueEndVotes <true/false>`  
Whether or not levels should be re-ordered after votes so the next vote has all-new options  
Default: True  
  * `!settings autoMsg <text>`  
The message to display when the level advances. `clear` to turn off.  
Default:   
  * `!settings autoMinPlayers <number>`  
How many players auto mode needs before it will advance to the next level  
Default: 1  
  * `!settings autoMaxTime <seconds>`  
Maximum amount of time a level can run for in auto mode before it advances to the next  
Default: 900  
  * `!settings autoSkipOffline <true/false>`  
Whether or not levels that are not official levels and are not workshop levels should be skipped over in auto mode  
Default: True  
  * `!settings autoVoteText <text>`  
The text to display for level-end votes. Formatting options: %NAME%, %DIFFICULTY%, %MBRONZE%, %MSILVER%, %MGOLD%, %MDIAMOND%, %AUTHOR%, %STARS%, %STARSINT%, %STARSDEC%  
Default: %NAME% [A0A0A0]by %AUTHOR%[-]  
* `!autospec` Settings
  * `!settings autoSpecReturnToLobby <true/false>`  
Whether or not to return to the lobby if everyone leaves while auto-spectate is running.  
Default: False  
  * `!settings autoSpecAllowPlayers <true/false>`  
Whether or not players/clients can use the !autospec command  
Default: True  
* `!play` Settings
  * `!settings play <true/false>`  
Whether or not players can add maps using !play  
Default: False  
  * `!settings addOne <true/false>`  
Limit players to adding only one map with !play  
Default: True  
  * `!settings playVote <true/false>`  
For non-hosts, !play will use !vote play instead of adding maps directly. The host still adds maps directly.  
Default: False  
* `!update` Settings
  * `!settings updateCheck <true/false>`  
Whether or not to show updates to ServerMod when a server is started  
Default: True  
* `!welcome` Settings
  * `!settings welcome <text>`  
The welcome message to show to players. `clear` to turn off. `%USERNAME%` is replaced with the player's name.  
Default:   
* `!vote` Settings
  * `!settings voteSystem <true/false>`  
Whether or not players can use votes with !vote  
Default: False  

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

#### Update
Permission: __HOST__  
Use: !update  
Display any newer versions of the ServerMod that are available.

#### Vote:
Permission: __ALL__
Use: !vote [y/n/i] [vote type] [value]
Allows voting for various things, with configurable passing thresholds:
* `!vote [y/n/i] skip` Vote to skip the current level
* `!vote [y/n/i] stop` Vote to stop the countdown
* `!vote [y/n/i] play [level name]` Vote to play a map
`[y/n/i]` can be left off, and the vote is done as a `yes` vote:
* `!vote skip` is `!vote y skip`
* `!vote play <map>` is `!vote y play <map>`

##### Examples:
`!vote skip` Vote to skip the current level  
`!vote y skip` Vote to skip the current level  
`!vote n skip` Cancel your vote to skip the level  
`!vote i stop` View the vote pass threshold, votes made, and votes needed to stop the countdown  
`!vote play inferno` Vote to play the level "Inferno"  
`!vote play -a krispy` Vote to play all of Krispy's maps  
Votes for maps are counted individually for every map, not by the command used.  

#### VoteCtrl:
Permission: __HOST__  
Use: !votectrl [vote type] [percent]  
Vote types are `skip`, `stop`, and `play`.  
`percent` should be a number between 0 and 100. Numbers above 100 effectively disable the vote.  

#### Welcome:
Permission: __ALL__  
Use: !welcome  
Shows the welcome message again  

#### Win
Permission: __ALL__  
Use: !win  
Prints a random winning sentence.

# Filters
Commands __level__ and __play__ can use filters to allow you to search maps.
* __-name__ or __-n__ : Select maps that contains theses words on it's name. If a map with an exact-match name is found, only that one map is returned.
* __-author__ or __-a__ : Only show you maps form the entred author
* __-mode__ or __-m__ : Select map on one particular gamemode. If not specified, it use the current gamemode
    * Valid gamemodes are : sprint, challenge, tag, soccer, style, stun  
    _(Soccer mode don't work perfectly)_
* __-index__ or __-i__ : Use the map at the specified index (if you have 7 map on the result without that filter, -index 4 use the 4th map from theses 7 maps)  
The index filter can be written multiple times to select many maps.
* __-l__ or __-last__ : Use the result of the last !level or !play command
* __-p__ or __-page__ : Show the specified page of level (10 level per page)
* __-all__ : Only for the !play command, if you use this filter, all the found maps are added on the playlist  
* __-regex__ or __-r__ : Choose maps using case-sensitive C# regex.

#### Examples
__!play -a snowstate -m sprint -all__ : Adds all the maps that snowstate have created on the sprint gamemode to the playlist.  
__!list -name epic level__ : List all the levels on the current gamemode that contains "epic level" on their name. It's equivalent as __!list epic level__  
__!play -name up -mode challenge -index 1__ : Add the first level from challenge mode found that contains "up" in it's name.  
__!level -last -p 4__ : Shows the 4th page (result 30 to 39) of the last command.

# Settings
When the plugin is started for the first time, it generate a setting file that look like this : (in json)
```
{
    "updateCheck": true,
	"playersCanAddMap" : false,
	"addOneMapOnly" : true,
    "playIsVote": false,
	"allowVoteSystem" : false,
	"autoSpecReturnToLobby" : false,
	"autoSpecAllowPlayers" : true,
	"welcome" : "",
	"voteNext" : false,
	"autoAdvanceMsg" : "",
	"autoMinPlayers" : 1,
	"autoMaxTime" : 900,
    "autoShuffleAtEndOfPlaylist": true,
    "autoUniqueEndVotes": false,
	"autoSkipOffline" : true,
	"autoVoteText" : "%NAME% [A0A0A0]by %AUTHOR%[-]"
	"voteSystemThresholds" : {
		"skip" : 0.7,
		"stop" : 0.7,
		"play" : 0.55,
		"kick" : 0.9,
		"count" : 0.55
	},
	"win" : [
		"ALL RIGHT!",
		"YEAH!"
	],
	"rip" : [
		"ACCESS VIOLATION!",
		"AW, THAT'S TOO BAD!",
		"YOU'RE OUT OF CONTROL!"
	]
}
```

* `"voteNext" :  <true/false>`,  
Whether or not players can vote for the next map at the end of a level in auto mode  
Default: False  
* `"autoShuffleAtEnd" :  <true/false>`,  
Whether or not the playlist should be shuffled when it finishes in auto mode  
Default: True  
* `"autoUniqueVotes" :  <true/false>`,  
Whether or not levels should be re-ordered after votes so the next vote has all-new options  
Default: True  
* `"autoAdvanceMsg" :  <text>`,  
The message to display when the level advances. `clear` to turn off.  
Default:   
* `"autoMinPlayers" :  <number>`,  
How many players auto mode needs before it will advance to the next level  
Default: 1  
* `"autoMaxTime" :  <seconds>`,  
Maximum amount of time a level can run for in auto mode before it advances to the next  
Default: 900  
* `"autoSkipOffline" :  <true/false>`,  
Whether or not levels that are not official levels and are not workshop levels should be skipped over in auto mode  
Default: True  
* `"autoVoteText" :  <text>`,  
The text to display for level-end votes. Formatting options: %NAME%, %DIFFICULTY%, %MBRONZE%, %MSILVER%, %MGOLD%, %MDIAMOND%, %AUTHOR%, %STARS%, %STARSINT%, %STARSDEC%  
Default: %NAME% [A0A0A0]by %AUTHOR%[-]  
* `"autoSpecReturnToLobby" :  <true/false>`,  
Whether or not to return to the lobby if everyone leaves while auto-spectate is running.  
Default: False  
* `"autoSpecAllowPlayers" :  <true/false>`,  
Whether or not players/clients can use the !autospec command  
Default: True  
* `"playersCanAddMap" :  <true/false>`,  
Whether or not players can add maps using !play  
Default: False  
* `"addOneMapOnly" :  <true/false>`,  
Limit players to adding only one map with !play  
Default: True  
* `"playIsVote" :  <true/false>`,  
For non-hosts, !play will use !vote play instead of adding maps directly. The host still adds maps directly.  
Default: False  
* `"updateCheck" :  <true/false>`,  
Whether or not to show updates to ServerMod when a server is started  
Default: True  
* `"welcome" :  <text>`,  
The welcome message to show to players. `clear` to turn off. `%USERNAME%` is replaced with the player's name.  
Default:   
* `"allowVoteSystem" :  <true/false>`,  
Whether or not players can use votes with !vote  
Default: False  
* `"voteSystemThresholds" :  <option>`,  
The thresholds at which each vote passes  
* `"win" :  <option>`,  
* `"rip" :  <option>`,  




# Author contacts
Steam : [Corecii](http://steamcommunity.com/id/corecii/)  
Discord : Corecii#3019  

### Note: The [original version of ServerMod](https://github.com/larnin/Spectrum) is by Nico
Nico can be reached with the following contacts.  
Steam : [http://steamcommunity.com/id/larnin/](http://steamcommunity.com/id/larnin/)  
Discord : Nico#5480 ([https://discord.gg/0SlqqvzfIbi6zhbY](https://discord.gg/0SlqqvzfIbi6zhbY))
