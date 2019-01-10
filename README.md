# AtomRPG.NuclearEdition
Modification for the game Atom RPG

# Install (easy)
Unpack [that] (https://yadi.sk/d/LiNwcOkg7QQrjw) archive to the **game folder**

# Install (hard)
1. Unpack [that] (https://yadi.sk/d/tRON_stJkeC6ng) archive to the **game folder**
2. Build and copy mod's DLL to the **game folder**\Mods\NuclearEdition

# Build
1. Open AtomRPG.NuclearEdition.csproj via text editor
2. Change "W:\Steam\steamapps\common\ATOM RPG" to your own game path
3. Build in Visual Studio 2017/2019

# Todo
1. Resolve game path via Windows Registry
2. Generate .mdb symbols after build

# Feautures
1. You can switch between characters during barter

# Current loader

```string modsFolder = Path.GetFullPath("Mods");
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
}```
