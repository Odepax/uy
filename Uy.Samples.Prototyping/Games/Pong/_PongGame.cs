using LinqToYourDoom;
using Microsoft.Extensions.Hosting;
using System;
using System.Numerics;
using System.Reactive.Disposables;
using Uy;
using Vortice.Direct2D1;
using Vortice.DirectWrite;
using Vortice.Mathematics;
using static Measure;

#pragma warning disable IDE0079
#pragma warning disable CS8618
#pragma warning disable IDE0044
#pragma warning disable IDE0060
class _PongGame : Game, IDisposable {
	IHostApplicationLifetime Application;
	IDeviceIndependentResourceDictionary ApplicationResources;

	public _PongGame(IHostApplicationLifetime application, IDeviceIndependentResourceDictionary applicationResources) {
		Application = application;
		ApplicationResources = applicationResources;
	}

	public void Dispose() {
		Player1Geometry?.Dispose();
		Player2Geometry?.Dispose();
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

	#region Physics resources

	enum State {
		Lobby, InGame, Player1Wins, Player2Wins
	}

	struct Player {
		public double Speed;
		public double Angle;
		public double Radius;
		public double Focus;
	}

	struct Ball {
		public double Speed;
		public Double2 Velocity;
		public Double2 Position;
		public double Radius;
		public bool Turn;
	}

	delegate void PowerUpApplyAction(bool turn, ref Player player, ref Player otherPlayer);

	struct PowerUp {
		public static double Speed = 0.15 / S;
		public static double Radius = 0.05;

		public Double2 Velocity;
		public Double2 Position;
		public bool Turn;

		public ID2D1ImageBrush Icon;
		public PowerUpApplyAction Apply;

		public PowerUp() {
			Velocity = default;
			Position = default;
			Turn = default;

			Icon = default!;
			Apply = default!;
		}
	}

	bool Lobby = true;
	Player Player1 = new();
	Player Player2 = new();
	ValueList<Ball> Balls = new();
	ValueList<PowerUp> PowerUps = new();

	PowerUp PowerUp_IncreasePlayerSpeed = new();
	PowerUp PowerUp_DecreasePlayerSpeed = new();
	PowerUp PowerUp_IncreasePlayerRadius = new();
	PowerUp PowerUp_DecreasePlayerRadius = new();
	PowerUp PowerUp_IncreasePlayerFocus = new();
	PowerUp PowerUp_DecreasePlayerFocus = new();
	PowerUp PowerUp_IncreaseBallSpeed = new();
	PowerUp PowerUp_DecreaseBallSpeed = new();
	PowerUp PowerUp_IncreaseBallCount = new();
	PowerUp PowerUp_DecreaseBallCount = new();

	void PowerUp_IncreasePlayerSpeed_method(bool turn, ref Player player, ref Player otherPlayer) {
		if (player.Speed < 0.7 * T / S)
			player.Speed += 0.01 * T / S;
	}

	void PowerUp_DecreasePlayerSpeed_method(bool turn, ref Player player, ref Player otherPlayer) {
		if (0.2 * T / S < player.Speed)
			player.Speed -= 0.01 * T / S;
	}

	void PowerUp_IncreasePlayerRadius_method(bool turn, ref Player player, ref Player otherPlayer) {
		if (player.Radius < 0.07) {
			player.Radius += 0.01;

			if (turn) SyncPlayerGeometry(ref Player1Geometry, (float) Player1.Radius);
			else SyncPlayerGeometry(ref Player2Geometry, (float) Player2.Radius);
		}
	}

	void PowerUp_DecreasePlayerRadius_method(bool turn, ref Player player, ref Player otherPlayer) {
		if (0.01 < player.Radius) {
			player.Radius -= 0.01;

			if (turn) SyncPlayerGeometry(ref Player1Geometry, (float) Player1.Radius);
			else SyncPlayerGeometry(ref Player2Geometry, (float) Player2.Radius);
		}
	}

	void PowerUp_IncreasePlayerFocus_method(bool turn, ref Player player, ref Player otherPlayer) {
		if (player.Focus < 1.2)
			player.Focus += 0.15;
	}

	void PowerUp_DecreasePlayerFocus_method(bool turn, ref Player player, ref Player otherPlayer) {
		if (0.2 < player.Focus)
			player.Focus -= 0.15;
	}

	void PowerUp_IncreaseBallSpeed_method(bool turn, ref Player player, ref Player otherPlayer) {
		for (var i = 0; i < Balls.Count; ++i)
			if (Balls[i].Speed < 1.5 / S)
				Balls[i].Speed += 0.01 / S;
	}

	void PowerUp_DecreaseBallSpeed_method(bool turn, ref Player player, ref Player otherPlayer) {
		for (var i = 0; i < Balls.Count; ++i)
			if (0.5 / S < Balls[i].Speed)
				Balls[i].Speed -= 0.01 / S;
	}
	
	void PowerUp_IncreaseBallCount_method(bool turn, ref Player player, ref Player otherPlayer) {
		var ball = Balls[MathD.Random.UncheckedBetween(0, Balls.Count)];

		var a = MathD.Random.PiAngle();
		var d = ball.Speed;

		ball.Velocity.X = d * Math.Cos(a);
		ball.Velocity.Y = d * Math.Sin(a);
		ball.Turn = MathD.Random.Bool();

		Balls.Add(ball);
	}

	void PowerUp_DecreaseBallCount_method(bool turn, ref Player player, ref Player otherPlayer) {
		if (1 < Balls.Count)
			Balls.RemoveAt(MathD.Random.UncheckedBetween(0, Balls.Count));
	}

	public override void OnLoad(CompositeDisposable disposables) {
		PowerUp_IncreasePlayerSpeed.Apply = PowerUp_IncreasePlayerSpeed_method;
		PowerUp_DecreasePlayerSpeed.Apply = PowerUp_DecreasePlayerSpeed_method;
		PowerUp_IncreasePlayerRadius.Apply = PowerUp_IncreasePlayerRadius_method;
		PowerUp_DecreasePlayerRadius.Apply = PowerUp_DecreasePlayerRadius_method;
		PowerUp_IncreasePlayerFocus.Apply = PowerUp_IncreasePlayerFocus_method;
		PowerUp_DecreasePlayerFocus.Apply = PowerUp_DecreasePlayerFocus_method;
		PowerUp_IncreaseBallSpeed.Apply = PowerUp_IncreaseBallSpeed_method;
		PowerUp_DecreaseBallSpeed.Apply = PowerUp_DecreaseBallSpeed_method;
		PowerUp_IncreaseBallCount.Apply = PowerUp_IncreaseBallCount_method;
		PowerUp_DecreaseBallCount.Apply = PowerUp_DecreaseBallCount_method;
	}

	void ResetGame() {
		Lobby = false;

		Balls.Clear();

		var ball = new Ball();

		var a = MathD.Random.PiAngle();
		var d = MathD.Random.Between(0, 0.5);

		ball.Speed = 0.5 / S;
		ball.Position.X = d * Math.Cos(a);
		ball.Position.Y = d * Math.Sin(a);

		a = MathD.Random.PiAngle();
		d = ball.Speed;

		ball.Velocity.X = d * Math.Cos(a);
		ball.Velocity.Y = d * Math.Sin(a);
		ball.Radius = 0.05;
		ball.Turn = MathD.Random.Bool();

		Balls.Add(ball);

		Player1.Speed = 0.3 * T / S;
		Player1.Angle = a + 0.2 * T;
		Player1.Radius = 0.02 * T;
		Player1.Focus = 0.2;
		
		Player2.Speed = 0.3 * T / S;
		Player2.Angle = a - 0.2 * T;
		Player2.Radius = 0.02 * T;
		Player2.Focus = 0.2;

		SyncPlayerGeometry(ref Player1Geometry, (float) Player1.Radius);
		SyncPlayerGeometry(ref Player2Geometry, (float) Player2.Radius);

		PowerUps.Clear();
	}

	#endregion
	#region Graphics resources - Device-independent

	static class Colors {
		public static Color4 Orchid = 0x9575A3.ToRgb();
		//public static Color4 Tan = 0xA38E75.ToRgb();
		public static Color4 Teal = 0x64D580.ToRgb();
		public static Color4 Blue = 0x32C0EA.ToRgb();
		public static Color4 Gold = 0xFDBE11.ToRgb();
		public static Color4 Orange = 0xF77A34.ToRgb();
		//public static Color4 Dark1 = 0x474649.ToRgb();
		//public static Color4 Dark2 = 0x302E33.ToRgb();
		public static Color4 Dark3 = 0x1F1C21.ToRgb();
		public static Color4 Dark4 = 0x0F0E10.ToRgb();
		public static Color4 Black = 0x000000.ToRgb();
		public static Color4 White = 0xFEFAEC.ToRgb();
	}

	/*

		JetBrains Mono Bold

	*/

	static class SpriteCoordinates {
		public static Rect CirclePong = new(0, 0, 836, 360);
		public static (float X, float Y, float Radius) CirclePongGradient = (593, 71, 916);
		public static Rect Player1Wins = new(0, 380, 690, 490);
		public static Rect Player2Wins = new(0, 870, 690, 490);
		public static (float X, float Y, float Radius) WinGradient = (586, 66, 1001);
		public static Rect LargeKey(int i) => new(690, 380 + 80 * i, 80, 80);
		public static Rect SmallKey(int i) => new(770, 380 + 80 * i, 58, 58);
		public static Rect PowerUp(int i) => new(690, 780 + 48 * i, 48, 48);
	}

	IDWriteTextFormat3 DebugFont;
	IDWriteTextFormat3 DisplayFont;
	IDWriteTextLayout4 Message;
	IDWriteTextLayout4 LobbyMessage;
	IDWriteTextLayout4 Player1WinMessage;
	IDWriteTextLayout4 Player2WinMessage;
	ID2D1PathGeometry1 Player1Geometry;
	ID2D1PathGeometry1 Player2Geometry;
	ID2D1PathGeometry1 QuitGeometry;
	ID2D1PathGeometry1 PlayGeometry;
	ID2D1PathGeometry1 UpArrowGeometry;
	ID2D1PathGeometry1 DownArrowGeometry;

	public override void OnApplicationInit(IDeviceIndependentResourceDictionary applicationResources, CompositeDisposable disposables) {
		using (var font = applicationResources.WriteFactory.CreateTextFormat("JetBrains Mono", 16))
			DebugFont = font.QueryInterface<IDWriteTextFormat3>().DisposeWith(disposables);

		using (var font = applicationResources.WriteFactory.CreateTextFormat(string.Empty, FontWeight.ExtraBold, FontStyle.Italic, 0.16f))
			DisplayFont = font.QueryInterface<IDWriteTextFormat3>().DisposeWith(disposables);

		DisplayFont.TextAlignment = TextAlignment.Center;
		DisplayFont.ParagraphAlignment = ParagraphAlignment.Center;

		using (var layout = applicationResources.WriteFactory.CreateTextLayout("Circle Pong\nQuit      Play", DisplayFont, 2, 2))
			LobbyMessage = layout.QueryInterface<IDWriteTextLayout4>().DisposeWith(disposables);

		using (var layout = applicationResources.WriteFactory.CreateTextLayout("Player #1 Wins!\nQuit      Play", DisplayFont, 2, 2))
			Player1WinMessage = layout.QueryInterface<IDWriteTextLayout4>().DisposeWith(disposables);

		using (var layout = applicationResources.WriteFactory.CreateTextLayout("Player #2 Wins!\nQuit      Play", DisplayFont, 2, 2))
			Player2WinMessage = layout.QueryInterface<IDWriteTextLayout4>().DisposeWith(disposables);

		Message = LobbyMessage;

		QuitGeometry = applicationResources.D2Factory.CreatePathGeometry().DisposeWith(disposables);
		{
			using var path = QuitGeometry.Open();

			path.BeginFigure(new Vector2(-0.45f, 0.33f), FigureBegin.Filled);
			path.AddLine(new Vector2(-0.35f, 0.26f));
			path.AddLine(new Vector2(-0.35f, 0.30f));
			path.AddLine(new Vector2(-0.16f, 0.33f));
			path.AddLine(new Vector2(-0.35f, 0.36f));
			path.AddLine(new Vector2(-0.35f, 0.40f));
			path.EndFigure(FigureEnd.Closed);

			path.Close();
		}

		PlayGeometry = applicationResources.D2Factory.CreatePathGeometry().DisposeWith(disposables);
		{
			using var path = PlayGeometry.Open();

			path.BeginFigure(new Vector2(0.57f, 0.16f), FigureBegin.Filled);
			path.AddLine(new Vector2(0.08f, 0.19f));
			path.AddLine(new Vector2(0.12f, 0.45f));
			path.AddLine(new Vector2(0.52f, 0.50f));
			path.EndFigure(FigureEnd.Closed);

			path.BeginFigure(new Vector2(0.45f, 0.33f), FigureBegin.Filled);
			path.AddLine(new Vector2(0.35f, 0.26f));
			path.AddLine(new Vector2(0.35f, 0.30f));
			path.AddLine(new Vector2(0.17f, 0.33f));
			path.AddLine(new Vector2(0.35f, 0.36f));
			path.AddLine(new Vector2(0.35f, 0.40f));
			path.EndFigure(FigureEnd.Closed);

			path.Close();
		}

		UpArrowGeometry = applicationResources.D2Factory.CreatePathGeometry().DisposeWith(disposables);
		{
			using var path = UpArrowGeometry.Open();

			path.BeginFigure(new Vector2(0, -0.05f), FigureBegin.Filled);
			path.AddLine(new Vector2(0.04f, 0));
			path.AddLine(new Vector2(0.01f, 0));
			path.AddLine(new Vector2(0, 0.05f));
			path.AddLine(new Vector2(-0.01f, 0f));
			path.AddLine(new Vector2(-0.04f, 0f));
			path.EndFigure(FigureEnd.Closed);

			path.Close();
		}

		DownArrowGeometry = applicationResources.D2Factory.CreatePathGeometry().DisposeWith(disposables);
		{
			using var path = DownArrowGeometry.Open();

			path.BeginFigure(new Vector2(0, 0.05f), FigureBegin.Filled);
			path.AddLine(new Vector2(0.04f, 0));
			path.AddLine(new Vector2(-0.04f, 0));
			path.EndFigure(FigureEnd.Closed);

			path.Close();
		}
	}

	void SyncPlayerGeometry(ref ID2D1PathGeometry1 geometry, float radius) {
		geometry?.Dispose();

		geometry = ApplicationResources.D2Factory.CreatePathGeometry();
		{
			using var path = geometry.Open();

			var x = MathF.Cos(-radius);
			var y = MathF.Sin(-radius);

			path.BeginFigure(new Vector2(x, y), FigureBegin.Hollow);

			x = MathF.Cos(+radius);
			y = MathF.Sin(+radius);

			path.AddArc(new ArcSegment(new Vector2(x, y), new Size(1), 0, SweepDirection.Clockwise, ArcSize.Small));
			path.EndFigure(FigureEnd.Open);
			path.Close();
		}
	}

	#endregion
	#region Graphics resources - Device-dependent

	ID2D1SolidColorBrush TextColor;
	ID2D1SolidColorBrush CircleColor;
	ID2D1SolidColorBrush Player1Color;
	ID2D1SolidColorBrush Player2Color;
	ID2D1SolidColorBrush QuitColor;
	ID2D1SolidColorBrush PlayColor;
	ID2D1SvgDocument PowerUpsSvgDocument;

	public override void OnDeviceInit(IDeviceIndependentResourceDictionary applicationResources, IDeviceDependentResourceDictionary deviceResources, CompositeDisposable disposables) {
		var context = deviceResources.D2DeviceContext;

		TextColor = context.CreateSolidColorBrush(Colors.White).DisposeWith(disposables);
		CircleColor = context.CreateSolidColorBrush(Colors.Orchid).DisposeWith(disposables);
		Player1Color = context.CreateSolidColorBrush(Colors.Gold).DisposeWith(disposables);
		Player2Color = context.CreateSolidColorBrush(Colors.Blue).DisposeWith(disposables);
		QuitColor = context.CreateSolidColorBrush(Colors.Orchid).DisposeWith(disposables);
		PlayColor = context.CreateSolidColorBrush(Colors.Teal).DisposeWith(disposables);

		using var resourceStream = typeof(PongGame).Assembly.GetEmbeddedResourceStream("PongSprite.svg");
		using var svgStream = applicationResources.WicFactory.CreateStream(resourceStream);

		PowerUpsSvgDocument = context.CreateSvgDocument(svgStream, new(5, 2)).DisposeWith(disposables);


	}

	#endregion
	#region Graphics resources - Window size-dependent

	ID2D1BitmapRenderTarget OffscreenTarget;
	ID2D1DeviceContext6 OffscreenContext;

	public override void OnResizeInit(IDeviceIndependentResourceDictionary applicationResources, IDeviceDependentResourceDictionary deviceResources, CompositeDisposable disposables) {
		var context = deviceResources.D2DeviceContext;

		var transformScale = 2 / (MathF.Min(Window.Size.X, Window.Size.Y) - 100);
		var powerUpScaledSize = (float) PowerUp.Radius * (MathF.Min(Window.Size.X, Window.Size.Y) - 100);
		var x = powerUpScaledSize * 5;
		var y = powerUpScaledSize * 2;

		OffscreenTarget = context.CreateCompatibleRenderTarget(new Size(x, y)).DisposeWith(disposables);
		OffscreenContext = OffscreenTarget.QueryInterface<ID2D1DeviceContext6>().DisposeWith(disposables);

		OffscreenContext.BeginDraw();
		{
			using (OffscreenContext.PushScale(powerUpScaledSize))
				OffscreenContext.DrawSvgDocument(PowerUpsSvgDocument);
		}
		OffscreenContext.EndDraw();

		ID2D1ImageBrush PowerUpBrush(int x, int y) => context.CreateImageBrush(OffscreenTarget.Bitmap, new ImageBrushProperties {
			SourceRectangle = new Rect(x * powerUpScaledSize, y * powerUpScaledSize, powerUpScaledSize, powerUpScaledSize)
		}).Also(brush => {
			var t = brush.Transform;

			t.M11 = t.M22 = transformScale;

			brush.Transform = t;
		}).DisposeWith(disposables);

		PowerUp_IncreasePlayerSpeed.Icon = PowerUpBrush(0, 0);
		PowerUp_DecreasePlayerSpeed.Icon = PowerUpBrush(0, 1);
		PowerUp_IncreasePlayerRadius.Icon = PowerUpBrush(1, 0);
		PowerUp_DecreasePlayerRadius.Icon = PowerUpBrush(1, 1);
		PowerUp_IncreasePlayerFocus.Icon = PowerUpBrush(2, 0);
		PowerUp_DecreasePlayerFocus.Icon = PowerUpBrush(2, 1);
		PowerUp_IncreaseBallSpeed.Icon = PowerUpBrush(3, 0);
		PowerUp_DecreaseBallSpeed.Icon = PowerUpBrush(3, 1);
		PowerUp_IncreaseBallCount.Icon = PowerUpBrush(4, 0);
		PowerUp_DecreaseBallCount.Icon = PowerUpBrush(4, 1);
	}

	#endregion
	#region Physics time

	double UpdateDuration;
	TimeSpan NextPowerUp;

	public override void OnUpdate() {
		var _updateStartTime = Clock.ElapsedMilliseconds;
		{
			if (Lobby) {
				var direction = Stick1.Vector.X + Stick2.Vector.X;

				if (direction < 0) Application.StopApplication();
				else if (0 < direction) ResetGame(); // Play!
			}

			else {
				Player1.Angle += Stick1.Vector.Y * Player1.Speed * Clock.SPF;
				Player2.Angle += Stick2.Vector.Y * Player2.Speed * Clock.SPF;

				var a = 0.0;
				var d = 0.0;

				for (var i = 0; i < Balls.Count; ++i) {
					ref var ball = ref Balls[i];

					ball.Position.X += ball.Velocity.X * Clock.SPF;
					ball.Position.Y += ball.Velocity.Y * Clock.SPF;

					// We're out of the circle!
					if (1 - ball.Radius < ball.Position.X * ball.Position.X + ball.Position.Y * ball.Position.Y) {
						var dirToBall = Math.Atan2(ball.Position.Y, ball.Position.X);
						var (dirToPlayer, playerRadius, playerFocus) = ball.Turn ? (Player1.Angle, Player1.Radius, Player1.Focus) : (Player2.Angle, Player2.Radius, Player2.Focus);
						var distToCenter = ClampAngle(dirToPlayer - dirToBall);

						// The ball is caught by the player who's it's the turn.
						// Bounce back in the circle and continue!
						if (Math.Abs(distToCenter) < playerRadius) {
							var dirOfVelocity = Math.Atan2(ball.Velocity.Y, ball.Velocity.X);

							ball.Position.X = (0.99 - ball.Radius) * Math.Cos(dirToBall);
							ball.Position.Y = (0.99 - ball.Radius) * Math.Sin(dirToBall);

							a = (dirToBall + 0.5 * T + dirToBall - dirOfVelocity) - playerFocus * distToCenter;
							d = ball.Speed;

							ball.Velocity.X = d * Math.Cos(a);
							ball.Velocity.Y = d * Math.Sin(a);

							ball.Turn = !ball.Turn;
						}

						// The player didn't catch the ball.
						// GAME OVER!
						else {
							Balls.RemoveAt(i);

							if (Balls.Count == 0) {
								Lobby = true;
								Message = ball.Turn ? Player2WinMessage : Player1WinMessage;
							}
						}
					}
				}

				if (NextPowerUp <= Clock.Time) {
					NextPowerUp = Clock.Time + TimeSpan.FromSeconds(10);

					var powerUp = MathD.Random.In(new[] {
						PowerUp_IncreasePlayerSpeed,
						PowerUp_DecreasePlayerSpeed,
						PowerUp_IncreasePlayerRadius,
						PowerUp_DecreasePlayerRadius,
						PowerUp_IncreasePlayerFocus,
						PowerUp_DecreasePlayerFocus,
						PowerUp_IncreaseBallSpeed,
						PowerUp_DecreaseBallSpeed,
						PowerUp_IncreaseBallCount,
						PowerUp_DecreaseBallCount,
					});

					a = MathD.Random.PiAngle();
					d = MathD.Random.Between(0, 0.5);

					powerUp.Position.X = d * Math.Cos(a);
					powerUp.Position.Y = d * Math.Sin(a);

					a = MathD.Random.PiAngle();
					d = PowerUp.Speed;

					powerUp.Velocity.X = d * Math.Cos(a);
					powerUp.Velocity.Y = d * Math.Sin(a);
					powerUp.Turn = MathD.Random.Bool();

					PowerUps.Add(powerUp);
				}

				for (var i = 0; i < PowerUps.Count; ++i) {
					ref var powerUp = ref PowerUps[i];

					powerUp.Position.X += powerUp.Velocity.X * Clock.SPF;
					powerUp.Position.Y += powerUp.Velocity.Y * Clock.SPF;

					// We're out of the circle!
					if (1 - PowerUp.Radius < powerUp.Position.X * powerUp.Position.X + powerUp.Position.Y * powerUp.Position.Y) {
						var dirToPowerUp = Math.Atan2(powerUp.Position.Y, powerUp.Position.X);
						var (dirToPlayer, playerRadius) = powerUp.Turn ? (Player1.Angle, Player1.Radius) : (Player2.Angle, Player2.Radius);

						// The power-up is caught by the player whom it's destined for.
						// Activate the power-up!
						if (Math.Abs((double) ClampAngle(dirToPlayer - dirToPowerUp)) < playerRadius)
							if (powerUp.Turn) powerUp.Apply(powerUp.Turn, ref Player1, ref Player2);
							else powerUp.Apply(powerUp.Turn, ref Player2, ref Player1);

						PowerUps.RemoveAt(i);
					}
				}
			}
		}
		var _updateEndTime = Clock.ElapsedMilliseconds;

		UpdateDuration = _updateEndTime - _updateStartTime;
	}

	// Adapted from:
	// https://stackoverflow.com/a/22949941
	// https://stackoverflow.com/questions/2320986/easy-way-to-keeping-angles-between-179-and-180-degrees#answer-22949941
	static double ClampAngle(double a) => a - Math.Floor((a + 0.5 * T) / T) * T;

	#endregion
	#region Drawing time

	public override void OnRender(IDeviceIndependentResourceDictionary applicationResources, IDeviceDependentResourceDictionary deviceResources) {
		var context = deviceResources.D2DeviceContext;

		context.Clear(Colors.Black);
		{
			using (context.PushTranslation(Window.Size / 2))
			using (context.PushScale((MathF.Min(Window.Size.X, Window.Size.Y) - 100) / 2)) {
				if (Lobby) {
					context.DrawGeometry(QuitGeometry, QuitColor, 0.04f);
					context.FillGeometry(PlayGeometry, PlayColor);
					context.DrawTextLayout(new Vector2(-1, -1), Message, TextColor);
				}

				else {
					context.DrawEllipse(new Ellipse(Vector2.Zero, 1, 1), CircleColor, 0.01f);

					for (var i = 0; i < Balls.Count; ++i) {
						var ball = Balls[i];

						context.FillEllipse(new Ellipse((Vector2) ball.Position, (float) ball.Radius, (float) ball.Radius), ball.Turn ? Player1Color : Player2Color);
					}

					for (var i = 0; i < PowerUps.Count; ++i) {
						var powerUp = PowerUps[i];

						var t = powerUp.Icon.Transform;
						t.Translation = (Vector2) powerUp.Position - new Vector2((float) PowerUp.Radius);
						powerUp.Icon.Transform = t;

						context.DrawEllipse(new Ellipse((Vector2) powerUp.Position, (float) PowerUp.Radius, (float) PowerUp.Radius), powerUp.Turn ? Player1Color : Player2Color, 0.02f);
						context.FillEllipse(new Ellipse((Vector2) powerUp.Position, (float) PowerUp.Radius, (float) PowerUp.Radius), powerUp.Icon);
					}

					using (context.PushRotation((float) Player1.Angle))
						context.DrawGeometry(Player1Geometry, Player1Color, 0.02f);

					RenderMovementHints(Player1.Angle, Player1.Radius, Player1Color);

					using (context.PushRotation((float) Player2.Angle))
						context.DrawGeometry(Player2Geometry, Player2Color, 0.02f);

					RenderMovementHints(Player2.Angle, Player2.Radius, Player2Color);

					void RenderMovementHints(double playerAngle, double playerRadius, ID2D1SolidColorBrush playerColor) {
						var x = 1.08 * Math.Cos(playerAngle - playerRadius);
						var y = 1.08 * Math.Sin(playerAngle - playerRadius);

						using (context.PushTranslation((float) x, (float) y))
							context.DrawGeometry(UpArrowGeometry, playerColor, 0.01f);

						x = 1.08 * Math.Cos(playerAngle + playerRadius);
						y = 1.08 * Math.Sin(playerAngle + playerRadius);

						using (context.PushTranslation((float) x, (float) y))
							context.DrawGeometry(DownArrowGeometry, playerColor, 0.01f);
					}
				}
			}
		}
		context.DrawText($"{ Clock.FPS :0} FPS - Update: { UpdateDuration :00.00}ms", DebugFont, Window.Box.Deflated(16).ToRect(), TextColor);
	}

	#endregion
}
