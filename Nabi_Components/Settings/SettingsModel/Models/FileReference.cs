using System.Xml.Serialization;

namespace SettingsModel.Models
{
	/// <summary>
	/// Implement a simple file reverence model to allow XML persistence
	/// of a List <seealso cref="FileReference"/> via this class.
	/// </summary>
	public class FileReference
	{
		/// <summary>
		/// Gets/sets the path to a file.
		/// </summary>
		[XmlAttribute(AttributeName = "path")]
		public string path { get; set; }
	}
}
