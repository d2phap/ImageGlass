using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using HANDLE = System.IntPtr;

namespace unvell.D2DLib
{
  public class D2DLayer : D2DObject
  {
    public D2DLayer(HANDLE layerHandle)
      : base(layerHandle)
    {
    }
  }
}
