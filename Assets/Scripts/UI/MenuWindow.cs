using DavyKager;

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
		public override void Close()
		{
			if (_menuClosed != null)
				_menuClosed(Index);
			base.Close();
		}

		public static MenuWindow CreateInstance(MenuParametersDTO parameters)
		{
			GameObject obj = new();
			var instance = obj.AddComponent<MenuWindow>();
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
		public void Initialize(MenuParametersDTO parameters)
		{
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

			RegisterShortcuts(
				(new KeyShortcut(KeyCode.End), LastItem),
				(new KeyShortcut(KeyCode.Home), FirstItem),
				(new KeyShortcut(KeyCode.UpArrow), PreviousItem),
				(new KeyShortcut(KeyCode.LeftArrow), PreviousItem),
				(new KeyShortcut(KeyCode.DownArrow), NextItem),
				(new KeyShortcut(KeyCode.RightArrow), NextItem),
				(new KeyShortcut(KeyCode.Return), ActivateItem),
				(new KeyShortcut(KeyCode.Escape), Quit)
			);
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
		protected virtual void SetIndex(int value) => _index = value;

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

			if (!string.IsNullOrEmpty(_introText))
				Tolk.Speak(_introText, true);
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
			=> (Index < 0 || Index > _lastItem);

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
				else JumpToLowerEdge();
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
			else Index--;

			if (IndexOffEdge())
			{
				if (_wrappingAllowed)
					WrapDown();
				else JumpToUpperEdge();
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
		protected virtual void SayItem()
		{
			Play(_selectionSound);
			string text = string.Join(_divider, _items[Index]);
			Tolk.Speak(text);
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
		/// Finds an item beginning with the specified letter and moves the cursor to it.
		/// </summary>
		/// <param name="letter">First letter of the requested item</param>
		protected void Navigate(char letter)
		{
			int result = -1;
			for (int i = 0; i < _items.Count && result == -1; i++)
			{
				string item = _items[i][_searchIndex].PrepareForIndexing();

				if (item[0] == letter)
					result = i;
			}

			if (result != -1)
			{
				Index = result;
				SayItem();
			}
		}
	}
}