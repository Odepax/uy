using System;
using System.Diagnostics;
using System.Reactive.Disposables;

struct Clock {
	public float FPS;
	public float SPF;
	public TimeSpan Time;

	Stopwatch Watch;

	public double ElapsedMilliseconds => Watch.Elapsed.TotalMilliseconds;

	public IDisposable Start() {
		Watch = Stopwatch.StartNew();

		return Disposable.Create(Watch.Stop);
	}

	public void Update() {
		var t = Watch.Elapsed;
		SPF = (float) (t - Time).TotalSeconds;
		FPS = 1.0f / SPF;
		Time = t;
	}
}
