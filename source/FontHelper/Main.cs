using System;
using TP.CS.EventsBus;
using TP.CS.EventsBus.Events;

namespace TP.CS.Registry.FontHelper
{
    public class FontHelperExe
    {
        public static void Main(string[] args)
        {

            EventBus current_bus = EventBus.PRIMARY;

            Fonts.isSelfContained = true;

            current_bus.Scan(typeof(Fonts));
            current_bus.post(new StartupEvent());


            Console.WriteLine("Font Helper execution complete, press any key to exit");
            Console.ReadKey();
        }
    }
}
