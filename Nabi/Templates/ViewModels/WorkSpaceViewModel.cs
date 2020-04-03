﻿using Nabi.Templates.ViewModels.Tools;
using Microsoft.Win32;
using Nabi.Templates.ViewModels.AD;
using Nabi.Templates.ViewModels.Interfaces;
using Nabi.ViewModels.Base;
using MWindowInterfacesLib.MsgBox.Enums;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using ICSharpCode.AvalonEdit.Document;
using MLib.Interfaces;
using System.Diagnostics;
using Nabi.Templates.ViewModels.Views;

namespace Nabi.Templates.ViewModels
{

	#region Helper Test Classes
	/// <summary>
	/// This class is uses to create a type safe list
	/// of enumeration members for selection in combobox.
	/// </summary>
	public class MessageImageCollection
	{
		public string Name { get; set; }
		public MsgBoxImage EnumKey { get; set; }
	}

	/// <summary>
	/// Test class to enumerate over message box buttons enumeration.
	/// </summary>
	public class MessageButtonCollection
	{
		public string Name { get; set; }
		public MsgBoxButtons EnumKey { get; set; }
	}

	/// <summary>
	/// Test class to enumerate over message box result enumeration.
	/// The <seealso cref="MsgBoxResult"/> enumeration is used to define
	/// a default button (if any).
	/// </summary>
	public class MessageResultCollection
	{
		public string Name { get; set; }
		public MsgBoxResult EnumKey { get; set; }
	}

	/// <summary>
	/// Test class to enumerate over languages (and their locale) that
	/// are supported with specific (non-English) button and tool tip strings.
	/// 
	/// The class definition is based on BCP 47 which in turn is used to
	/// set the UI and thread culture (which in turn selects the correct string resource in MsgBox assembly).
	/// </summary>
	public class LanguageCollection
	{
		public string Language { get; set; }
		public string Locale { get; set; }
		public string Name { get; set; }

		/// <summary>
		/// Get BCP47 language tag for this language
		/// See also http://en.wikipedia.org/wiki/IETF_language_tag
		/// </summary>
		public string BCP47
		{
			get
			{
				if (string.IsNullOrEmpty(this.Locale) == false)
					return String.Format("{0}-{1}", this.Language, this.Locale);
				else
					return String.Format("{0}", this.Language);
			}
		}

		/// <summary>
		/// Get BCP47 language tag for this language
		/// See also http://en.wikipedia.org/wiki/IETF_language_tag
		/// </summary>
		public string DisplayName
		{
			get
			{
				return String.Format("{0} {1}", this.Name, this.BCP47);
			}
		}
	}
	#endregion Helper Test Classes

	public enum ToggleEditorOption
	{
		WordWrap = 0,
		ShowLineNumber = 1,
		ShowEndOfLine = 2,
		ShowSpaces = 3,
		ShowTabs = 4
	}

	/// <summary>
	/// The WorkSpaceViewModel implements AvalonDock demo specific properties, events and methods.
	/// </summary>
	internal class WorkSpaceViewModel : Nabi.ViewModels.Base.ViewModelBase, IWorkSpaceViewModel
	{
		#region private fields
		private ObservableCollection<FileViewModel> _files = new ObservableCollection<FileViewModel>();
		private ReadOnlyObservableCollection<FileViewModel> _readonyFiles = null;
		private ToolViewModel[] _tools = null;

		private ICommand _openCommand = null;
		private ICommand _newCommand = null;
		private ICommand _toggleEditorOptionCommand = null;


		private FileStatsViewModel _fileStats = null;
		private ColorPickerViewModel _ColorPicker = null;
		private Output_ViewModel _output;
		private Log_ViewModel _log;

		private FileViewModel _activeDocument = null;
		#endregion private fields

		#region constructors
		/// <summary>
		/// Class constructor
		/// </summary>
		public WorkSpaceViewModel()
		{
		}
		#endregion constructors

		/// <summary>
		/// Event is raised when AvalonDock (or the user) selects a new document.
		/// </summary>
		public event EventHandler ActiveDocumentChanged;

		#region Properties
		#region ActiveDocument
		/// <summary>
		/// Gets/Sets the currently active document.
		/// </summary>
		public FileViewModel ActiveDocument
		{
			get { return _activeDocument; }

			set             // This can also be set by the user via the view
			{
				if (_activeDocument != value)
				{
					_activeDocument = value;
					RaisePropertyChanged("ActiveDocument");

					if (ActiveDocumentChanged != null)
						ActiveDocumentChanged(this, EventArgs.Empty);
				}
			}
		}
		#endregion

		/// <summary>
		/// Gets a collection of all currently available documents
		/// </summary>
		public ReadOnlyObservableCollection<FileViewModel> Files
		{
			get
			{
				if (_readonyFiles == null)
					_readonyFiles = new ReadOnlyObservableCollection<FileViewModel>(_files);

				return _readonyFiles;
			}
		}

		/// <summary>
		/// Gets an enumeration of all currently available tool window viewmodels.
		/// </summary>
		public IEnumerable<ToolViewModel> Tools
		{
			get
			{
				if (_tools == null)
					_tools = new ToolViewModel[] { FileStats, ColorPicker, Output, Log };

				return _tools;
			}
		}

		/// <summary>
		/// Gets an instance of the file stats tool window viewmodels.
		/// </summary>
		public FileStatsViewModel FileStats
		{
			get
			{
				if (_fileStats == null)
					_fileStats = new FileStatsViewModel(this as IWorkSpaceViewModel);

				return _fileStats;
			}
		}

		/// <summary>
		/// Gets an instance of the color picker tool window viewmodels.
		/// </summary>
		public ColorPickerViewModel ColorPicker
		{
			get
			{
				if (_ColorPicker == null)
					_ColorPicker = new ColorPickerViewModel(this as IWorkSpaceViewModel);

				return _ColorPicker;
			}
		}

		/// <summary>
		/// Gets an instance of the tool1 tool window viewmodel.
		/// </summary>
		public Output_ViewModel Output
		{
			get
			{
				if (_output == null)
					_output = new Output_ViewModel(this as IWorkSpaceViewModel);

				return _output;
			}
		}

		/// <summary>
		/// Gets an instance of the tool2 tool window viewmodel.
		/// </summary>
		public Log_ViewModel Log
		{
			get
			{
				if (_log == null)
					_log = new Log_ViewModel(this as IWorkSpaceViewModel);

				return _log;
			}
		}


		/// <summary>
		/// Gets a open document command to open files from the file system.
		/// </summary>
		public ICommand OpenCommand
		{
			get
			{
				if (_openCommand == null)
				{
					_openCommand = new RelayCommand<object>((p) => OnOpen(p), (p) => CanOpen(p));
				}

				return _openCommand;
			}
		}

		/// <summary>
		/// Gets a new document command to create new documents from scratch.
		/// </summary>
		public ICommand NewCommand
		{
			get
			{
				if (_newCommand == null)
				{
					
					_newCommand = new RelayCommand<object>((p) => OnNew(p), (p) => CanNew(p));
				}

				return _newCommand;
			}
		}
		#endregion Properties

		#region methods
		/// <summary>
		/// Checks if a document can be closed and asks the user whether
		/// to save before closing if the document appears to be dirty.
		/// </summary>
		/// <param name="fileToClose"></param>
		public void Close(FileViewModel fileToClose)
		{
			if (fileToClose.IsDirty)
			{
				var res = MessageBox.Show(string.Format("'{0}' 파일의 변경 내용을 저장하시겠습니까?", fileToClose.FileName), "Nabi", MessageBoxButton.YesNoCancel);
				if (res == MessageBoxResult.Cancel)
					return;

				if (res == MessageBoxResult.Yes)
				{
					Save(fileToClose);
				}
			}

			_files.Remove(fileToClose);
		}

		public void Run(FileViewModel fvm)
		{
			//Output.a();
			//Log.b();
			if (fvm.IsDirty)
			{
				var res = MessageBox.Show(string.Format("'{0}' 파일의 변경 내용을 저장하시겠습니까?", fvm.FileName), "Nabi", MessageBoxButton.YesNoCancel);
				if (res == MessageBoxResult.Cancel)
					return;

				if (res == MessageBoxResult.Yes)
				{
					Save(fvm);
				}
			}
			PythonManager.Compiler.run(fvm);
		}

		/// <summary>
		/// Saves a document and resets the dirty flag.
		/// </summary>
		/// <param name="fileToSave"></param>
		/// <param name="saveAsFlag"></param>
		public void Save(FileViewModel fileToSave, bool saveAsFlag = false)
		{
			if (fileToSave.FilePath == null || saveAsFlag)
			{
				var dlg = new SaveFileDialog();
				dlg.Title = "파일 저장";
				dlg.Filter = "파이썬 파일 (*.py)|*.py";
				if (dlg.ShowDialog().GetValueOrDefault())
					fileToSave.FilePath = dlg.FileName;
				//MessageBox.Show(dlg.FileName + '\n' + fileToSave.FileName);
				
			}

			System.IO.File.WriteAllText(fileToSave.FilePath, fileToSave.Document.Text);
			ActiveDocument.IsDirty = false;
		}

		#region OpenCommand
		/// <summary>
		/// Determines if application can currently open a document or not.
		/// </summary>
		/// <param name="parameter"></param>
		/// <returns></returns>
		private bool CanOpen(object parameter)
		{
			return true;
		}

		private void OnOpen(object parameter)
		{
			var dlg = new OpenFileDialog();
			if (dlg.ShowDialog().GetValueOrDefault())
			{
				var fileViewModel = Open(dlg.FileName);
				ActiveDocument = fileViewModel;
			}
		}

		/// <summary>
		/// Open a file and return its content in a viewmodel.
		/// </summary>
		/// <param name="filepath"></param>
		/// <returns></returns>
		public FileViewModel Open(string filepath)
		{
			var fileViewModel = _files.FirstOrDefault(fm => fm.FilePath == filepath);
			if (fileViewModel != null)
				return fileViewModel;

			fileViewModel = new FileViewModel(filepath, this as IWorkSpaceViewModel);
			_files.Add(fileViewModel);

			return fileViewModel;
		}
		#endregion  OpenCommand

		#region NewCommand
		/// <summary>
		/// Determines if application can currently create a new document or not.
		/// </summary>
		/// <param name="parameter"></param>
		/// <returns></returns>
		private bool CanNew(object parameter)
		{
			return true;
		}

		private void OnNew(object parameter)
		{
			_files.Add(new FileViewModel(this as IWorkSpaceViewModel) { Document = new TextDocument() });
			ActiveDocument = _files.Last();
			var appearance = GetService<IAppearanceManager>();
			if (appearance.ThemeName.Contains("Dark"))
			{
				using (System.IO.Stream s = typeof(FileViewModel).Assembly.GetManifestResourceStream("Nabi.Resource.white.xshd"))
				{
					if (s == null)
						throw new InvalidOperationException("Could not find embedded resource");
					using (System.Xml.XmlReader reader = new System.Xml.XmlTextReader(s))
					{
						ActiveDocument.HighlightDef = ICSharpCode.AvalonEdit.Highlighting.Xshd.HighlightingLoader.Load(reader, ICSharpCode.AvalonEdit.Highlighting.HighlightingManager.Instance);
					}
				}
			}			
		}

		#endregion

		#region ToggleEditorOptionCommand
		public ICommand ToggleEditorOptionCommand
		{
			get
			{
				if (_toggleEditorOptionCommand == null)
				{
					_toggleEditorOptionCommand = new RelayCommand<object>((p) => OnToggleEditorOption(p), (p) => CanToggleEditorOption(p));
				}

				return _toggleEditorOptionCommand;
			}
		}

		private bool CanToggleEditorOption(object parameter)
		{
			if (this.ActiveDocument != null)
				return true;
			return false;
		}

		private void OnToggleEditorOption(object parameter)
		{
			FileViewModel f = this.ActiveDocument;

			if (parameter == null)
				return;
			if ((parameter is ToggleEditorOption) == false)
				return;

			ToggleEditorOption t = (ToggleEditorOption)parameter;

			if (f != null)
			{
				switch (t)
				{
					case ToggleEditorOption.WordWrap:
						f.WordWrap = !f.WordWrap;
						break;

					case ToggleEditorOption.ShowLineNumber:
						f.ShowLineNumbers = !f.ShowLineNumbers;
						break;

					case ToggleEditorOption.ShowSpaces:
						f.TextOptions.ShowSpaces = !f.TextOptions.ShowSpaces;
						break;

					case ToggleEditorOption.ShowTabs:
						f.TextOptions.ShowTabs = !f.TextOptions.ShowTabs;
						break;

					case ToggleEditorOption.ShowEndOfLine:
						f.TextOptions.ShowEndOfLine = !f.TextOptions.ShowEndOfLine;
						break;

					default:
						break;

				
				}
			}
		}

        #endregion
        #endregion methods
    }
}
