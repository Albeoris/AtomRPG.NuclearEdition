using System;
using UnityEngine;

namespace AtomRPG.NuclearEdition
{
    internal sealed class PinnedCharacterPosition : IDisposable
    {
        public CharacterComponent Character { get; }
        public PathCell Cell { get; }

        private readonly Int32 _originalX, _originalY;
        private readonly Vector3 _originalPosition;

        public PinnedCharacterPosition(CharacterComponent cc)
        {
            Character = cc;
            Cell = cc.GetCell();

            _originalX = Cell.X;
            _originalY = Cell.Y;

            _originalPosition = cc.transform.position;
        }

        public void Dispose()
        {
            Cell.X = _originalX;
            Cell.Y = _originalY;
            Character.transform.position = _originalPosition;
        }

        public static PinnedCharacterPosition Hack(CharacterComponent cc, Node movementTarget)
        {
            PinnedCharacterPosition result = new PinnedCharacterPosition(cc);

            if (movementTarget != null)
            {
                result.Cell.X = movementTarget.x;
                result.Cell.Y = movementTarget.y;
                result. Character.transform.position = movementTarget.GetPosition();
            }

            return result;
        }
    }
}