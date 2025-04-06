using System.Collections.Generic;

namespace MacGame
{
    public class ObjectPool<T>
    {
        /// <summary>
        ///  The list of all objects 
        /// </summary>
        protected List<T> objects;

        /// <summary>
        /// The list of disabled objects that nobody cares about. These lost children are waiting for you to adopt them
        /// and repurpose them and once again give them life.
        /// </summary>
        protected Queue<T> availableQueue;

        public int IndexOf(T obj)
        {
            return objects.IndexOf(obj);
        }

        public ObjectPool(int poolSize = 0)
        {
            this.objects = new List<T>(poolSize);
            this.availableQueue = new Queue<T>(poolSize);
        }

        public void AddObject(T obj)
        {
            objects.Add(obj);
            availableQueue.Enqueue(obj);
        }

        public T TryGetObject()
        {
            if (availableQueue.Count > 0)
            {
                return availableQueue.Dequeue();
            }
            return default(T);
        }

        /// <summary>
        /// puts an object back in the available queue
        /// </summary>
        public virtual void ReturnObject(T item)
        {
            availableQueue.Enqueue(item);
        }

        /// <summary>
        /// use this to enumerate through to update or draw.
        /// </summary>
        public List<T> RawList
        {
            get
            {
                return objects;
            }
        }

    }
}
