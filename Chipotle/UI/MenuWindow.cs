using DavyKager;

using Luky;

using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;

namespace Game.UI
{
    /// <summary>
    /// a virtual window for voice menus
    /// </summary>
    public class MenuWindow : VirtualWindow
    {
        /// <summary>
        /// Index of the first item of the menu
        /// </summary>
        protected const int _firstItem = 0;

        /// <summary>
        /// Name of a sound played after the menu is opened
        /// </summary>
        protected readonly string _introSound;

        /// <summary>
        /// A text uttered by a screen reader or voice synthesizer after the menu is opened
        /// </summary>
        protected readonly string _introText;
		private string _divider;
		private int _searchIndex;

		/// <summary>
		/// List of the menu items
		/// </summary>
		protected List<List<string>> _items;

        /// <summary>
        /// Name of a sound played when cursor reaches last item of the menu
        /// </summary>
        protected readonly string _lowerEdgeSound;

        /// <summary>
        /// Name of a sound played when the menu is closed
        /// </summary>
        protected readonly string _outroSound;

        /// <summary>
        /// Name of a sound played when an item is selected
        /// </summary>
        protected readonly string _selectionSound;

        /// <summary>
        /// Name of a sound played when cursor reaches first item of the menu
        /// </summary>
        protected readonly string _upperEdgeSound;

        /// <summary>
        /// Name of a sound played when the menu wraps down
        /// </summary>
        protected readonly string _wrapDownSound;

        /// <summary>
        /// Indicates if wrapping is allowed.
        /// </summary>
        protected readonly bool _wrappingAllowed;

        /// <summary>
        /// Name of a sound played when the menu wraps up
        /// </summary>
        protected readonly string _wrapUpSound;

        /// <summary>
        /// Constructor
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
        public MenuWindow(List<List<string>> items, string introText, string divider = " ", int searchIndex = 0, bool wrappingAllowed = true, string introSound = null, string outroSound = null, string selectionSound = null, string wrapDownSound = null, string wrapUpSound = null, string upperEdgeSound = null, string lowerEdgeSound = null)
        {
            _items = items;
            _introText = introText;
            _divider = divider;
            _searchIndex = searchIndex;
            _wrappingAllowed = wrappingAllowed;
            _introSound = introSound;
            _outroSound = outroSound;
            _selectionSound = selectionSound;
            _wrapDownSound = wrapDownSound;
            _wrapUpSound = wrapUpSound;
            _upperEdgeSound = upperEdgeSound;
            _lowerEdgeSound = lowerEdgeSound;

            RegisterShortcuts(
        (new KeyShortcut(Keys.End), LastItem),
        (new KeyShortcut(Keys.Home), FirstItem),
        (new KeyShortcut(Keys.Up), PreviousItem),
        (new KeyShortcut(Keys.Left), PreviousItem),
        (new KeyShortcut(Keys.Down), NextItem),
        (new KeyShortcut(Keys.Right), NextItem),
        (new KeyShortcut(Keys.Return), ActivateItem),
        (new KeyShortcut(Keys.Escape), Quit)
    );
        }

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
            => _items.Count- 1;

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
        /// Plays a sound.
        /// </summary>
        /// <param name="name">Name of the sound to be played</param>
        protected void Play(string name)
        {
            if (!string.IsNullOrEmpty(name))
                World.Sound.Play(name);
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
            string text = string.Join(_divider,_items[Index]);
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
            for (int i = 0; i < _items.Count&& result == -1; i++)
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