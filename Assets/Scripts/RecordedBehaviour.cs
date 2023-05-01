using System;
using System.Collections.Generic;
using UnityEngine;

namespace ButterBoard
{
    /// <summary>
    /// Keeps track of all instances of derived classes.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public abstract class RecordedBehaviour<T> : MonoBehaviour
        where T : RecordedBehaviour<T>
    {
        private static readonly List<T> _record = new List<T>();
        public static IReadOnlyList<T> Record => _record;

        protected virtual void Awake()
        {
            _record.Add((T)this);
        }

        protected virtual void OnDestroy()
        {
            _record.Remove((T)this);
        }

        public static void ExecuteOnAll(Action<T> executionAction)
        {
            foreach (T recordedBehaviour in Record)
            {
                executionAction?.Invoke(recordedBehaviour);
            }
        }
    }
}