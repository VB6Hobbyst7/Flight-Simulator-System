using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace AirPlane
{
    class Timers : System.Windows.Forms.Timer
    {
        private Timer _timer;
        private int _interval;

        public Timer Timer
        {
            get
            {
                return _timer;
            }
        }
      
        public int  interval
        {

            get
            {
                return _interval;
            }
            set
            {
                _interval = value;
            }
        }

        public Timers()
        {
            _timer = new Timer();
        }
        public Timers(Timer timer)
        {
            _timer = timer;
        }

        public override bool Enabled
        {
            get
            {
                return base.Enabled;
            }
            set
            {
                base.Interval = _interval;
                base.Enabled = value;
            }
        }
    }
}
