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
using System.IO;
using System.Reflection;
using ImageGlass.Plugins;
using System.Linq;

namespace ImageGlass.Plugins
{
    /// <summary>
    /// Summary description for PluginServices.
    /// </summary>
    public class PluginServices : IPluginHost   //<--- Notice how it inherits IPluginHost interface!
    {
        /// <summary>
        /// Constructor of the Class
        /// </summary>
        public PluginServices()
        {
        }

        private Types.AvailablePlugins colAvailablePlugins = new Types.AvailablePlugins();

        /// <summary>
        /// A Collection of all Plugins Found and Loaded by the FindPlugins() Method
        /// </summary>
        public Types.AvailablePlugins AvailablePlugins
        {
            get { return colAvailablePlugins; }
            set { colAvailablePlugins = value; }
        }

        /// <summary>
        /// Searches the Application's Startup Directory for Plugins
        /// </summary>
        public void FindPlugins()
        {
            FindPlugins(AppDomain.CurrentDomain.BaseDirectory);
        }
        /// <summary>
        /// Searches the passed Path for Plugins
        /// </summary>
        /// <param name="Path">Directory to search for Plugins in</param>
        public void FindPlugins(string Path)
        {
            //First empty the collection, we're reloading them all
            colAvailablePlugins.Clear();

            //Go through all the files in the plugin directory
            foreach (string fileOn in Directory.GetFiles(Path))
            {
                FileInfo file = new FileInfo(fileOn);

                //Preliminary check, must be .dll
                if (file.Extension.Equals(".dll"))
                {
                    //Add the 'plugin'
                    AddPlugin(fileOn);
                }
            }
        }

        /// <summary>
        /// Unloads and Closes all AvailablePlugins
        /// </summary>
        public void ClosePlugins()
        {
            foreach (Types.AvailablePlugin pluginOn in colAvailablePlugins)
            {
                //Close all plugin instances
                //We call the plugins Dispose sub first incase it has to do 
                //Its own cleanup stuff
                pluginOn.Instance.Dispose();

                //After we give the plugin a chance to tidy up, get rid of it
                pluginOn.Instance = null;
            }

            //Finally, clear our collection of available plugins
            colAvailablePlugins.Clear();
        }

        private void AddPlugin(string FileName)
        {
            //Create a new assembly from the plugin file we're adding..
            Assembly pluginAssembly = Assembly.LoadFile(FileName);

            //Next we'll loop through all the Types found in the assembly
            foreach (Type pluginType in pluginAssembly.GetTypes())
            {
                if (pluginType.IsPublic) //Only look at public types
                {
                    if (!pluginType.IsAbstract)  //Only look at non-abstract types
                    {
                        //Gets a type object of the interface we need the plugins to match
                        Type typeInterface = pluginType.GetInterface("ImageGlass.Plugins.IPlugin", true);

                        //Make sure the interface we want to use actually exists
                        if (typeInterface != null)
                        {
                            //Create a new available plugin since the type implements the IPlugin interface
                            Types.AvailablePlugin newPlugin = new Types.AvailablePlugin();

                            //Set the filename where we found it
                            newPlugin.AssemblyPath = FileName;

                            //Create a new instance and store the instance in the collection for later use
                            //We could change this later on to not load an instance.. we have 2 options
                            //1- Make one instance, and use it whenever we need it.. it's always there
                            //2- Don't make an instance, and instead make an instance whenever we use it, then close it
                            //For now we'll just make an instance of all the plugins
                            newPlugin.Instance = (IPlugin)Activator.CreateInstance(pluginAssembly.GetType(pluginType.ToString()));

                            //Set the Plugin's host to this class which inherited IPluginHost
                            newPlugin.Instance.Host = this;

                            //Call the initialization sub of the plugin
                            newPlugin.Instance.Initialize();

                            //Add the new plugin to our collection here
                            colAvailablePlugins.Add(newPlugin);

                            //cleanup a bit
                            newPlugin = null;
                        }

                        typeInterface = null; //Mr. Clean			
                    }
                }
            }

            pluginAssembly = null; //more cleanup
        }

        ///// <summary>
        ///// Displays a feedback dialog from the plugin
        ///// </summary>
        ///// <param name="Feedback">String message for feedback</param>
        ///// <param name="Plugin">The plugin that called the feedback</param>
        public void Feedback(string Feedback, IPlugin Plugin)
        {
        //    //This sub makes a new feedback form and fills it out
        //    //With the appropriate information
        //    //This method can be called from the actual plugin with its Host Property

        //    System.Windows.Forms.Form newForm = null;
        //    frmFeedback newFeedbackForm = new frmFeedback();

        //    //Here we set the frmFeedback's properties that i made custom
        //    newFeedbackForm.PluginAuthor = "By: " + Plugin.Author;
        //    newFeedbackForm.PluginDesc = Plugin.Description;
        //    newFeedbackForm.PluginName = Plugin.Name;
        //    newFeedbackForm.PluginVersion = Plugin.Version;
        //    newFeedbackForm.Feedback = Feedback;

        //    //We also made a Form object to hold the frmFeedback instance
        //    //If we were to declare if not as  frmFeedback object at first,
        //    //We wouldn't have access to the properties we need on it
        //    newForm = newFeedbackForm;
        //    newForm.ShowDialog();

        //    newFeedbackForm = null;
        //    newForm = null;

        }


    }
    

    namespace Types
    {
        /// <summary>
        /// Collection for AvailablePlugin Type
        /// </summary>
        public class AvailablePlugins : System.Collections.CollectionBase
        {
            //A Simple Home-brew class to hold some info about our Available Plugins

            /// <summary>
            /// Add a Plugin to the collection of Available plugins
            /// </summary>
            /// <param name="pluginToAdd">The Plugin to Add</param>
            public void Add(Types.AvailablePlugin pluginToAdd)
            {
                List.Add(pluginToAdd);
            }

            /// <summary>
            /// Remove a Plugin to the collection of Available plugins
            /// </summary>
            /// <param name="pluginToRemove">The Plugin to Remove</param>
            public void Remove(Types.AvailablePlugin pluginToRemove)
            {
                List.Remove(pluginToRemove);
            }

            /// <summary>
            /// Finds a plugin in the available Plugins
            /// </summary>
            /// <param name="pluginNameOrPath">The name or File path of the plugin to find</param>
            /// <returns>Available Plugin, or null if the plugin is not found</returns>
            public Types.AvailablePlugin Find(string pluginNameOrPath)
            {
                Types.AvailablePlugin toReturn = null;

                //Loop through all the plugins
                foreach (Types.AvailablePlugin pluginOn in List)
                {
                    //Find the one with the matching name or filename
                    if ((pluginOn.Instance.Name.Equals(pluginNameOrPath)) || pluginOn.AssemblyPath.Equals(pluginNameOrPath))
                    {
                        toReturn = pluginOn;
                        break;
                    }
                }
                return toReturn;
            }
        }

        /// <summary>
        /// Data Class for Available Plugin.  Holds and instance of the loaded Plugin, as well as the Plugin's Assembly Path
        /// </summary>
        public class AvailablePlugin
        {
            //This is the actual AvailablePlugin object.. 
            //Holds an instance of the plugin to access
            //ALso holds assembly path... not really necessary
            private IPlugin myInstance = null;
            private string myAssemblyPath = "";

            public IPlugin Instance
            {
                get { return myInstance; }
                set { myInstance = value; }
            }
            public string AssemblyPath
            {
                get { return myAssemblyPath; }
                set { myAssemblyPath = value; }
            }
        }
    }
}

