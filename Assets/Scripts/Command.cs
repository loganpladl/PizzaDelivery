using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CommandPattern
{
    public abstract class Command
    {
        protected Character character;
        protected Command(Character character)
        {
            this.character = character;
        }
        // Execute command
        public abstract void Execute();
        public abstract void ImmediateExecute();
    }

    public class Movement : Command
    {
        float horizontal;
        float vertical;
        public Movement(Character character, float horizontal, float vertical) : base(character)
        {
            this.horizontal = horizontal;
            this.vertical = vertical;
        }
        public override void Execute()
        {
            character.UpdateMove(horizontal, vertical);
        }

        public override void ImmediateExecute()
        {

        }
    }

    public class Look : Command
    {
        public float xVelocity;
        public float yVelocity;
        public Look(Character character, float xVelocity, float yVelocity) : base(character)
        {
            
            this.xVelocity = xVelocity;
            this.yVelocity = yVelocity;
            ImmediateExecute();
        }
        public override void Execute()
        {
            character.UpdateLook(xVelocity, yVelocity);
        }

        public override void ImmediateExecute()
        {
            character.UpdateLookRealtime(xVelocity, yVelocity);
        }
    }

    public class Jump : Command
    {
        public Jump(Character character) : base(character)
        {

        }

        public override void Execute()
        {
            character.TryJump();
        }

        public override void ImmediateExecute()
        {

        }
    }

    public class Interact : Command
    {
        public Interact(Character character) : base(character)
        {

        }

        public override void Execute()
        {
            character.TryInteract();
        }
        public override void ImmediateExecute()
        {

        }
    }
}