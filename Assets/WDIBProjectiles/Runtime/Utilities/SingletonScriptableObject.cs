using System;
using UnityEngine;

namespace WDIB.Utilities
{
    /// <summary>
    /// Auto gets Scriptable Object type as a Singleton
    /// Based on https://www.youtube.com/watch?v=VBA1QCoEAX4
    /// Inspiration from i01
    /// </summary>
    /// <typeparam name="T">Singleton type</typeparam>

    public abstract class SingletonScriptableObject<T> : ScriptableObject where T : ScriptableObject
    {
        static T instance = null;
        public static T Instance
        {
            get
            {
                Type parameterType = typeof(T);
                if (instance == null)
                {
                    instance = (T)Resources.Load(parameterType.Name);
                }
                    
                return instance;
            }
        }
    }
}
