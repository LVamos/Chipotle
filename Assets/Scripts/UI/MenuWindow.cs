using DavyKager;

using Game.Controls;

using System;
using System.Collections.Generic;

using UnityEngine;

namespace Game.UI
{
	/// <summary>
	/// a virtual window for voice menus
	/// </summary>
	public class MenuWindow : VirtualWindow
	{
		/// <summary>
		/// Searches for the first item that starts with the given prefix, starting at 'start' and wrapping once.
		/// </summary>
		private int? FindMatch(int start, string prefix)
		{
			if (string.IsNullOrEmpty(prefix))
				return -1;

			int count = _items.Count;
			int? foundItem = FindItem(start, count);
			foundItem ??= FindItem(0, start);
			return foundItem;
			int? FindItem(int startIndex, int endIndex)
			{
				for (int i = startIndex; i < endIndex; i++)
				{
					if (ItemMatches(i))
						return i;
				}
				return null;
			}

			bool ItemMatches(int itemIndex)
			{
				List<string> record = _items[itemIndex];
				int searchIndex = record.Count == 1 ? 0 : _searchIndex;
				string item = record[searchIndex]?.Sanitize();
				return !string.IsNullOrEmpty(item) && item.StartsWith(prefix);
			}
		}

		private string _typeSearchBuffer = string.Empty;

		private const float _typeTimeout = 1f; // seconds

		private float _lastTypeAt;

		public override void Close()
		{
			FinalizeMenu();
			base.Close();
		}

		protected virtual void FinalizeMenu()
		{
			_menuClosed?.Invoke(Index);
		}

		public static MenuWindow CreateInstance(MenuParameters parameters)
		{
			GameObject obj = new();
			MenuWindow instance = obj.AddComponent<MenuWindow>();
			instance.Initialize(parameters);
			return instance;
		}

		/// <summary>
		/// Index of the first item of the menu
		/// </summary>
		protected const int _firstItem = 0;

		/// <summary>
		/// Name of a sound played after the menu is opened
		/// </summary>
		protected string _introSound;

		/// <summary>
		/// A text uttered by a screen reader or voice synthesizer after the menu is opened
		/// </summary>
		protected string _introText;
		private string _divider;
		private int _searchIndex;

		/// <summary>
		/// List of the menu items
		/// </summary>
		protected List<List<string>> _items;

		/// <summary>
		/// Name of a sound played when cursor reaches last item of the menu
		/// </summary>
		protected string _lowerEdgeSound;

		/// <summary>
		/// Name of a sound played when the menu is closed
		/// </summary>
		protected string _outroSound;

		/// <summary>
		/// Name of a sound played when an item is selected
		/// </summary>
		protected string _selectionSound;

		/// <summary>
		/// Name of a sound played when cursor reaches first item of the menu
		/// </summary>
		protected string _upperEdgeSound;

		/// <summary>
		/// Name of a sound played when the menu wraps down
		/// </summary>
		protected string _wrapDownSound;

		/// <summary>
		/// Indicates if wrapping is allowed.
		/// </summary>
		protected bool _wrappingAllowed;

		/// <summary>
		/// Name of a sound played when the menu wraps up
		/// </summary>
		protected string _wrapUpSound;

		/// <summary>
		/// Initializes a new instance of the MenuWindow class.
		/// </summary>
		/// <param name="items">Items for the menu</param>
		/// <param name="introText">Text to announce when menu is activated</param>
		/// <param name="wrappingAllowed">Enables or disables menu wrapping</param>
		/// <param name="introSound">Name of a sound to be played when the menu is activated</param>
		/// <param name="outroSound">Name of a sound to be played when menu is closed</param>
		/// <param name="selectionSound">Name of a sound to be played when user selects an item</param>
		/// <param name="wrapDownSound">
		/// Name of a sound to be played when the menu wraps to lower edge
		/// </param>
		/// <param name="wrapUpSound">
		/// Name of a sound to be played when the menu wraps to upper edge
		/// </param>
		/// <param name="upperEdgeSound">
		/// Name of a sound to be palyed when cursor gets to upper edge fo the menu
		/// </param>
		/// <param name="lowerEdgeSound">
		/// Name of a sound to be played when cursor gets to lower edge of the menu
		/// </param>
		public void Initialize(MenuParameters parameters)
		{
			base.Initialize();

			_items = parameters.Items;
			_introText = parameters.IntroText;
			_divider = parameters.Divider;
			_searchIndex = parameters.SearchIndex;
			_wrappingAllowed = parameters.WrappingAllowed;
			_introSound = parameters.IntroSound;
			_outroSound = parameters.OutroSound;
			_selectionSound = parameters.SelectionSound;
			_wrapDownSound = parameters.WrapDownSound;
			_wrapUpSound = parameters.WrapUpSound;
			_upperEdgeSound = parameters.UpperEdgeSound;
			_lowerEdgeSound = parameters.LowerEdgeSound;
			_menuClosed = parameters.MenuClosed;
			_index = parameters.DefaultIndex;
			_sayItemAtStartup = parameters.SayItemAtStartup;
		}

		private bool _sayItemAtStartup;

		protected override void AddShortcuts()
		{
			base.AddShortcuts();

			AddShortcut(CommandId.MenuLastItem, LastItem);
			AddShortcut(CommandId.MenuFirstItem, FirstItem);
			AddShortcut(CommandId.MenuPreviousItem, PreviousItem);
			AddShortcut(CommandId.MenuNextItem, NextItem);
			AddShortcut(CommandId.MenuActivateItem, ActivateItem);
			AddShortcut(CommandId.MenuQuit, Quit);
		}

		private Action<int> _menuClosed;

		/// <summary>
		/// Sets the cursor to the last item and announces it.
		/// </summary>
		protected void LastItem()
		{
			Index = _lastItem;
			SayItem();
		}

		/// <summary>
		/// Sets the cursor to the first item and announces it.
		/// </summary>
		protected void FirstItem()
		{
			Index = _firstItem;
			SayItem();
		}

		/// <summary>
		/// A backing field for Index.
		/// </summary>
		protected int _index = -1;

		/// <summary>
		/// Index of the currently selected item
		/// </summary>
		public int Index { get => _index; protected set => SetIndex(value); }

		/// <summary>
		/// A setter for the Index property.
		/// </summary>
		/// <param name="value"></param>
		protected virtual void SetIndex(int value)
		{
			_index = value;
		}

		/// <summary>
		/// Index of last item
		/// </summary>
		protected int _lastItem
			=> _items.Count - 1;

		/// <summary>
		/// Action performed when the menu is activated
		/// </summary>
		public override void OnActivate()
		{
			base.OnActivate();
			Play(_introSound);

			if (!string.IsNullOrWhiteSpace(_introText))
				Tolk.Speak(_introText, false);
			if (_sayItemAtStartup)
				SayItem(false, false);
		}

		/// <summary>
		/// Action performed when the menu is deactivated
		/// </summary>
		public override void OnDeactivate()
		{
			base.OnDeactivate();
			Play(_outroSound);
		}

		/// <summary>
		/// Wraps menu to upper edge
		/// </summary>
		protected void _wrapUp()
		{
			Index = _firstItem;
			Play(_wrapUpSound);
		}

		/// <summary>
		/// Performs an action assigned to the selected item.
		/// </summary>
		protected virtual void ActivateItem()
		{
			if (!IndexOffEdge())
				Close();
		}

		/// <summary>
		/// Checks if cursor got out of range
		/// </summary>
		protected bool IndexOffEdge()
		{
			return (Index < 0 || Index > _lastItem);
		}

		/// <summary>
		/// Jumps to last item.
		/// </summary>
		protected void JumpToLowerEdge()
		{
			Index = _lastItem;
			Play(_lowerEdgeSound);
		}

		/// <summary>
		/// Jumps to first item
		/// </summary>
		protected void JumpToUpperEdge()
		{
			Index = _firstItem;
			Play(_upperEdgeSound);
		}

		/// <summary>
		/// Sets cursor to next item
		/// </summary>
		protected void NextItem()
		{
			Index++;

			if (IndexOffEdge())
			{
				if (_wrappingAllowed)
					_wrapUp();
				else
					JumpToLowerEdge();
			}
			SayItem();
		}

		/// <summary>
		/// Sets cursor to previous item
		/// </summary>
		protected void PreviousItem()
		{
			// If user haven't selected anything yet then just announce first item.
			if (IndexOffEdge())
				Index = 0;
			else
				Index--;

			if (IndexOffEdge())
			{
				if (_wrappingAllowed)
					WrapDown();
				else
					JumpToUpperEdge();
			}
			SayItem(); // Ohlaš aktuální položku
		}

		/// <summary>
		/// Quits the menu
		/// </summary>
		protected virtual void Quit()
		{
			Index = -1;
			Close();
		}

		/// <summary>
		/// Announces selected item using a screen reader or voice synthesizer
		/// </summary>
		protected virtual void SayItem(bool playSound = true, bool interruptSpeech = true)
		{
			if (playSound)
				Play(_selectionSound);

			string text = string.Join(_divider, _items[Index]);
			Tolk.Speak(text, interruptSpeech);
		}

		/// <summary>
		/// Wraps menu to lower edge
		/// </summary>
		protected void WrapDown()
		{
			Index = _lastItem;
			Play(_wrapDownSound);
		}

		/// <summary>
		/// Handles the KeyPress message.
		/// </summary>
		/// <param name="letter">The key that was pressed</param>
		public override void OnKeyPress(char letter)
		{
			base.OnKeyPress(letter);
			Navigate(letter);
		}

		/// <summary>
		/// Finds an item using incremental type-to-select (Windows ListView-like).
		/// - Builds a time-limited search buffer (prefix match, case-insensitive).
		/// - Typing the same single character within timeout cycles through matches.
		/// - Search starts after the current item and wraps to the top.
		/// </summary>
		/// <param name="letter">Pressed character</param>
		protected void Navigate(char letter)
		{
			if (_items == null || _items.Count == 0)
				return;

			// Reset buffer when timed out
			float now = Time.unscaledTime;
			bool within = now - _lastTypeAt <= _typeTimeout;
			_lastTypeAt = now;

			char ch = char.ToLowerInvariant(letter);
			if (!within)
				_typeSearchBuffer = string.Empty;

			// Cycle on repeated single character within timeout, otherwise extend buffer
			string search;
			if (within && _typeSearchBuffer.Length == 1 && _typeSearchBuffer[0] == ch)
				search = _typeSearchBuffer; // cycle through same-first-letter items
			else
			{
				if (!within)
					_typeSearchBuffer = string.Empty;
				_typeSearchBuffer += ch;
				search = _typeSearchBuffer;
			}

			// Start from the next item after current selection
			int start = IndexOffEdge() ? 0 : Index + 1;
			int? result = FindMatch(start, search);

			if (result != null)
			{
				Index = result.Value;
				SayItem();
			}
		}

	}
}