using System.Collections.Generic;
using Godot;

namespace ScooterStunt.Progression;

public partial class PlayerProgression : Node
{
	[Signal] public delegate void XpChangedEventHandler(int totalXp, int level, int upgradePoints);
	[Signal] public delegate void UpgradePurchasedEventHandler(string upgradeId, int newLevel);

	private const int XpPerLevel = 100;
	private const string SavePath = "user://save.json";

	public int TotalXp { get; private set; }
	public int Level { get; private set; } = 1;
	public int UpgradePoints { get; private set; }

	private readonly Dictionary<string, int> _upgradeLevels = new();
	private readonly HashSet<string> _completedMissions = new();

	public override void _Ready()
	{
		Load();
	}

	public void AddXp(int amount)
	{
		if (amount <= 0)
		{
			return;
		}

		TotalXp += amount;
		var newLevel = TotalXp / XpPerLevel + 1;
		if (newLevel > Level)
		{
			UpgradePoints += newLevel - Level;
			Level = newLevel;
		}

		EmitSignal(SignalName.XpChanged, TotalXp, Level, UpgradePoints);
		Save();
	}

	public int GetUpgradeLevel(string upgradeId)
	{
		return _upgradeLevels.TryGetValue(upgradeId, out var level) ? level : 0;
	}

	public bool TryPurchaseUpgrade(UpgradeDefinition upgrade)
	{
		var currentLevel = GetUpgradeLevel(upgrade.UpgradeId);
		if (currentLevel >= upgrade.MaxLevel || UpgradePoints < upgrade.CostPerLevel)
		{
			return false;
		}

		UpgradePoints -= upgrade.CostPerLevel;
		_upgradeLevels[upgrade.UpgradeId] = currentLevel + 1;

		EmitSignal(SignalName.UpgradePurchased, upgrade.UpgradeId, currentLevel + 1);
		EmitSignal(SignalName.XpChanged, TotalXp, Level, UpgradePoints);
		Save();
		return true;
	}

	public void MarkMissionCompleted(string missionName)
	{
		if (_completedMissions.Add(missionName))
		{
			Save();
		}
	}

	public bool IsMissionCompleted(string missionName)
	{
		return _completedMissions.Contains(missionName);
	}

	private void Save()
	{
		var upgrades = new Godot.Collections.Dictionary();
		foreach (var kvp in _upgradeLevels)
		{
			upgrades[kvp.Key] = kvp.Value;
		}

		var missions = new Godot.Collections.Array();
		foreach (var name in _completedMissions)
		{
			missions.Add(name);
		}

		var data = new Godot.Collections.Dictionary
		{
			["total_xp"] = TotalXp,
			["level"] = Level,
			["upgrade_points"] = UpgradePoints,
			["upgrades"] = upgrades,
			["completed_missions"] = missions
		};

		using var file = FileAccess.Open(SavePath, FileAccess.ModeFlags.Write);
		file?.StoreString(Json.Stringify(data));
	}

	private void Load()
	{
		if (!FileAccess.FileExists(SavePath))
		{
			return;
		}

		using var file = FileAccess.Open(SavePath, FileAccess.ModeFlags.Read);
		if (file == null)
		{
			return;
		}

		var text = file.GetAsText();
		var parsed = Json.ParseString(text);
		if (parsed.VariantType != Variant.Type.Dictionary)
		{
			return;
		}

		var data = parsed.AsGodotDictionary();
		TotalXp = data.TryGetValue("total_xp", out var xp) ? xp.AsInt32() : 0;
		Level = data.TryGetValue("level", out var lvl) ? lvl.AsInt32() : 1;
		UpgradePoints = data.TryGetValue("upgrade_points", out var pts) ? pts.AsInt32() : 0;

		_upgradeLevels.Clear();
		if (data.TryGetValue("upgrades", out var upgradesVariant) && upgradesVariant.VariantType == Variant.Type.Dictionary)
		{
			var upgradesDict = upgradesVariant.AsGodotDictionary();
			foreach (var key in upgradesDict.Keys)
			{
				_upgradeLevels[key.AsString()] = upgradesDict[key].AsInt32();
			}
		}

		_completedMissions.Clear();
		if (data.TryGetValue("completed_missions", out var missionsVariant) && missionsVariant.VariantType == Variant.Type.Array)
		{
			var missionsArray = missionsVariant.AsGodotArray();
			foreach (var m in missionsArray)
			{
				_completedMissions.Add(m.AsString());
			}
		}
	}
}
