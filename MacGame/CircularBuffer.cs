using System.Collections;
using System.Collections.Generic;

namespace MacGame
{
    public class CircularBuffer<T> : IEnumerable, IEnumerable<T>
    {
        public IEnumerator GetEnumerator()
        {
            for (int i = 0; i < objects.Length; i++)
            {
                yield return objects[i];
            }
        }
        
        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        protected T[] objects;
        protected int nextItemIndex = 0;

        public CircularBuffer(int poolSize)
        {
            this.objects = new T[poolSize];
        }

        public int Length { get { return objects.Length; } }

        public T GetNextObject()
        {
            var retVal = objects[nextItemIndex];
            nextItemIndex = (nextItemIndex + 1) % objects.Length;
            return retVal;
        }

        public void SetItem(int index, T item)
        {
            objects[index] = item;
        }

        public T GetItem(int index)
        {
            return objects[index];
        }

        IEnumerator<T> IEnumerable<T>.GetEnumerator()
        {
            for (int i = 0; i < objects.Length; i++)
            {
                yield return objects[i];
            }
        }
    }
}
