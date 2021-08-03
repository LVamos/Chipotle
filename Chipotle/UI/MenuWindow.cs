using System;
using System.Collections.Generic;
using System.Windows.Forms;

namespace Game.UI
{
    public class MenuWindow : VirtualWindow
    {


        private void Play(string name)
        {
            if (!string.IsNullOrEmpty(name))
                World.Sound.Play(name);
        }

        public override void OnActivate()
        {
            base.OnActivate();
            Play(_introSound);

            if (string.IsNullOrEmpty(_introSound))
            {
                SayDelegate(_introText);
                return;
            }

            Timer t = new Timer();
            t.Interval = 1000;
            t.Tick += (s, e) =>
            {
                SayDelegate(_introText);
                t.Stop();
            };
            t.Start();
        }


        public override void OnDeactivate()
        {
            base.OnDeactivate();
            Play(_outroSound);
        }


        private void ActivateItem()
        {
            if (!CursorOffEdge())
                Close();
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
        /// Checks if cursor got out of range
        /// </summary>
        private bool CursorOffEdge()
            => (Cursor < 0 || Cursor > _lastItem);

        /// <summary>
        /// Wraps menu to lower edge
        /// </summary>
        private void WrapDown()
        {
            Cursor = _lastItem;
            Play(_wrapDownSound);
        }//mtd

        /// <summary>
        /// Wraps menu to upper edge
        /// </summary>
        private void _wrapUp()
        {
            Cursor = _firstItem;
            Play(_wrapUpSound);
        }

        /// <summary>
        /// Jumps to last item
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
        }//mtd

        /// <summary>
        /// Quits the menu
        /// </summary>
        private void Quit()
        {
            Cursor = -1;
            Close();
        }

        /// <summary>
        /// Announces selected item
        /// </summary>
        private void SayItem()
        {
            Play(_selectionSound);
            SayDelegate(_items[Cursor]);
        }

        public int Cursor { get; private set; } = -1;

        private int _lastItem => _items.Length - 1;

        private const int _firstItem = 0;

        private string _introText;
        private bool _wrappingAllowed;
        private string _introSound;
        private string _outroSound;
        private string[] _items;
        private string _selectionSound;
        private string _wrapDownSound;
        private string _wrapUpSound;
        private string _upperEdgeSound;
        private string _lowerEdgeSound;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="items">Items for the menu</param>
        /// <param name="introText">Text to announce when menu is activated</param>
        /// <param name="wrappingAllowed">Enables or disables menu wrapping</param>
        /// <param name="introSound">Name of a sound to be played when the menu is activated</param>
        /// <param name="outroSound">Name of a sound to be played when menu is closed</param>
        /// <param name="selectionSound">Name of a sound to be played when user selects an item</param>
        /// <param name="wrapDownSound">Name of a sound to be played when the menu wraps to lower edge</param>
        /// <param name="wrapUpSound">Name of a sound to be played when the menu wraps to upper edge</param>
        /// <param name="upperEdgeSound">Name of a sound to be palyed when cursor gets to upper edge fo the menu</param>
        /// <param name="lowerEdgeSound">Name of a sound to be played when cursor gets to lower edge of the menu</param>
        public MenuWindow(string[] items, string introText, bool wrappingAllowed = true, string introSound = null, string outroSound = null, string selectionSound = null, string wrapDownSound = null, string wrapUpSound = null, string upperEdgeSound = null, string lowerEdgeSound = null)
        {
            _items = items;
            Assert(items != null, nameof(items));
            _introText = introText;
            Assert(!string.IsNullOrEmpty(introText), nameof(introText));
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
    }
}
