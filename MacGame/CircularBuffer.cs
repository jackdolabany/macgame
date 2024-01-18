namespace MacGame
{
    public class CircularBuffer<T>
    {
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

        public void SetItem(T item, int index)
        {
            objects[index] = item;
        }
    }
}
