using AutoGladiators.Core.Models.Exploration;
using TestMauiApp.Game;
using WorldPosition = AutoGladiators.Core.Models.WorldPosition;

namespace TestMauiApp;

public partial class MainPage : ContentPage
{
	private bool _initialised;

	public MainPage()
	{
		InitializeComponent();
		WireDpad(BtnUp, Facing.Up);
		WireDpad(BtnDown, Facing.Down);
		WireDpad(BtnLeft, Facing.Left);
		WireDpad(BtnRight, Facing.Right);

		var action = new TapGestureRecognizer();
		action.Tapped += (_, _) => MapView.Interact();
		BtnAction.GestureRecognizers.Add(action);
	}

	protected override async void OnAppearing()
	{
		base.OnAppearing();
		if (_initialised) return;
		_initialised = true;

		var zone = BuildTestZone(24, 24);
		LocationLabel.Text = zone.Name;
		await MapView.InitAsync(zone, playerTileX: 12, playerTileY: 12);
		HookKeyboard();
	}

	// Press-and-hold: keep walking while the pad is held, stop on release.
	private void WireDpad(Border pad, Facing facing)
	{
		var pointer = new PointerGestureRecognizer();
		pointer.PointerPressed += (_, _) =>
		{
			pad.BackgroundColor = Color.FromArgb("#3A4056");
			MapView.HoldDirection(facing);
		};
		pointer.PointerReleased += (_, _) =>
		{
			pad.BackgroundColor = Color.FromArgb("#232634");
			MapView.ReleaseDirection(facing);
		};
		pointer.PointerExited += (_, _) =>
		{
			pad.BackgroundColor = Color.FromArgb("#232634");
			MapView.ReleaseDirection(facing);
		};
		pad.GestureRecognizers.Add(pointer);

		// Fallback tap (single step) for platforms without pointer hover.
		var tap = new TapGestureRecognizer();
		tap.Tapped += (_, _) => MapView.TryMove(facing);
		pad.GestureRecognizers.Add(tap);
	}

	private void OnKey(Facing facing, bool down)
	{
		if (down) MapView.HoldDirection(facing);
		else MapView.ReleaseDirection(facing);
	}

	// Keyboard is desktop-only (WASD / arrow keys), hooked via the native panel.
#if WINDOWS
	private void HookKeyboard()
	{
		if (Handler?.PlatformView is Microsoft.UI.Xaml.Controls.Panel panel)
		{
			panel.IsTabStop = true;
			panel.KeyDown += (_, e) => { if (MapVirtualKey(e.Key, true)) e.Handled = true; };
			panel.KeyUp += (_, e) => { if (MapVirtualKey(e.Key, false)) e.Handled = true; };
			panel.Focus(Microsoft.UI.Xaml.FocusState.Programmatic);
		}
	}

	private bool _confirmHeld;

	private bool MapVirtualKey(Windows.System.VirtualKey k, bool down)
	{
		switch (k)
		{
			case Windows.System.VirtualKey.Up:
			case Windows.System.VirtualKey.W: OnKey(Facing.Up, down); return true;
			case Windows.System.VirtualKey.Down:
			case Windows.System.VirtualKey.S: OnKey(Facing.Down, down); return true;
			case Windows.System.VirtualKey.Left:
			case Windows.System.VirtualKey.A: OnKey(Facing.Left, down); return true;
			case Windows.System.VirtualKey.Right:
			case Windows.System.VirtualKey.D: OnKey(Facing.Right, down); return true;
			case Windows.System.VirtualKey.Space:
			case Windows.System.VirtualKey.Enter:
			case Windows.System.VirtualKey.E:
				// Fire once per physical press; key-repeat must not race dialogue.
				if (down && !_confirmHeld) { _confirmHeld = true; MapView.Interact(); }
				else if (!down) _confirmHeld = false;
				return true;
			default: return false;
		}
	}
#else
	private void HookKeyboard() { }
#endif

	/// <summary>
	/// A hand-built zone with terrain variety so the renderer, camera and
	/// collision are all visibly exercised. Uses the real Core WorldZone model.
	/// </summary>
	private static WorldZone BuildTestZone(int w, int h)
	{
		var zone = new WorldZone("lab_zone", "Renderer Lab", "Terrain test bed", w, h);

		MapTile Water() => new(TileType.Water, isPassable: false, spriteId: "water");
		MapTile Forest() => new(TileType.Forest, isPassable: true, hasEncounters: true, "forest", 0.1);
		MapTile Mountain() => new(TileType.Mountain, isPassable: false, spriteId: "mountain");
		MapTile Road() => new(TileType.Road, isPassable: true, spriteId: "road");
		MapTile Sand() => new(TileType.Desert, isPassable: true, spriteId: "sand");

		for (int x = 0; x < w; x++) { zone.SetTile(x, 0, Mountain()); zone.SetTile(x, h - 1, Mountain()); }
		for (int y = 0; y < h; y++) { zone.SetTile(0, y, Mountain()); zone.SetTile(w - 1, y, Mountain()); }
		for (int y = 1; y < h - 1; y++) zone.SetTile(w / 2, y, Road());
		for (int y = 3; y <= 7; y++) for (int x = 3; x <= 8; x++) zone.SetTile(x, y, Water());
		for (int y = 15; y <= 20; y++) for (int x = 15; x <= 20; x++) zone.SetTile(x, y, Forest());
		for (int y = 17; y <= 20; y++) for (int x = 3; x <= 9; x++) zone.SetTile(x, y, Sand());

		// Populate with characters. NPCs are non-blocking (live collision handles
		// them) so they don't leave a permanently blocked tile when they wander.
		AddNpc(zone, "npc_townsfolk", WorldObjectType.NPC, "TOWNSFOLK", 10, 10, new[]
		{
			"Welcome to the Arena Frontier, challenger.",
			"They say wild bots roam the tall grass to the south.",
		});
		AddNpc(zone, "npc_prof", WorldObjectType.QuestGiver, "PROF. GLADIUS", 11, 15, new[]
		{
			"Ah, a new gladiator! Your core signature is unusual.",
			"Defeat three rival trainers and return to me.",
		});
		AddNpc(zone, "npc_rex", WorldObjectType.Trainer, "RIVAL REX", 14, 12, new[]
		{
			"You there! That's a shiny new chassis.",
			"My bots will scrap it for parts. Prepare yourself!",
		});
		AddNpc(zone, "npc_scrap", WorldObjectType.Trainer, "SCRAP-BOT", 16, 10, new[]
		{
			"UNIT ONLINE. INTRUDER DETECTED.",
			"INITIATING COMBAT PROTOCOL.",
		});

		// Buildings — door tile is where the player stands to enter.
		zone.AddObject(new WorldObject("bld_heal", WorldObjectType.HealingCenter, "REPAIR BAY",
			new WorldPosition(zone.Id, 8, 13)));
		zone.AddObject(new WorldObject("bld_shop", WorldObjectType.Shop, "PARTS SHOP",
			new WorldPosition(zone.Id, 18, 13)));

		return zone;
	}

	private static void AddNpc(WorldZone zone, string id, WorldObjectType type, string name,
	                           int x, int y, string[] lines)
	{
		var obj = new WorldObject(id, type, name, new WorldPosition(zone.Id, x, y),
		                          spriteId: null, blocksMovement: false, isInteractable: true);
		obj.SetProperty("lines", lines);
		zone.AddObject(obj);
	}
}
