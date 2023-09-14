using DavyKager;

using Game.Entities;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;

namespace Game.UI
{
    public class InventoryMenu : MenuWindow
    {



        /// <summary>
        /// The inventory from which the player selects an object.
        /// </summary>
        private Item[] _inventory;

        /// <summary>
        /// The object selected for maniuplation.
        /// </summary>
        public Item SelectedObject { get; private set; }


        public static (ActionType a, Item o) Run(Item[] inventory)
        {
            InventoryMenu menu = new InventoryMenu(inventory);
            WindowHandler.OpenModalWindow(menu);
            return (menu.Action, menu.SelectedObject);
        }

        /// <summary>
        /// Defines possible actions
        /// </summary>
        public enum ActionType
        {
            /// <summary>
            /// No action selected
            /// </summary>
            None = 0,

            /// <summary>
            /// Puts an object on the ground.
            /// </summary>
            Place,

            /// <summary>
            /// Uses an object.
            /// </summary>
            Use
        }

        /// <summary>
        /// The action selected by the player
        /// </summary>
        public ActionType Action { get; protected set; }

        /// <summary>
        /// Constructor
        /// </summary>
        public InventoryMenu(Item[] inventory) : base(null, "inventář", " ", 0, false)
        {
            // Prepare the menu items and sort them by picking time.
            _inventory = inventory;
            _items =
                 _inventory.Select(o => new List<string> { o.Name.Indexed })
                 .Reverse()
                .ToList();

            // Add new key shortcuts
            RegisterShortcuts(
(new KeyShortcut(Keys.Return), UseObject),
    (new KeyShortcut(KeyShortcut.Modifiers.Control, Keys.Return), PutObject)
);
        }

        /// <summary>
        /// Selects the current item as an object that should be put and quits the menu.
        /// </summary>
        private void PutObject()
        {
            Action = ActionType.Place;
            ActivateItem();
        }

        /// <summary>
        /// Selects the current item as an object that should be used and quits the menu.
        /// </summary>
        private void UseObject()
        {
            Action = ActionType.Use;

            ActivateItem();
        }

        /// <summary>
        /// Quits the menu
        /// </summary>
        protected override void Quit()
        {
            base.Quit();
            Action = ActionType.None;
        }

        /// <summary>
        /// Uses the selected object.
        /// </summary>
        protected override void ActivateItem()
        {
            if (IndexOffEdge())
                return;

            base.ActivateItem();
        }

        /// <summary>
        /// A setter for the Index property.
        /// </summary>
        /// <param name="value"></param>
        protected override void SetIndex(int value)
        {
            base.SetIndex(value);

            if (!IndexOffEdge())
                AssignSelectedObject();
        }

        /// <summary>
        /// Identifies the currently selected object and assigns it to the SelectedObject property.
        /// </summary>
        protected void AssignSelectedObject() => SelectedObject = _inventory.First(o => o.Name.Indexed == _items[_index][0]);

        /// <summary>
        /// Announces selected item using a screen reader or voice synthesizer
        /// </summary>
        protected override void SayItem()
        {
            Play(_selectionSound);
            Tolk.Speak(SelectedObject.Name.Friendly);
        }
    }
}
