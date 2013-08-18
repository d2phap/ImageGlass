/*
ImageGlass Project - Image viewer for Windows
Copyright (C) 2013 DUONG DIEU PHAP
Project homepage: http://imageglass.org

This program is free software: you can redistribute it and/or modify
it under the terms of the GNU General Public License as published by
the Free Software Foundation, either version 3 of the License, or
(at your option) any later version.

This program is distributed in the hope that it will be useful,
but WITHOUT ANY WARRANTY; without even the implied warranty of
MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
GNU General Public License for more details.

You should have received a copy of the GNU General Public License
along with this program.  If not, see <http://www.gnu.org/licenses/>.
*/

using System;

namespace ImageGlass
{
	/// <summary>
	/// Holds A static instance of global program shtuff
	/// </summary>
	public class Global
	{
		public Global(){} //Constructor
		
		//What have we done here?	
        public static ImageGlass.Plugins.PluginServices Plugins = new Plugins.PluginServices();
		
		/*
			instead of on the frmMain.cs having to declare a PluginService object
			what i've done here is created one in the Global Class.. i've also made
			it static, so we don't have to worry about the object.. It's always gonna
			be there for us and the same object will always be accessed by everything
			else in the program...
			
			So now, everywhere else in this project i can type:
			
				Global.Plugins .... > 
				
			and it will bring up the Plugins object created above.. peachy, eh?
		
		*/
	}
}
