using LinqToYourDoom;
using Microsoft.Extensions.Hosting;
using System;
using System.Numerics;
using System.Reactive.Concurrency;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Runtime.InteropServices;
using System.Text;
using Uy;
using Vortice.Direct2D1;
using Vortice.DirectWrite;
using Vortice.Mathematics;
using static Measure;
using BitmapInterpolationMode = Vortice.Direct2D1.BitmapInterpolationMode;

// TODO: commit
// TODO: push
// TODO: Deploy V2 to itch.io
// TODO: backup
// TODO: SFX
// TODO: Opti
// TODO: *duplicate* to Game2
// TODO: *duplicate* to Game3
// TODO: *duplicate* to Game4 (need to create Game4...)
// TODO: Opti
// TODO: Deploy V3 to itch.io

#pragma warning disable IDE0079
#pragma warning disable CS8618
#pragma warning disable IDE0044
#pragma warning disable IDE0060
class PongGame : Game, IDisposable {
	#region Utilities

	IHostApplicationLifetime Application;
	IDeviceIndependentResourceDictionary ApplicationResources;

	public PongGame(IHostApplicationLifetime application, IDeviceIndependentResourceDictionary applicationResources) {
		Application = application;
		ApplicationResources = applicationResources;
	}

	public void Dispose() {
		Player1.Geometry?.Dispose();
		Player2.Geometry?.Dispose();
	}

	struct ValueList<T> where T : struct {
		T[] Items;
		public ValueList() => Items = new T[4];

		public int Count { get; private set; } = 0;

		public ref T this[int index] => ref Items[index];

		public void Add(T item) {
			if (Count == Items.Length)
				Array.Resize(ref Items, Items.Length * 2);

			Items[Count] = item;
			++Count;
		}

		public void RemoveAt(int index) {
			--Count;
			Items[index] = Items[Count];
		}

		public void Clear() => Count = 0;
	}

	[DllImport("User32.dll", CharSet = CharSet.Unicode)]
	static extern int GetKeyNameText(int lParam, [Out] StringBuilder lpString, int nSize);
	static string GetKeyName(Key key) {
		var scanCode = key switch {
			Key.KeyW => 17,
			Key.KeyA => 30,
			Key.KeyS => 31,
			Key.KeyD => 32,

			_ => throw new NotImplementedException()
		};

		var builder = new StringBuilder(4, 4);

		return GetKeyNameText(scanCode << 16, builder, builder.Capacity) == 0
				? "?"
				: builder.ToString();
	}

	#endregion
	#region Physics resources

	enum State {
		Lobby, InGame, Player1Wins, Player2Wins
	}

	struct Player {
		public AngleF Angle;
		public float Radius;
		public float Speed;
		public float Focus;

		public ID2D1LinearGradientBrush Brush;
		public ID2D1PathGeometry1 Geometry;

		public void SyncGeometry(IDeviceIndependentResourceDictionary applicationResources) {
			Geometry?.Dispose();

			Geometry = applicationResources.D2Factory.CreatePathGeometry();
			{
				using var path = Geometry.Open();

				var (y, x) = MathF.SinCos(-Radius);

				path.BeginFigure(new Vector2(x, y) * 300, FigureBegin.Hollow);

				(y, x) = MathF.SinCos(+Radius);

				path.AddArc(new ArcSegment(new Vector2(x, y) * 300, new Size(300), 0, SweepDirection.Clockwise, ArcSize.Small));
				path.EndFigure(FigureEnd.Open);
				path.Close();
			}

			if (Brush != null)
				Brush.StartPoint = new Vector2(Radius.Cos() * 300 - 5, 0);
		}
	}

	struct Ball {
		public Vector2 Position;
		public float Radius;
		public Vector2 Velocity;
		public bool PlayerSwitch;
		public float TimeSinceAppear;
	}

	struct ExplosionParticle {
		public Vector2 Position;
		public float TTL;
		public float LifeTime;

		public ExplosionParticle(Vector2 position, float ttl) {
			Position = position;
			TTL =
			LifeTime = ttl;
		}
	}

	delegate void PowerUpApplyAction(bool playerSwitch, ref Player player, ref Player otherPlayer);

	struct PowerUp {
		public static float Radius = 24;

		public Vector2 Position;
		public Vector2 Velocity;
		public int Icon;
		public PowerUpApplyAction Apply;
		public float TimeSinceAppear;
	}

	void ApplyPowerUp_IncreasePlayerSpeed(bool playerSwitch, ref Player player, ref Player otherPlayer) {
		if (player.Speed < 0.7f * T / S)
			player.Speed += 0.01f * T / S;
	}

	void ApplyPowerUp_DecreasePlayerSpeed(bool playerSwitch, ref Player player, ref Player otherPlayer) {
		if (0.2f * T / S < player.Speed)
			player.Speed -= 0.01f * T / S;
	}

	void ApplyPowerUp_IncreasePlayerRadius(bool playerSwitch, ref Player player, ref Player otherPlayer) {
		if (player.Radius < 0.07f * T) {
			player.Radius += 0.01f;
			player.SyncGeometry(ApplicationResources);
		}
	}

	void ApplyPowerUp_DecreasePlayerRadius(bool playerSwitch, ref Player player, ref Player otherPlayer) {
		if (0.01f * T < player.Radius) {
			player.Radius -= 0.01f;
			player.SyncGeometry(ApplicationResources);
		}
	}

	void ApplyPowerUp_IncreasePlayerFocus(bool playerSwitch, ref Player player, ref Player otherPlayer) {
		if (player.Focus < 0.8f)
			player.Focus += 0.15f;
	}

	void ApplyPowerUp_DecreasePlayerFocus(bool playerSwitch, ref Player player, ref Player otherPlayer) {
		if (-0.8f < player.Focus)
			player.Focus -= 0.15f;
	}

	void ApplyPowerUp_IncreaseBallCount(bool playerSwitch, ref Player player, ref Player otherPlayer) {
		SpawnBall(!playerSwitch);
	}

	void ApplyPowerUp_DecreaseBallCount(bool playerSwitch, ref Player player, ref Player otherPlayer) {
		if (1 < Balls.Count)
			for (var i = 0; i < Balls.Count; ++i)
				if (Balls[i].PlayerSwitch == playerSwitch)
					Balls.RemoveAt(i);
	}

	void ApplyPowerUp_IncreaseBallSpeed(bool playerSwitch, ref Player player, ref Player otherPlayer) {
		for (var i = 0; i < Balls.Count; ++i) {
			var speed = Balls[i].Velocity.Length();

			if (speed < 1.5f / S)
				Balls[i].Velocity.SetLength(speed + 0.01f / S);
		}
	}

	void ApplyPowerUp_DecreaseBallSpeed(bool playerSwitch, ref Player player, ref Player otherPlayer) {
		for (var i = 0; i < Balls.Count; ++i) {
			var speed = Balls[i].Velocity.Length();

			if (0.5 / S < speed)
				Balls[i].Velocity.SetLength(speed - 0.01f / S);
		}
	}

	State GameState;
	Player Player1 = new();
	Player Player2 = new();
	ValueList<Ball> Balls = new();
	ValueList<ExplosionParticle> Explosions = new();
	ValueList<PowerUp> PowerUps = new();
	ValueList<ValueTuple<PowerUpApplyAction>> AvailablePowerUps = new();

	public override void OnLoad(CompositeDisposable disposables) {
		GameState = State.Lobby;

		AvailablePowerUps.Add(new(ApplyPowerUp_IncreasePlayerSpeed));
		AvailablePowerUps.Add(new(ApplyPowerUp_DecreasePlayerSpeed));
		AvailablePowerUps.Add(new(ApplyPowerUp_IncreasePlayerRadius));
		AvailablePowerUps.Add(new(ApplyPowerUp_DecreasePlayerRadius));
		AvailablePowerUps.Add(new(ApplyPowerUp_IncreasePlayerFocus));
		AvailablePowerUps.Add(new(ApplyPowerUp_DecreasePlayerFocus));
		AvailablePowerUps.Add(new(ApplyPowerUp_IncreaseBallCount));
		AvailablePowerUps.Add(new(ApplyPowerUp_DecreaseBallCount));
		AvailablePowerUps.Add(new(ApplyPowerUp_IncreaseBallSpeed));
		AvailablePowerUps.Add(new(ApplyPowerUp_DecreaseBallSpeed));
	}

	CompositeDisposable InGameDisposables = new();

	void StartPlaying() {
		GameState = State.InGame;

		var firstBallDirection = MathD.Random.PiAngleF();

		Player1.Speed = 0.3f * T / S;
		Player1.Angle = firstBallDirection + 0.2f * T;
		Player1.Radius = 0.02f * T;
		Player1.Focus = 0.2f;
		Player1.SyncGeometry(ApplicationResources);

		Player2.Speed = 0.3f * T / S;
		Player2.Angle = firstBallDirection - 0.2f * T;
		Player2.Radius = 0.02f * T;
		Player2.Focus = 0.2f;
		Player2.SyncGeometry(ApplicationResources);

		Balls.Clear();
		Explosions.Clear();

		SpawnBall(firstBallDirection, 150 / S, MathD.Random.Bool());

		Observable
			.Interval(TimeSpan.FromSeconds(9))
			.ObserveOn(Clock.GameLoopScheduler)
			.Subscribe(SpawnPowerUp)
			.DisposeWith(InGameDisposables);
	}

	void SpawnBall(bool playerSwitch) => SpawnBall(MathD.Random.PiAngleF(), 150 / S, playerSwitch);
	void SpawnBall(float speed, bool playerSwitch) => SpawnBall(MathD.Random.PiAngleF(), speed, playerSwitch);
	void SpawnBall(float direction, float speed, bool playerSwitch) {
		var position = Vector2.Zero.WithAngularOffset(MathD.Random.PiAngleF(), MathD.Random.UncheckedBetween(0f, 135));

		Explosions.Add(new(position, 1 * S));

		Clock.GameLoopScheduler
			.Schedule(TimeSpan.FromSeconds(1), () =>
				Balls.Add(new() {
					Position = position,
					Radius = 16,
					Velocity = Vector2.Zero.WithAngularOffset(direction, speed),
					PlayerSwitch = playerSwitch,
				})
			)
			.DisposeWith(InGameDisposables);
	}

	void SpawnPowerUp() {
		var position = Vector2.Zero.WithAngularOffset(MathD.Random.PiAngleF(), MathD.Random.UncheckedBetween(0f, 135));
		var icon = MathD.Random.UncheckedBetween(0, AvailablePowerUps.Count);
		var apply = AvailablePowerUps[icon].Item1;

		Explosions.Add(new(position, 1 * S));

		Clock.GameLoopScheduler
			.Schedule(TimeSpan.FromSeconds(1), () =>
				PowerUps.Add(new() {
					Position = position,
					Velocity = Vector2.Zero.WithAngularOffset(MathD.Random.PiAngleF(), 150 / S),
					Icon = icon,
					Apply = apply,
				})
			)
			.DisposeWith(InGameDisposables);
	}

	void DeclareWinner(bool playerSwitch) {
		InGameDisposables.Clear();
		GameState = playerSwitch ? State.Player1Wins : State.Player2Wins;
	}

	#endregion
	#region Graphics resources - Device-independent

	IDWriteTextFormat3 DebugFont;
	IDWriteTextFormat3 SmallKeyFont;
	IDWriteTextFormat3 LargeKeyFont;
	ID2D1StrokeStyle1 PlayerShadowStrokeStyle;

	public override void OnApplicationInit(IDeviceIndependentResourceDictionary applicationResources, CompositeDisposable disposables) {
		using var embeddedFonts = new FontCache(applicationResources.WriteFactory)
			.DisposeWith(disposables)
			.Add(typeof(PongGame).Assembly.GetEmbeddedResourceBytes("Games/Pong/JetBrainsMono-Regular.ttf"))
			.BuildCollection();

		using (var font = applicationResources.WriteFactory.CreateTextFormat("JetBrains Mono", embeddedFonts, FontWeight.Normal, FontStyle.Normal, FontStretch.Normal, 16))
			DebugFont = font.QueryInterface<IDWriteTextFormat3>().DisposeWith(disposables);

		using (var font = applicationResources.WriteFactory.CreateTextFormat("JetBrains Mono", embeddedFonts, FontWeight.Normal, FontStyle.Normal, FontStretch.Normal, 26))
			SmallKeyFont = font.QueryInterface<IDWriteTextFormat3>().DisposeWith(disposables);

		SmallKeyFont.TextAlignment = TextAlignment.Center;
		SmallKeyFont.ParagraphAlignment = ParagraphAlignment.Center;

		using (var font = applicationResources.WriteFactory.CreateTextFormat("JetBrains Mono", embeddedFonts, FontWeight.Normal, FontStyle.Normal, FontStretch.Normal, 34))
			LargeKeyFont = font.QueryInterface<IDWriteTextFormat3>().DisposeWith(disposables);

		LargeKeyFont.TextAlignment = TextAlignment.Center;
		LargeKeyFont.ParagraphAlignment = ParagraphAlignment.Center;

		PlayerShadowStrokeStyle = applicationResources.D2Factory.CreateStrokeStyle(new StrokeStyleProperties1 {
			StartCap = CapStyle.Round,
			EndCap = CapStyle.Round,
		}).DisposeWith(disposables);
	}

	#endregion
	#region Graphics resources - Device-dependent

	static class Colors {
		public static Color4 Orchid = 0x9575A3.ToRgb();
		//public static Color4 Tan = 0xA38E75.ToRgb();
		public static Color4 Teal = 0x64D580.ToRgb();
		public static Color4 Blue = 0x32C0EA.ToRgb();
		public static Color4 Gold = 0xFDBE11.ToRgb();
		public static Color4 Orange = 0xF77A34.ToRgb();
		public static Color4 Dark1 = 0x474649.ToRgb();
		//public static Color4 Dark2 = 0x302E33.ToRgb();
		public static Color4 Dark3 = 0x1F1C21.ToRgb();
		public static Color4 Dark4 = 0x0F0E10.ToRgb();
		public static Color4 Black = 0x000000.ToRgb();
		public static Color4 White = 0xFEFAEC.ToRgb();
		public static Color4 Transparent = Vortice.Mathematics.Colors.Transparent;
	}

	static class SpriteCoordinates {
		public static Box4 CirclePong = Box4.FromLeftTop(0, 0, 836, 380);
		public static Box4 CirclePongKey = Box4.FromLeftTop(256.7f, 252.7f, 70, 70);
		public static (Vector2 Center, float Radius) CirclePongGradient = (new(594, 77), 916);
		public static Box4 Player1Wins = Box4.FromLeftTop(0, 380, 690, 490);
		public static Box4 Player2Wins = Box4.FromLeftTop(0, 870, 690, 490);
		public static Box4 WinKey = Box4.FromLeftTop(183.7f, 361.7f, 70, 70);
		public static (Vector2 Center, float Radius) WinGradient = (new(585, 66), 1001);
		public static Box4 LargeKey(int i) => Box4.FromLeftTop(690, 380 + 80 * i, 80, 80);
		public static Box4 SmallKey(int i) => Box4.FromLeftTop(770, 380 + 80 * i, 58, 58);
		public static Box4 PowerUp(int i) => Box4.FromLeftTop(690, 780 + 48 * i, 48, 48);
	}

	ID2D1SolidColorBrush TextColor;
	ID2D1SolidColorBrush CircleColor;
	ID2D1SolidColorBrush ShadowColor;
	ID2D1SolidColorBrush BallColor1;
	ID2D1SolidColorBrush BallColor2;
	ID2D1SolidColorBrush IndicatorColor1;
	ID2D1SolidColorBrush IndicatorColor2;
	ID2D1RadialGradientBrush BackgroundGradient;
	ID2D1RadialGradientBrush CircleBackground;
	ID2D1RadialGradientBrush ExplosionGradient;
	ID2D1SvgDocument SpriteSvg;

	public override void OnDeviceInit(IDeviceIndependentResourceDictionary applicationResources, IDeviceDependentResourceDictionary deviceResources, CompositeDisposable disposables) {
		var context = deviceResources.D2DeviceContext;

		TextColor = context.CreateSolidColorBrush(Colors.White).DisposeWith(disposables);
		CircleColor = context.CreateSolidColorBrush(Colors.Orchid).DisposeWith(disposables);
		ShadowColor = context.CreateSolidColorBrush(Colors.Black).DisposeWith(disposables);
		BallColor1 = context.CreateSolidColorBrush(Colors.Gold).DisposeWith(disposables);
		BallColor2 = context.CreateSolidColorBrush(Colors.Blue).DisposeWith(disposables);
		IndicatorColor1 = context.CreateSolidColorBrush(Colors.Orange).DisposeWith(disposables);
		IndicatorColor2 = context.CreateSolidColorBrush(Colors.Blue).DisposeWith(disposables);

		using (var stops = context.CreateGradientStopCollection(new GradientStop[] {
			new(0, Colors.Dark3),
			new(1, Colors.Dark4),
		}))
			BackgroundGradient = context.CreateRadialGradientBrush(default, stops).DisposeWith(disposables);
		
		using (var stops = context.CreateGradientStopCollection(new GradientStop[] {
			new(0, Colors.Dark4),
			new(1, Colors.Dark3),
		}))
			CircleBackground = context.CreateRadialGradientBrush(new RadialGradientBrushProperties {
				Center = Vector2.Zero,
				RadiusX = 300,
				RadiusY = 300,
			}, stops).DisposeWith(disposables);

		using (var stops = context.CreateGradientStopCollection(new GradientStop[] {
			new(0, Colors.Dark1),
			new(1, Colors.Transparent),
		}))
			ExplosionGradient = context.CreateRadialGradientBrush(new RadialGradientBrushProperties {
				RadiusX = 50,
				RadiusY = 50,
			}, stops).DisposeWith(disposables);

		using (var stops = context.CreateGradientStopCollection(new GradientStop[] {
			new(0, Colors.Gold),
			new(1, Colors.Orange),
		}))
			Player1.Brush = context.CreateLinearGradientBrush(new LinearGradientBrushProperties {
				StartPoint = new Vector2(Player1.Radius.Cos() * 300 - 5, 0),
				EndPoint = new Vector2(305, 0),
			}, stops).DisposeWith(disposables);
		
		using (var stops = context.CreateGradientStopCollection(new GradientStop[] {
			new(0, Colors.Teal),
			new(1, Colors.Blue),
		}))
			Player2.Brush = context.CreateLinearGradientBrush(new LinearGradientBrushProperties {
				StartPoint = new Vector2(Player2.Radius.Cos() * 300 - 5, 0),
				EndPoint = new Vector2(305, 0),
			}, stops).DisposeWith(disposables);

		SpriteSvg = deviceResources.LoadSvg(applicationResources, new Vector2(836, 1360), "Games/Pong/Sprite.svg").DisposeWith(disposables);
	}

	#endregion
	#region Graphics resources - Window size-dependent

	Box4 CenterBox;
	float CenterScale;
	ID2D1BitmapRenderTarget SpriteTarget;
	ID2D1DeviceContext6 SpriteContext;

	public override void OnResizeInit(IDeviceIndependentResourceDictionary applicationResources, IDeviceDependentResourceDictionary deviceResources, CompositeDisposable disposables) {
		var context = deviceResources.D2DeviceContext;

		CenterBox = Box4.FromCenter(Window.Size / 2, new Vector2(MathF.Min(Window.Size.X, Window.Size.Y)));
		CenterScale = CenterBox.Height / 1080;

		SpriteTarget = context.CreateCompatibleRenderTarget(new Size(836, 1360)).DisposeWith(disposables);
		SpriteContext = SpriteTarget.QueryInterface<ID2D1DeviceContext6>().DisposeWith(disposables);

		SpriteContext.BeginDraw();
		{
			SpriteContext.DrawSvgDocument(SpriteSvg);
		}
		SpriteContext.EndDraw();
	}

	#endregion
	#region Physics time

	double UpdateDuration;

	public override void OnUpdate() {
		var _updateStartTime = Clock.ElapsedMilliseconds;
		{
			switch (GameState) {
				case State.InGame:
					OnUpdate_InGame();
					break;

				case State.Lobby:
				case State.Player1Wins:
				case State.Player2Wins:
					OnUpdate_Lobby();
					break;
					
				default: throw new NotImplementedException("Unknown game state " + GameState);
			}

		}
		var _updateEndTime = Clock.ElapsedMilliseconds;

		UpdateDuration = _updateEndTime - _updateStartTime;
	}

	void OnUpdate_Lobby() {
		var direction = Stick1.Vector.X + Stick2.Vector.X;

		if (direction < 0)
			Application.StopApplication();

		else if (0 < direction)
			StartPlaying();
	}

	void OnUpdate_InGame() {
		OnUpdate_InGame_PlayersMovement();
		OnUpdate_InGame_BallsMovement();
		OnUpdate_InGame_PowerUpsMovement();
		OnUpdate_InGame_ExplosionsAnimation();
	}

	void OnUpdate_InGame_PlayersMovement() {
		Player1.Angle += Stick1.Vector.Y * Player1.Speed * Clock.SPF;
		Player2.Angle += Stick2.Vector.Y * Player2.Speed * Clock.SPF;
	}
	
	void OnUpdate_InGame_BallsMovement() {
		for (var i = 0; i < Balls.Count; ++i) {
			ref var ball = ref Balls[i];

			ball.Position.X += ball.Velocity.X * Clock.SPF;
			ball.Position.Y += ball.Velocity.Y * Clock.SPF;
			ball.TimeSinceAppear += Clock.SPF;

			// We're out of the circle!
			if (300 - ball.Radius < ball.Position.Length()) {
				var ballAngle = Vector2.Zero.DirectionTo(ball.Position);
				var player = ball.PlayerSwitch ? Player1 : Player2;
				var rotationToPlayerCenter = ballAngle.ShortArcTo(player.Angle);

				// The ball is caught by the player who's it's the turn.
				// Bounce back in the circle and continue!
				if (MathF.Abs(rotationToPlayerCenter) < player.Radius) {
					ball.Position.X = (299 - ball.Radius) * MathF.Cos(ballAngle);
					ball.Position.Y = (299 - ball.Radius) * MathF.Sin(ballAngle);
					ball.Velocity = Vector2.Zero.WithAngularOffset(ballAngle.Opposite + ballAngle - ball.Velocity.Direction() - player.Focus * rotationToPlayerCenter, ball.Velocity.Length());
					ball.PlayerSwitch = !ball.PlayerSwitch;
				}

				// The player didn't catch the ball.
				// If it was the last ball, it means GAME OVER!
				else {
					Balls.RemoveAt(i);

					if (Balls.Count == 0)
						DeclareWinner(!ball.PlayerSwitch);
				}
			}
		}
	}
	
	void OnUpdate_InGame_PowerUpsMovement() {
		for (var i = 0; i < PowerUps.Count; ++i) {
			ref var powerUp = ref PowerUps[i];

			powerUp.Position.X += powerUp.Velocity.X * Clock.SPF;
			powerUp.Position.Y += powerUp.Velocity.Y * Clock.SPF;
			powerUp.TimeSinceAppear += Clock.SPF;

			// We're out of the circle!
			if (300 - PowerUp.Radius / 2f < powerUp.Position.Length()) {
				var powerUpAngle = Vector2.Zero.DirectionTo(powerUp.Position);

				// The power-up is caught by player #1.
				// Activate the power-up!
				if (powerUpAngle.ShortArcTo(Player1.Angle).Abs() < Player1.Radius)
					powerUp.Apply(true, ref Player1, ref Player2);

				// The power-up is caught by player #2.
				// Activate the power-up!
				if (powerUpAngle.ShortArcTo(Player2.Angle).Abs() < Player2.Radius)
					powerUp.Apply(false, ref Player2, ref Player1);

				PowerUps.RemoveAt(i);
			}
		}
	}

	void OnUpdate_InGame_ExplosionsAnimation() {
		for (var i = 0; i < Explosions.Count; ++i)
			if ((Explosions[i].TTL -= Clock.SPF) < 0)
				Explosions.RemoveAt(i);
	}

	#endregion
	#region Drawing time

	public override void OnRender(IDeviceIndependentResourceDictionary applicationResources, IDeviceDependentResourceDictionary deviceResources) {
		var context = deviceResources.D2DeviceContext;

		context.Clear(Colors.Black);
		{
			switch (GameState) {
				case State.Lobby: OnRender_Lobby(applicationResources, deviceResources); break;
				case State.InGame: OnRender_InGame(applicationResources, deviceResources); break;
				case State.Player1Wins: OnRender_PlayerWin(true, applicationResources, deviceResources); break;
				case State.Player2Wins: OnRender_PlayerWin(false, applicationResources, deviceResources); break;

				default: throw new NotImplementedException("Unknown game state " + GameState);
			}
		}
		context.DrawText($"{ Clock.FPS :0} FPS - Update: { UpdateDuration :00.00}ms - State: { GameState }", DebugFont, Window.Box.Deflated(16).ToRect(), TextColor);
	}

	void OnRender_Lobby(IDeviceIndependentResourceDictionary applicationResources, IDeviceDependentResourceDictionary deviceResources) =>
		DrawScreenSprite(deviceResources, SpriteCoordinates.CirclePong, SpriteCoordinates.CirclePongKey, SpriteCoordinates.CirclePongGradient);

	void OnRender_InGame(IDeviceIndependentResourceDictionary applicationResources, IDeviceDependentResourceDictionary deviceResources) {
		var context = deviceResources.D2DeviceContext;

		BackgroundGradient.Center = CenterBox.CenterAnchor;
		BackgroundGradient.RadiusX =
		BackgroundGradient.RadiusY = 756 * CenterScale;

		context.FillRectangle(new Rect(Window.Size.X, Window.Size.Y), BackgroundGradient);

		using (context.PushTranslation(CenterBox.CenterAnchor))
		using (context.PushScale(CenterScale)) {
			context.FillEllipse(new Ellipse(Vector2.Zero, 300, 300), CircleBackground);
			context.DrawEllipse(new Ellipse(Vector2.Zero, 300, 300), CircleColor, 6);

			RenderParticles();
			RenderPowerUps();
			RenderBalls();
			RenderPlayer(true, Player1);
			RenderPlayer(false, Player2);

			void RenderParticles() {
				for (var i = 0; i < Explosions.Count; ++i) {
					var explosion = Explosions[i];

					ExplosionGradient.Center = explosion.Position;
					ExplosionGradient.Opacity = explosion.TTL / explosion.LifeTime;

					context.FillEllipse(new Ellipse(explosion.Position, 50, 50), ExplosionGradient);
				}
			}

			void RenderPowerUps() {
				for (var i = 0; i < PowerUps.Count; ++i) {
					var powerUp = PowerUps[i];
					var powerUpRadius = PowerUp.Radius * (powerUp.TimeSinceAppear * 2).CoerceAtMost(1f);
					var spriteRect = SpriteCoordinates.PowerUp(powerUp.Icon).ToRect();
					var drawRect = Box4.FromCenter(powerUp.Position, new Vector2(powerUpRadius * 2)).ToRect();

					context.DrawBitmap(SpriteTarget.Bitmap, drawRect, 1f, BitmapInterpolationMode.Linear, spriteRect);
				}
			}

			void RenderBalls() {
				for (var i = 0; i < Balls.Count; ++i) {
					var ball = Balls[i];
					var ballRadius = ball.Radius * (ball.TimeSinceAppear * 2).CoerceAtMost(1f);

					context.DrawEllipse(new Ellipse(ball.Position, ballRadius, ballRadius), ShadowColor, 12);
					context.FillEllipse(new Ellipse(ball.Position, ballRadius, ballRadius), ball.PlayerSwitch ? BallColor1 : BallColor2);
				}
			}

			void RenderPlayer(bool playerSwitch, Player player) {
				var hasActiveBall = false;
				var largeKeySwitch = 0;

				var min = float.NaN;
				for (var i = 0; i < Balls.Count; ++i) {
					var ball = Balls[i];

					if (ball.PlayerSwitch == playerSwitch) {
						var (x0, y0) = ball.Position;
						var (x1, y1) = ball.Position.WithOffset(ball.Velocity);
						var d = MathF.Sqrt((x1 - x0).Pow(2) + (y1 - y0).Pow(2));

						var estimatedCircleCollision = new Vector2(
							x0 + (300 - ball.Position.Length()) * (x1 - x0) / d,
							y0 + (300 - ball.Position.Length()) * (y1 - y0) / d
						);

						min = MathD.AbsMin(min, player.Angle.ShortArcTo(estimatedCircleCollision.Direction()));
					}
				}

				if (min.IsNotNaN()) {
					hasActiveBall = true;
					largeKeySwitch = min.Sign().CoerceToInt();
				}

				else {
					for (var i = 0; i < PowerUps.Count; ++i) {
						var powerUp = PowerUps[i];

						var (x0, y0) = powerUp.Position;
						var (x1, y1) = powerUp.Position.WithOffset(powerUp.Velocity);
						var d = MathF.Sqrt((x1 - x0).Pow(2) + (y1 - y0).Pow(2));

						var estimatedCircleCollision = new Vector2(
							x0 + (300 - powerUp.Position.Length()) * (x1 - x0) / d,
							y0 + (300 - powerUp.Position.Length()) * (y1 - y0) / d
						);

						min = MathD.AbsMin(min, player.Angle.ShortArcTo(estimatedCircleCollision.Direction()));
					}
				}

				if (min.IsNotNaN())
					largeKeySwitch = min.Sign().CoerceToInt();

				var spriteRect = largeKeySwitch == -1
					? SpriteCoordinates.LargeKey(playerSwitch ? 2 : 3).ToRect()
					: SpriteCoordinates.SmallKey(playerSwitch ? 2 : 3).ToRect();

				var drawRect = SquareAtCircleEdge(player.Angle - (player.Radius + 0.01f * T), spriteRect.Height);

				context.DrawBitmap(SpriteTarget.Bitmap, drawRect, 1f, BitmapInterpolationMode.Linear, spriteRect);

				if (playerSwitch)
					context.DrawText(GetKeyName(Key.KeyW), largeKeySwitch == -1 ? LargeKeyFont : SmallKeyFont, drawRect, TextColor);

				spriteRect = largeKeySwitch == 1
					? SpriteCoordinates.LargeKey(playerSwitch ? 2 : 4).ToRect()
					: SpriteCoordinates.SmallKey(playerSwitch ? 2 : 4).ToRect();

				drawRect = SquareAtCircleEdge(player.Angle + (player.Radius + 0.01f * T), spriteRect.Height);

				context.DrawBitmap(SpriteTarget.Bitmap, drawRect, 1f, BitmapInterpolationMode.Linear, spriteRect);

				if (playerSwitch)
					context.DrawText(GetKeyName(Key.KeyS), largeKeySwitch == 1 ? LargeKeyFont : SmallKeyFont, drawRect, TextColor);

				static Rect SquareAtCircleEdge(AngleF squareDirection, float squareSide) {
					var squareCenter = Vector2.Zero.WithAngularOffset(squareDirection, 306 + (squareDirection.Cos.Abs() + squareDirection.Sin.Abs()) * squareSide / 2f);

					return Box4.FromCenter(squareCenter, new Vector2(squareSide)).ToRect();
				}

				using (context.PushRotation(player.Angle)) {
					var indicatorBrush = playerSwitch ? IndicatorColor1 : IndicatorColor2;

					var p1 = Vector2.Zero.WithAngularOffset(-player.Radius, 300);
					var p2 = p1.WithAngularOffset(MathF.PI - player.Radius * (1 + player.Focus), 50);

					context.DrawLine(p1, p2, indicatorBrush, 2f);

					p1 = Vector2.Zero.WithAngularOffset(player.Radius, 300);
					p2 = p1.WithAngularOffset(MathF.PI + player.Radius * (1 + player.Focus), 50);

					context.DrawLine(p1, p2, indicatorBrush, 2f);

					context.DrawGeometry(player.Geometry, ShadowColor, hasActiveBall ? 40 : 30, PlayerShadowStrokeStyle);
					context.DrawGeometry(player.Geometry, player.Brush, 10);
				}
			}
		}
	}

	void OnRender_PlayerWin(bool playerSwitch, IDeviceIndependentResourceDictionary applicationResources, IDeviceDependentResourceDictionary deviceResources) =>
		DrawScreenSprite(deviceResources, playerSwitch ? SpriteCoordinates.Player1Wins : SpriteCoordinates.Player2Wins, SpriteCoordinates.WinKey, SpriteCoordinates.WinGradient);

	void DrawScreenSprite(IDeviceDependentResourceDictionary deviceResources, Box4 spriteCoordinates, Box4 keyCoordinates, (Vector2, float) spriteGradientCoordinates) {
		var context = deviceResources.D2DeviceContext;
		var (center, radius) = spriteGradientCoordinates;

		center *= CenterScale;
		radius *= CenterScale;

		BackgroundGradient.Center = center + CenterBox.CenterAnchor - CenterScale * spriteCoordinates.Size / 2;
		BackgroundGradient.RadiusX = radius;
		BackgroundGradient.RadiusY = radius;

		context.FillRectangle(new Rect(Window.Size.X, Window.Size.Y), BackgroundGradient);
		context.DrawBitmap(SpriteTarget.Bitmap, Box4.FromCenter(Window.Size / 2, spriteCoordinates.Size * CenterScale).ToRect(), 1, BitmapInterpolationMode.Linear, spriteCoordinates.ToRect());

		using (context.PushScale(CenterScale)) {
			keyCoordinates.OffsetBy(CenterBox.CenterAnchor / CenterScale - spriteCoordinates.Size / 2);

			context.DrawText(GetKeyName(Key.KeyA), LargeKeyFont, keyCoordinates.ToRect(), TextColor);
			context.DrawText(GetKeyName(Key.KeyD), LargeKeyFont, keyCoordinates.WithOffset(339, 0).ToRect(), TextColor);
		}
	}

	#endregion
}
