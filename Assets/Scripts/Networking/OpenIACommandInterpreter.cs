using System;
using UnityEngine;

namespace Networking
{
    public class OpenIACommandInterpreter
    {
        public event Action OnRotate;
        
        public void Interpret(byte[] command)
        {
            switch (command[0])
            {
                default:
                    Debug.LogError($"OpenIA command \"{command[0]}\" unknown!");
                    break;
            }
        }
    }
}