using System;
using System.Collections.Generic;
using System.Text;

namespace ImageGlass.Plugins
{
    /// <summary>
    /// Holds A static instance of global program shtuff
    /// </summary>
    public class PluginManager
    {
        public PluginManager() { } //Constructor

        //What have we done here?	
        public static PluginServices Plugins = new PluginServices();

        /*
            instead of on the frmMain.cs having to declare a PluginService object
            what i've done here is created one in the Global Class.. i've also made
            it static, so we don't have to worry about the object.. It's always gonna
            be there for us and the same object will always be accessed by everything
            else in the program...
			
            So now, everywhere else in this project i can type:
			
                PluginManager.Plugins .... > 
				
            and it will bring up the Plugins object created above.. peachy, eh?
		
        */
                
    }
}
