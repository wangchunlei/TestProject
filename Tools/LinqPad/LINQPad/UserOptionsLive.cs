namespace LINQPad
{
    using System;
    using System.Runtime.CompilerServices;
    using System.Threading;

    internal class UserOptionsLive : OptionsLive
    {
        public static readonly UserOptionsLive Instance = new UserOptionsLive();

        public event EventHandler AutoScrollResultsChanged;

        public event EventHandler ExecutionTrackingDisabledChanged;

        public event EventHandler OptimizeQueriesChanged;

        [CompilerGenerated]
        private static void <.ctor>b__0(object sender, EventArgs e)
        {
        }

        [CompilerGenerated]
        private static void <.ctor>b__1(object sender, EventArgs e)
        {
        }

        [CompilerGenerated]
        private static void <.ctor>b__2(object sender, EventArgs e)
        {
        }

        public bool AutoScrollResults
        {
            get
            {
                return base.Read<bool>("AutoScrollResults");
            }
            set
            {
                if (base.Write("AutoScrollResults", value))
                {
                    this.AutoScrollResultsChanged(this, EventArgs.Empty);
                }
            }
        }

        public override string BaseFolder
        {
            get
            {
                return Program.UserDataFolder;
            }
        }

        public bool ExecutionTrackingDisabled
        {
            get
            {
                return base.Read<bool>("ExecutionTrackingDisabled");
            }
            set
            {
                if (base.Write("ExecutionTrackingDisabled", value))
                {
                    this.ExecutionTrackingDisabledChanged(this, EventArgs.Empty);
                }
            }
        }

        public bool OptimizeQueries
        {
            get
            {
                return base.Read<bool>("OptimizeQueries");
            }
            set
            {
                if (base.Write("OptimizeQueries", value))
                {
                    this.OptimizeQueriesChanged(this, EventArgs.Empty);
                }
            }
        }
    }
}

