using System;

namespace Gate.Dock.DockWidget
{
   /// <summary>
   ///
   /// </summary>
   [Flags]
   public enum GateDockWidgetStateFlags
   {
      invisible = 0x0,
      
      floating = 0x10,
      dock = 0x20,
      group = 0x40,

      left = 0x1,
      right = 0x2,
      up = 0x4,
      down = 0x8,

      dock_left = dock | left,
      dock_right = dock | right,
      dock_up = dock | up,
      dock_down = dock | down,

      group_left = group | left,
      group_right = group | right,
      group_up = group | up,
      group_down = group | down,

      tabbed = 0x80,
   }
}
