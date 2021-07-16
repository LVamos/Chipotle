using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Luky;

namespace Game.Entities
{
  public class Chipotle:Entity
    {



        private InputComponent _inputComponent;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="input">Input component for Columbo entity</param>
        /// <param name="phisics">Physics component for Columbo entity</param>
        /// <param name="sound">Sound component for Columbo</param>
        public Chipotle(ChipotleInputComponent input, ChipotlePhysicsComponent phisics, ChipotleSoundComponent sound):base(new Name("Chipotle", "detektiv Chipotle"), "Chipotle", phisics, sound, null, input)
        {
        }






    }
}
