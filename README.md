﻿# AtomRPG.NuclearEdition
Modification for the game Atom RPG

# Install (easy)
Unpack [that](https://yadi.sk/d/NYZBpk-a9nSFXg) archive to the **game folder**

# Install (hard)
1. Unpack [that](https://yadi.sk/d/tRON_stJkeC6ng) archive to the **game folder**
2. Build and copy mod's DLL to the **game folder**\Mods\NuclearEdition

# Build
1. Open AtomRPG.NuclearEdition.csproj via text editor
2. Change "W:\Steam\steamapps\common\ATOM RPG" to your own game path
3. Build in Visual Studio 2017/2019

# Todo
1. Resolve game path via Windows Registry

# Feautures
1. Switch between allies in the barter window
Open a barter window with an NPC or partner. Press D (Camera Right) to display the inventory of the next ally on the left side of the screen. Press A (Camera Left) to return to the previous one. The list is looped. A vehicle is only available if it is located in the same location as you. The name of the current character will be displayed in the middle of the screen.

2. Sale of surplus
Open a barter window with an NPC. Select items to purchase (this is mandatory) worth 20 rubles or more. Press Alt+A (Highlight + Camera Left) to automatically select items of comparable value from the party inventory. As in the previous case, the vehicle inventory will only be available if the vehicle is located on the same map as you. Items for sale are selected automatically based on heuristic analysis. You must complete the sale yourself - the algorithm only selects items for sale, but does not complete the transaction.

3. Aggregation of loot from corpses
Open the inventory of the dead character. Items of all dead characters within a radius of 20 cells from looting character will be displayed on the right side of the screen. You can take all or some of these items.

4. Highlight of empty corpses and containers
If an unlocked container or a dead character doesn't have items in the inventory, they will be highlighted not in green, but in yellow.

5. Display hit chances without movement
In battle, hold Alt (Highlight) to display the hit chance of the current weapon for each visible enemy.

# Current loader

    string modsFolder = Path.GetFullPath("Mods");
    if (Directory.Exists(modsFolder))
    {
        Debug.Log("Looking for mods (" + modsFolder + ")...");
        foreach (string fileName in Directory.GetFiles(modsFolder, "*.dll", SearchOption.AllDirectories))
        {
            Debug.Log("Found assembly (" + fileName + "). Looking for entry points...");
            try
            {
                foreach (Type type in Assembly.LoadFrom(fileName).GetTypes())
                {
                    if (type.Name == "ModEntryPoint")
                    {
                        Debug.Log("Found entry point (" + type.FullName + "). Initializing...");
                        GameObject gameObject = new GameObject(fileName + "_" + type.FullName);
                        gameObject.AddComponent(type);
                        Object.DontDestroyOnLoad(gameObject);
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.LogError(string.Format("Failed to load assembly {0}. Error: {1}", fileName, ex));
            }
        }
    }
    else
    {
        Debug.Log("Mods directory is not exists (" + modsFolder + ")");
    }
