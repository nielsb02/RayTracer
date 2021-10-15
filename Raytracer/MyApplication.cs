#region Packages

using System.Diagnostics;

#endregion Packages

namespace Raytracer
{
    /// <summary>
    ///     MyApplication is the I/O to the program
    ///     It controls the raytracer which itself will generate the output
    ///     It also keeps track of the tick time and shows it to the user
    /// </summary>
    internal class MyApplication
    {
        public bool DebugMode;
        public bool HasResized;

        public Raytracer Raytracer;

        // member variables
        public Surface Screen;

        private Stopwatch _timer;
        public bool Update;
        private int _updateCount;

        /// <summary>
        ///     Initializes MyApplication
        /// </summary>
        public void Init()
        {
            Raytracer = new Raytracer(Screen);
            DebugMode = false;
            _timer = new Stopwatch();
            Update = true;
            _updateCount = 0;
        }

        /// <summary>
        ///     Runs every time a new frame is needed
        ///     Will run raytracer and keep track of the runtime
        /// </summary>
        public void Tick()
        {
            _timer.Start();
            if (HasResized) Raytracer = new Raytracer(Screen);

            if (Update || HasResized)
            {
                if (DebugMode)
                    Raytracer.RenderDebug(Screen);
                else
                    Raytracer.RenderSim(Screen);

                _timer.Stop();
                Screen.Print("tickTime: " + _timer.ElapsedMilliseconds + " ms", Screen.width - 220, 5, 0xffffff);
                if (_updateCount > 1)
                {
                    HasResized = false;
                    _updateCount = 0;
                }
                else
                {
                    _updateCount++;
                }
            }

            _timer.Reset();
        }
    }
}