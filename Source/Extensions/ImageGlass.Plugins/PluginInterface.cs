using System;

namespace ImageGlass.Plugins//PluginInterface
{
	public interface IPlugin
	{
		IPluginHost Host {get;set;}
		
		string Name {get;}
		string Description {get;}
		string Author {get;}
		string Version {get;}
		
		System.Windows.Forms.UserControl MainInterface {get;}
		
		void Initialize();
		void Dispose();
	
	}
	
	public interface IPluginHost
	{
		void Feedback(string Feedback, IPlugin Plugin);	
	}
}
