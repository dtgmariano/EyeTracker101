using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TETCSharpClient;
using TETCSharpClient.Data;
using System.Diagnostics;
using System.Drawing;



namespace Exemplo1
{
    class EyeTribe: IGazeListener
    {
        private int lookX = 0;
        private int lookY = 0;

        public EyeTribe()
        {
            GazeManager.Instance.Activate(GazeManager.ApiVersion.VERSION_1_0, GazeManager.ClientMode.Push);
            GazeManager.Instance.AddGazeListener(this);
        }

        public void Shutdown()
        {
            GazeManager.Instance.Deactivate();
        }

        public void OnGazeUpdate(GazeData gazeData)
        {
            lookX = (int)gazeData.SmoothedCoordinates.X;
            lookY = (int)gazeData.SmoothedCoordinates.Y;
        }

        //public Point GetCoordinates()
        //{
        //    return new Point(lookX, lookY);
        //    //return Cursor.Position;
        //}
    }
}
