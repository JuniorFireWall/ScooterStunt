using Godot;
using ScooterStunt.Progression;

namespace ScooterStunt.UI;

public partial class UpgradeShop : CanvasLayer
{
	[Export] public Godot.Collections.Array<UpgradeDefinition> Upgrades = new();

	private PlayerProgression _progression;
	private VBoxContainer _list;
	private Label _pointsLabel;
	private Label[] _rowLabels;
	private Button[] _buyButtons;

	public override void _Ready()
	{
		_progression = GetNode<PlayerProgression>("/root/Progression");
		_list = GetNode<VBoxContainer>("Panel/VBox/UpgradeList");
		_pointsLabel = GetNode<Label>("Panel/VBox/PointsLabel");

		_rowLabels = new Label[Upgrades.Count];
		_buyButtons = new Button[Upgrades.Count];

		for (var i = 0; i < Upgrades.Count; i++)
		{
			var row = new HBoxContainer();
			var label = new Label();
			var button = new Button { Text = "Kaufen" };
			var index = i;
			button.Pressed += () => OnBuyPressed(index);

			row.AddChild(label);
			row.AddChild(button);
			_list.AddChild(row);

			_rowLabels[i] = label;
			_buyButtons[i] = button;
		}

		_progression.XpChanged += (_, _, _) => RefreshUi();
		_progression.UpgradePurchased += (_, _) => RefreshUi();

		RefreshUi();
	}

	private void OnBuyPressed(int index)
	{
		_progression.TryPurchaseUpgrade(Upgrades[index]);
	}

	private void RefreshUi()
	{
		_pointsLabel.Text = $"Level {_progression.Level} - Upgrade-Punkte: {_progression.UpgradePoints}";

		for (var i = 0; i < Upgrades.Count; i++)
		{
			var upgrade = Upgrades[i];
			var level = _progression.GetUpgradeLevel(upgrade.UpgradeId);
			_rowLabels[i].Text = $"{upgrade.DisplayName} Lv.{level}/{upgrade.MaxLevel} (Kosten: {upgrade.CostPerLevel})";
			_buyButtons[i].Disabled = level >= upgrade.MaxLevel || _progression.UpgradePoints < upgrade.CostPerLevel;
		}
	}
}
