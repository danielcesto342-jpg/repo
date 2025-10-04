**General Information:**
*   All commands are prefixed with `!` (this is the `CommandStarterCharacter` from `ChatCommandManager`).
*   Most commands are only available if "cheats" are enabled on the server. The `SuicideCommand` is an exception and is available even if cheats are disabled.
*   Command arguments are generally space-separated.

Summary of each command:

---

### `!ad`
*   **Syntax:** `!ad bonusAd`
*   **Explanation:** Adds a flat amount of bonus Attack Damage (AD) to your champion.
*   **Example:** `!ad 50` (Adds 50 flat bonus AD to your champion)

---

### `!ap`
*   **Syntax:** `!ap bonusAp`
*   **Explanation:** Adds a flat amount of bonus Ability Power (AP) to your champion permanently.
*   **Example:** `!ap 75` (Adds 75 flat bonus AP to your champion)

---

### `!armor`
*   **Syntax:** `!armor bonusArmor`
*   **Explanation:** Adds a flat amount of bonus Armor to your champion permanently.
*   **Example:** `!armor 100` (Adds 100 flat bonus Armor to your champion)

---

### `!as`
*   **Syntax:** `!as bonusAs (percent)`
*   **Explanation:** Increases your champion's bonus Attack Speed by a specified percentage. The value is treated as a percentage (e.g., 50 means +50%).
*   **Example:** `!as 50` (Increases bonus attack speed by 50%)

---

### `!cc`
*   **Syntax:** `!cc ccType duration`
*   **Explanation:** Applies a specific crowd control effect to your champion for the specified duration in seconds.
*   **Available CC Types:** stun, silence, blind, slow, fear, taunt, root (or snare), suppression (or suppress)
*   **Examples:** 
    *   `!cc stun 3` (Stuns your champion for 3 seconds)
    *   `!cc blind 5` (Blinds your champion for 5 seconds)
    *   `!cc root 2.5` (Roots your champion for 2.5 seconds)

---

### `!changeteam`
*   **Syntax:** `!changeteam teamNumber`
*   **Explanation:** Changes your champion's team to the specified team number. The `teamNumber` will be mapped to an internal `TeamId`.
*   **Example:** `!changeteam 1` (Changes your champion to team 1, assuming 1 is a valid team ID like Order/Blue)

---

### `!ch`
*   **Syntax:** `!ch championName`
*   **Explanation:** Changes your current champion to the specified `championName`. Your position and team are preserved. The old champion object is removed and a new one is created.
*   **Example:** `!ch Ashe` (Changes your champion to Ashe)

---

### `!clearslows`
*   **Syntax:** `!clearslows`
*   **Explanation:** Removes all movement speed slows currently affecting your champion.
*   **Example:** `!clearslows`

---

### `!cdr`
*   **Syntax:** `!cdr bonusCdr`
*   **Explanation:** Adds bonus Cooldown Reduction (CDR) to your champion. The input value is treated as a percentage (e.g., 20 for 20% CDR).
*   **Example:** `!cdr 20` (Grants 20% Cooldown Reduction)

---

### `!coords`
*   **Syntax:** `!coords`
*   **Explanation:** Displays your champion's current X, Y, and Height (Z) coordinates, as well as movement direction information if currently pathing. The output is sent to the system chat.
*   **Example:** `!coords`

---

### `!debugmode`
*   **Syntax:** `!debugmode [mode]`
*   **Explanation:** Toggles various visual debugging overlays. If no mode is specified, it shows the current debug mode.
    *   `none`: Disables all debug visualizations.
    *   `self`: Shows debug info (pathfinding radius, waypoints) for your champion.
    *   `champions`: Shows debug info for all champions.
    *   `minions`: Shows debug info for all minions.
    *   `projectiles`: Shows debug info (collision radius, path) for spell projectiles.
    *   `pathfinding`: Shows debug info related to pathfinding around your champion, including navigation grid cells and nearby units.
    *   `all`: Shows debug info for all game objects.
*   **Examples:**
    *   `!debugmode self` (Toggles debug visualization for your champion)
    *   `!debugmode all` (Toggles debug visualization for all objects)
    *   `!debugmode none` (Disables all debug visualizations)

---

### `!gcstats`
*   **Syntax:** `!gcstats`
*   **Explanation:** Displays garbage collection statistics and memory usage information, including generation sizes, collection counts, GC mode, and working set memory.
*   **Example:** `!gcstats` (Shows current memory usage and GC statistics)

---

### `!gold`
*   **Syntax:** `!gold goldAmount`
*   **Explanation:** Adds the specified amount of gold to your champion.
*   **Example:** `!gold 5000` (Gives your champion 5000 gold)

---

### `!health`
*   **Syntax:** `!health maxHealth`
*   **Explanation:** Adds a flat amount to your champion's maximum health.
*   **Example:** `!health 1000` (Increases your champion's max health by 1000)

---

### `!help`
*   **Syntax:** `!help`
*   **Explanation:** Lists all available chat commands in the system chat. It will also notify if chat commands (cheats) are disabled for the current game.
*   **Example:** `!help`

---

### `!hotreload`
*   **Syntax:** `!hotreload 0 (disable) / 1 (enable)`
*   **Explanation:** Enables or disables the script hot reloading feature for the server.
*   **Examples:**
    *   `!hotreload 1` (Enables script hot reloading)
    *   `!hotreload 0` (Disables script hot reloading)

---

### `!inhib`
*   **Syntax:** `!inhib`
*   **Explanation:** Spawns a "Worm" minion (using "BasicJungleMonsterAI") at your champion's current position.
*   **Example:** `!inhib`

---

### `!junglespawn`
*   **Syntax:** `!junglespawn`
*   **Explanation:** This command currently logs a message and sends a "Jungle Spawned!" system chat message.
*   **Example:** `!junglespawn`

---

### `!kill`
*   **Syntax:** `!kill minions`
*   **Explanation:** If the argument is "minions", this command will kill all minions currently on the map.
*   **Example:** `!kill minions`

---

### `!level`
*   **Syntax:** `!level level`
*   **Explanation:** Sets your champion's level to the specified `level`. The level must be higher than your current level and not exceed the game mode's maximum level.
*   **Example:** `!level 18` (Sets your champion's level to 18, if valid)

---

### `!mana`
*   **Syntax:** `!mana maxMana`
*   **Explanation:** Adds a flat amount to your champion's maximum mana and also increases current mana by the same amount.
*   **Example:** `!mana 500` (Increases max mana by 500 and gives 500 current mana)

---

### `!mobs`
*   **Syntax:** `!mobs teamNumber`
*   **Explanation:** Pings (danger ping) all minions and neutral minions belonging to the specified `teamNumber` on the map, visible to the user who issued the command.
*   **Example:** `!mobs 2` (Pings all mobs on team 2)

---

### `!model`
*   **Syntax:** `!model modelName`
*   **Explanation:** Changes the visual model of your champion to the specified `modelName`. This typically doesn't change champion abilities, just appearance.
*   **Example:** `!model Teemo` (Changes your champion's appearance to Teemo's model)

---

### `!mr`
*   **Syntax:** `!mr bonusMr`
*   **Explanation:** Adds a flat amount of bonus Magic Resist (MR) to your champion permanently.
*   **Example:** `!mr 80` (Adds 80 flat bonus MR to your champion)

---

### `!newcommand`
*   **Syntax:** `!newcommand`
*   **Explanation:** This appears to be a test or placeholder command. When executed, it sends an info message to chat and then removes itself from the list of available commands.
*   **Example:** `!newcommand`

---

### `!packet`
*   **Syntax:** `!packet XX XX XX...`
*   **Explanation:** Sends a raw debug network packet. Each `XX` is a hexadecimal byte value. The special string "netid" can be used in place of a byte, and it will be replaced with your champion's network ID.
*   **Example:** `!packet C1 netid 0A FF` (Sends a packet starting with 0xC1, then your champion's NetID, then 0x0A, then 0xFF)

---

### `!particle`
*   **Syntax:** `!particle <particle_name.troy>`
*   **Explanation:** Spawns the specified particle effect (e.g., `LevelUp_Yellow.troy`) attached to your champion.
*   **Example:** `!particle Global_Heal.troy`

---

### `!playanim`
*   **Syntax:** `!playanim animationName`
*   **Explanation:** Plays the specified animation (e.g., "Dance", "Attack1") on your champion.
*   **Example:** `!playanim Dance`

---

### `!rainbow`
*   **Syntax:** `!rainbow [alpha] [speed]`
*   **Explanation:** Toggles a rainbow tint effect for your champion's team.
    *   `alpha` (optional): A float value (0.0 to 1.0) for the transparency of the tint. Default is 0.5.
    *   `speed` (optional): A float value for the speed of color change (in seconds per color step). Default is 0.25 (meaning 250ms delay).
*   **Examples:**
    *   `!rainbow` (Toggles rainbow with default alpha 0.5 and speed 0.25)
    *   `!rainbow 0.8` (Toggles rainbow with alpha 0.8)
    *   `!rainbow 0.5 0.1` (Toggles rainbow with alpha 0.5 and speed 0.1)

---

### `!reloadscripts`
*   **Syntax:** `!reloadscripts`
*   **Explanation:** Attempts to reload all game scripts. An info message will indicate success or failure.
*   **Example:** `!reloadscripts`

---

### `!revive`
*   **Syntax:** `!revive`
*   **Explanation:** If your champion is dead, this command will instantly revive them.
*   **Example:** `!revive`

---

### `!size`
*   **Syntax:** `!size size`
*   **Explanation:** Increases your champion's size by the specified percentage value permanently.
*   **Example:** `!size 50` (Increases your champion's size by 50%)

---

### `!skillpoints`
*   **Syntax:** `!skillpoints`
*   **Explanation:** Grants your champion 17 spell training points (skill points).
*   **Example:** `!skillpoints`

---

### `!spawn`
*   **Syntax:** `!spawn <type> [arguments...]`
*   **Explanation:** Spawns various game entities.
    *   `!spawn minionsblue` or `!spawn minionschaos`: Spawns a set of standard minions (melee, cannon, caster, super) for the blue (Order) or chaos (Red) team respectively, near your champion's position. They spawn idle.
    *   `!spawn champblue [championName]` or `!spawn champchaos [championName]`: Spawns a bot champion for the blue or chaos team. If `championName` (e.g., "Ashe") is provided, it spawns that champion; otherwise, it defaults to "Katarina". The bot spawns near your champion, idle, and with increased health.
    *   `!spawn regionblue [size] [time]` or `!spawn regionpurple [size] [time]`: Creates a perception bubble (area of vision) for the blue or purple (Neutral/Hostile to both?) team at your champion's position.
        *   `size` (optional): Radius of the vision bubble. Default 250.0.
        *   `time` (optional): Duration of the bubble in seconds. Default -1.0 (permanent).
*   **Examples:**
    *   `!spawn minionsblue`
    *   `!spawn champchaos Ashe`
    *   `!spawn regionblue 500 60` (Spawns a blue team vision bubble of radius 500 for 60 seconds)

---

### `!spawnstate`
*   **Syntax:** `!spawnstate 0 (disable) / 1 (enable)`
*   **Explanation:** Enables or disables automatic lane minion spawning for the current game mode.
*   **Examples:**
    *   `!spawnstate 1` (Enables minion spawning)
    *   `!spawnstate 0` (Disables minion spawning)

---

### `!speed`
*   **Syntax:** `!speed [flat speed] [percent speed]`
*   **Explanation:** Modifies your champion's movement speed.
    *   If one argument is provided, it's treated as `flat speed`.
    *   If two arguments are provided, the first is `flat speed` and the second is `percent speed` (e.g., 20 for +20%).
*   **Examples:**
    *   `!speed 50` (Adds 50 flat movement speed)
    *   `!speed 25 10` (Adds 25 flat movement speed and 10% bonus movement speed)

---

### `!stopanim`
*   **Syntax:** `!stopanim animationName`
*   **Explanation:** Stops the specified animation if it's currently playing on your champion.
*   **Example:** `!stopanim Dance`

---

### `!suicide`
*   **Syntax:** `!suicide`
*   **Explanation:** Causes your champion to take a massive amount of true damage, effectively killing them. **This command is available even if chat cheats are disabled.**
*   **Example:** `!suicide`

---

### `!targetable`
*   **Syntax:** `!targetable false (untargetable) / true (targetable)`
*   **Explanation:** Sets whether your champion can be targeted by abilities and attacks.
*   **Examples:**
    *   `!targetable false` (Makes your champion untargetable)
    *   `!targetable true` (Makes your champion targetable)

---

### `!tp`
*   **Syntax:** `!tp [target NetID] X Y` OR `!tp X Y`
*   **Explanation:** Teleports a unit to the specified X and Y coordinates.
    *   If only X and Y are provided, your champion is teleported.
    *   If a `target NetID` (network ID of a game object) is provided before X and Y, that unit will be teleported (if it's an AttackableUnit).
*   **Examples:**
    *   `!tp 1500 2000` (Teleports your champion to coordinates X:1500, Y:2000)
    *   `!tp 12345 3000 3500` (Teleports the unit with NetID 12345 to X:3000, Y:3500)

---

### `!xp`
*   **Syntax:** `!xp xpAmount`
*   **Explanation:** Adds the specified amount of experience points (XP) to your champion. The amount is capped at the total XP required for max level. Negative values are ignored.
*   **Example:** `!xp 1000` (Gives your champion 1000 XP)

---