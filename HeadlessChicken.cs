using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Robocode;

namespace TizzleTazzle {
    class HeadlessChicken : Robot {
        public override void Run() {
            var rng = new Random();
            while (true) {
                this.Ahead(100);
                this.TurnRight(rng.Next(90));
            }
        }
    }
}
