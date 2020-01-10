using Nabi.Templates.ViewModels.Interfaces;
using Nabi.ViewModels.Base;
using System.IO;
using System.Windows.Input;
using System.Windows.Media;
using ICSharpCode.AvalonEdit.Highlighting;
using ICSharpCode.AvalonEdit.Highlighting.Xshd;
using ICSharpCode.AvalonEdit.Utils;
using ICSharpCode.AvalonEdit.Document;
using System.Text;
using System;
using System.Xml;
using MLib.Interfaces;
using System.Windows;

namespace Nabi.Templates.ViewModels.AD
{
    internal class FileViewModel : PaneViewModel
    {

		#region fields
		private static ImageSourceConverter ISC = new ImageSourceConverter();
		private IWorkSpaceViewModel _workSpaceViewModel = null;

		private string _filePath = null;
		private bool _isDirty = false;

		ICommand _closeCommand = null;
		ICommand _saveAsCommand = null;
		ICommand _saveCommand = null;
		ICommand _runCommand = null;
		#endregion fields

		#region ctors
		/// <summary>
		/// Class constructor
		/// </summary>
		/// <param name="filePath"></param>
		/// <param name="workSpaceViewModel"></param>
		public FileViewModel(string filePath, IWorkSpaceViewModel workSpaceViewModel)
			: this(workSpaceViewModel)
		{
			FilePath = filePath;
			Title = FileName;

			//Set the icon only for open documents (just a test)
			IconSource = ISC.ConvertFromInvariantString(@"pack://application:,,/Resource/Images/document.png") as ImageSource;
		}

		/// <summary>
		/// class constructor
		/// </summary>
		/// <param name="workSpaceViewModel"></param>
		public FileViewModel(IWorkSpaceViewModel workSpaceViewModel)
		{
			_workSpaceViewModel = workSpaceViewModel;
			IsDirty = false;
			Title = FileName;
		}
		#endregion ctors

		#region Properties
		public string FilePath
		{
			get { return _filePath; }
			set
			{
				if (_filePath != value)
				{
					_filePath = value;
					RaisePropertyChanged("FilePath");
					RaisePropertyChanged("FileName");
					RaisePropertyChanged("Title");
					var appearance = GetService<IAppearanceManager>();

					if (File.Exists(_filePath))
					{
						//_textContent = File.ReadAllText(_filePath);
						//ContentId = _filePath;
						this._document = new TextDocument();
						
						if (FileName.Contains(".py"))
						{
							if (appearance.ThemeName.Contains("Dark"))
							{
								using (Stream s = typeof(FileViewModel).Assembly.GetManifestResourceStream("Nabi.Resource.Python.xshd"))
								{
									if (s == null)
										throw new InvalidOperationException("Could not find embedded resource");
									using (XmlReader reader = new XmlTextReader(s))
									{
										this.HighlightDef = HighlightingLoader.Load(reader, HighlightingManager.Instance);
									}
								}
							}
							else
								this.HighlightDef = HighlightingManager.Instance.GetDefinition("Python");
						}
						else
						{
							if (appearance.ThemeName.Contains("Dark"))
							{
								using (Stream s = typeof(FileViewModel).Assembly.GetManifestResourceStream("Nabi.Resource.white.xshd"))
								{
									if (s == null)
										throw new InvalidOperationException("Could not find embedded resource");
									using (XmlReader reader = new XmlTextReader(s))
									{
										this.HighlightDef = HighlightingLoader.Load(reader, HighlightingManager.Instance);
									}
								}
							}
						}
						// 배경 테마에 맞게 글꼴 색 설정 기능 추가

						this._isDirty = false;
						this.IsReadOnly = false;
						this.ShowLineNumbers = false;
						this.WordWrap = false;

						// Check file attributes and set to read-only if file attributes indicate that
						if ((File.GetAttributes(this._filePath) & FileAttributes.ReadOnly) != 0)
						{
							this.IsReadOnly = true;
							this.IsReadOnlyReason = "이 파일은 다른 프로세스가 사용 중이기 때문에 편집할 수 없습니다.\n" +
													"파일을 편집하고 싶다면 파일의 접근 권한을 바꾸거나 다른 경로로 저장하세요.";
						}

						using (FileStream fs = new FileStream(this._filePath, FileMode.Open, FileAccess.Read, FileShare.Read))
						{
							using (StreamReader reader = FileReader.OpenStream(fs, Encoding.UTF8))
							{
								this._document = new TextDocument(reader.ReadToEnd());
							}
						}

						ContentId = _filePath;
					}
				}
			}
		}

		public string FileName
		{
			get
			{
				if (FilePath == null)
					return "무제" + (IsDirty ? "*" : "");

				return System.IO.Path.GetFileName(FilePath) + (IsDirty ? "*" : "");
			}
		}

		#region HighlightingDefinition

		private IHighlightingDefinition _highlightdef = null;

		public IHighlightingDefinition HighlightDef
		{
			get { return this._highlightdef; }
			set
			{
				if (this._highlightdef != value)
				{
					this._highlightdef = value;
					RaisePropertyChanged("HighlightDef");
					IsDirty = true;
				}
			}
		}

		#endregion

		#region IsReadonly
		private bool mIsReadOnly = false;

		public bool IsReadOnly
		{
			get
			{
				return this.mIsReadOnly;
			}

			protected set
			{
				if (this.mIsReadOnly != value)
				{
					this.mIsReadOnly = value;
					this.RaisePropertyChanged("IsReadOnly");
				}
			}
		}

		private string mIsReadOnlyReason = string.Empty;
		public string IsReadOnlyReason
		{
			get
			{
				return this.mIsReadOnlyReason;
			}

			protected set
			{
				if (this.mIsReadOnlyReason != value)
				{
					this.mIsReadOnlyReason = value;
					this.RaisePropertyChanged("IsReadOnlyReason");
				}
			}
		}
		#endregion

		#region WordWrap
		private bool mWordWrap = false;
		public bool WordWrap
		{
			get
			{
				return this.mWordWrap;
			}
			set
			{
				if (this.mWordWrap != value)
				{
					this.mWordWrap = value;
					this.RaisePropertyChanged("WordWrap");
				}
			}
		}
		#endregion WordWrap

		#region ShowLineNumbers
		private bool mShowLineNumbers = false;
		public bool ShowLineNumbers
		{
			get
			{
				return this.mShowLineNumbers;
			}
			set
			{
				if (this.mShowLineNumbers != value)
				{
					this.mShowLineNumbers = value;
					this.RaisePropertyChanged("ShowLineNumbers");
				}
			}
		}
		#endregion ShowLineNumbers

		#region TextEditorOptions
		private ICSharpCode.AvalonEdit.TextEditorOptions mTextOptions
			= new ICSharpCode.AvalonEdit.TextEditorOptions()
			{
				ConvertTabsToSpaces = false,
				IndentationSize = 4
			};

		public ICSharpCode.AvalonEdit.TextEditorOptions TextOptions
		{
			get
			{
				return this.mTextOptions;
			}

			set
			{
				if (this.mTextOptions != value)
				{
					this.mTextOptions = value;
					this.RaisePropertyChanged("TextOptions");
				}
			}
		}
        #endregion TextEditorOptions

        #region TextContent

        private TextDocument _document = null;
		public TextDocument Document
		{
			get { return this._document; }
			set
			{
				if (_document != value)
				{
					this._document = value;
					RaisePropertyChanged("Document");
					IsDirty = true;
				}
			}
		}

		#endregion

		public bool IsDirty
		{
			get { return _isDirty; }
			set
			{
				if (_isDirty != value)
				{
					_isDirty = value;
					RaisePropertyChanged("IsDirty");
					RaisePropertyChanged("FileName");
				}
			}
		}

		public ICommand SaveCommand
		{
			get
			{
				if (_saveCommand == null)
				{
					_saveCommand = new RelayCommand<object>((p) => OnSave(p), (p) => CanSave(p));
				}

				return _saveCommand;
			}
		}

		public ICommand SaveAsCommand
		{
			get
			{
				if (_saveAsCommand == null)
				{
					_saveAsCommand = new RelayCommand<object>((p) => OnSaveAs(p), (p) => CanSaveAs(p));
				}

				return _saveAsCommand;
			}
		}

		public ICommand RunCommand
		{
			get
			{
				if (_runCommand == null)
				{
					_runCommand = new RelayCommand<object>((p) => Run(), (p) => CanRun());
				}

				return _runCommand;
			}
		}

		#region CloseCommand
		public ICommand CloseCommand
		{
			get
			{
				if (_closeCommand == null)
				{
					_closeCommand = new RelayCommand<object>((p) => OnClose(), (p) => CanClose());
				}

				return _closeCommand;
			}
		}
		#endregion
		#endregion Properties

		#region methods
		private void Run()
		{

			_workSpaceViewModel.Run(this);
		}

		private bool CanRun()
		{
			return true;
		}

		private bool CanClose()
		{
			return true;
		}

		private void OnClose()
		{
			_workSpaceViewModel.Close(this);
		}

		private bool CanSave(object parameter)
		{
			return IsDirty;
		}

		private void OnSave(object parameter)
		{
			_workSpaceViewModel.Save(this, false);
		}

		private bool CanSaveAs(object parameter)
		{
			return IsDirty;
		}

		private void OnSaveAs(object parameter)
		{
			_workSpaceViewModel.Save(this, true);
		}
		#endregion methods
	}
}
