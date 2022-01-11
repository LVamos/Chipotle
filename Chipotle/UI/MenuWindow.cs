using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

using DavyKager;

using Luky;

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
        private const int _firstItem = 0;

        /// <summary>
        /// Name of a sound played after the menu is opened
        /// </summary>
        private readonly string _introSound;

        /// <summary>
        /// A text uttered by a screen reader or voice synthesizer after the menu is opened
        /// </summary>
        private readonly string _introText;

        /// <summary>
        /// List of the menu items
        /// </summary>
        private readonly string[] _items;

        /// <summary>
        /// Name of a sound played when cursor reaches last item of the menu
        /// </summary>
        private readonly string _lowerEdgeSound;

        /// <summary>
        /// Name of a sound played when the menu is closed
        /// </summary>
        private readonly string _outroSound;

        /// <summary>
        /// Name of a sound played when an item is selected
        /// </summary>
        private readonly string _selectionSound;

        /// <summary>
        /// Name of a sound played when cursor reaches first item of the menu
        /// </summary>
        private readonly string _upperEdgeSound;

        /// <summary>
        /// Name of a sound played when the menu wraps down
        /// </summary>
        private readonly string _wrapDownSound;

        /// <summary>
        /// Indicates if wrapping is allowed.
        /// </summary>
        private readonly bool _wrappingAllowed;

        /// <summary>
        /// Name of a sound played when the menu wraps up
        /// </summary>
        private readonly string _wrapUpSound;

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
        public MenuWindow(string[] items, string introText, bool wrappingAllowed = true, string introSound = null, string outroSound = null, string selectionSound = null, string wrapDownSound = null, string wrapUpSound = null, string upperEdgeSound = null, string lowerEdgeSound = null)
        {
            _items = items;
            Assert(items != null, nameof(items));
            _introText = introText;
            _wrappingAllowed = wrappingAllowed;
            _introSound = introSound;
            _outroSound = outroSound;
            _selectionSound = selectionSound;
            _wrapDownSound = wrapDownSound;
            _wrapUpSound = wrapUpSound;
            _upperEdgeSound = upperEdgeSound;
            _lowerEdgeSound = lowerEdgeSound;

            RegisterShortcuts(
    new Dictionary<KeyShortcut, Action>
    {
        [new KeyShortcut(Keys.Up)] = PreviousItem,
        [new KeyShortcut(Keys.Left)] = PreviousItem,
        [new KeyShortcut(Keys.Down)] = NextItem,
        [new KeyShortcut(Keys.Right)] = NextItem,
        [new KeyShortcut(Keys.Return)] = ActivateItem,
        [new KeyShortcut(Keys.Escape)] = Quit
    }
    );
        }

        /// <summary>
        /// Index of currently selected item
        /// </summary>
        public int Cursor { get; private set; } = -1;

        /// <summary>
        /// Index of last item
        /// </summary>
        private int _lastItem
            => _items.Length - 1;

        /// <summary>
        /// Action performed when the menu is activated
        /// </summary>
        public override void OnActivate()
        {
            base.OnActivate();
            Play(_introSound);

            if (!string.IsNullOrEmpty(_introText))
                    Tolk.Speak(_introText);
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
        private void _wrapUp()
        {
            Cursor = _firstItem;
            Play(_wrapUpSound);
        }

        /// <summary>
        /// Performs an action assigned to the selected item.
        /// </summary>
        private void ActivateItem()
        {
            if (!CursorOffEdge())
                Close();
        }

        /// <summary>
        /// Checks if cursor got out of range
        /// </summary>
        private bool CursorOffEdge()
            => (Cursor < 0 || Cursor > _lastItem);

        /// <summary>
        /// Jumps to last item.
        /// </summary>
        private void JumpToLowerEdge()
        {
            Cursor = _lastItem;
            Play(_lowerEdgeSound);
        }

        /// <summary>
        /// Jumps to first item
        /// </summary>
        private void JumpToUpperEdge()
        {
            Cursor = _firstItem;
            Play(_upperEdgeSound);
        }

        /// <summary>
        /// Sets cursor to next item
        /// </summary>
        private void NextItem()
        {
            Cursor++;

            if (CursorOffEdge())
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
        private void Play(string name)
        {
            if (!string.IsNullOrEmpty(name))
                World.Sound.Play(name);
        }

        /// <summary>
        /// Sets cursor to previous item
        /// </summary>
        private void PreviousItem()
        {
            // If user haven't selected anything yet then just announce first item.
            if (CursorOffEdge())
                Cursor = 0;
            else Cursor--;

            if (CursorOffEdge())
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
        private void Quit()
        {
            Cursor = -1;
            Close();
        }

        /// <summary>
        /// Announces selected item using a screen reader or voice synthesizer
        /// </summary>
        private void SayItem()
        {
            Play(_selectionSound);
            Tolk.Speak(_items[Cursor]);
        }

        /// <summary>
        /// Wraps menu to lower edge
        /// </summary>
        private void WrapDown()
        {
            Cursor = _lastItem;
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
            for (int i = 0; i < _items.Length && result == -1; i++)
            {
                string item = _items[i].PrepareForIndexing();

                if (item[0] == letter)
                    result = i;
            }

            if (result != -1)
            {
                Cursor = result;
                SayItem();
            }
        }
    }
}