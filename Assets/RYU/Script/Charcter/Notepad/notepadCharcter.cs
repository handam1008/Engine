using System;
using UnityEngine;
using UnityEngine.InputSystem;

namespace RYU.Script.Charcter
{
    public class NotepadCharcter : AbstractUnit
    {
        private void Update()
        {
            if (Keyboard.current.spaceKey.wasPressedThisFrame)
            {
                Attack.Attacking(this);
            }
        }
    }
}