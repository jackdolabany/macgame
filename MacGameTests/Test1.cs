using MacGame;
using System.Reflection;

namespace MacGameTests
{
    [TestClass]
    public sealed class Test1
    {
        /// <summary>
        /// This test at least tests the simple properties of the class.
        /// </summary>
        [TestMethod]
        public void TestCloneStorageState()
        {
            // Arrange
            var storageState = new StorageState(1);

            // Alter the simple properties.
            Mutate(storageState);

            // Act
            var clone = (StorageState)storageState.Clone();

            // Assert
            Assert.IsTrue(AreEqual(storageState, clone), "Clone should be equal to the original.");
            
            // Mutate the clone.
            Mutate(clone);

            // Assert they are now different.
            Assert.IsFalse(AreEqual(storageState, clone), "Clone should not be equal after mutation.");
        }

        [TestMethod]
        public void TestCloneKeyStorageState()
        {
            // Arrange
            var storageState = new KeyStorageState();
            // Alter the simple properties.
            Mutate(storageState);
            // Act
            var clone = (KeyStorageState)storageState.Clone();
            // Assert
            Assert.IsTrue(AreEqual(storageState, clone), "Clone should be equal to the original.");
            // Mutate the clone.
            Mutate(clone);
            // Assert they are now different.
            Assert.IsFalse(AreEqual(storageState, clone), "Clone should not be equal after mutation.");
        }

        [TestMethod]
        public void TestCloneLevelStorageState()
        {
            // Arrange
            var storageState = new LevelStorageState();
            // Alter the simple properties.
            Mutate(storageState);
            // Act
            var clone = (LevelStorageState)storageState.Clone();
            // Assert
            Assert.IsTrue(AreEqual(storageState, clone), "Clone should be equal to the original.");
            // Mutate the clone.
            Mutate(clone);
            // Assert they are now different.
            Assert.IsFalse(AreEqual(storageState, clone), "Clone should not be equal after mutation.");
        }

        [TestMethod]
        public void TestCloneLevelState()
        {
            var levelState1 = new LevelState();
            var levelState2 = new LevelState();

            Mutate(levelState1);

            Assert.IsFalse(AreEqual(levelState1, levelState2), "Clone should not be equal after mutation.");

            levelState1.Reset();

            Assert.IsTrue(AreEqual(levelState1, levelState2), "Reset level should be equal to the original.");

        }

        /// <summary>
        /// Set integer, float, and boolean properties to something different. 
        /// </summary>
        /// <param name="target"></param>
        public static void Mutate(object target)
        {
            if (target == null) return;

            var props = target.GetType()
                .GetProperties(BindingFlags.Public | BindingFlags.Instance);

            foreach (var prop in props)
            {
                if (!prop.CanRead || !prop.CanWrite) continue;

                var type = prop.PropertyType;
                var value = prop.GetValue(target);

                // Check if it's a nullable type
                var underlyingType = Nullable.GetUnderlyingType(type);
                var isNullable = underlyingType != null;
                var actualType = isNullable ? underlyingType : type;

                if (actualType == typeof(int))
                {
                    var intValue = value == null ? 0 : (int)value;
                    prop.SetValue(target, intValue + 1);
                }
                else if (actualType == typeof(float))
                {
                    var floatValue = value == null ? 0f : (float)value;
                    prop.SetValue(target, floatValue + 1f);
                }
                else if (actualType == typeof(double))
                {
                    var doubleValue = value == null ? 0d : (double)value;
                    prop.SetValue(target, doubleValue + 1d);
                }
                else if (actualType == typeof(bool))
                {
                    var boolValue = value == null ? false : (bool)value;
                    prop.SetValue(target, !boolValue);
                }
                else if (type == typeof(string))
                {
                    prop.SetValue(target, "Some value!");
                }
            }
        }

        public static bool AreEqual(object a, object b)
        {
            if (ReferenceEquals(a, b)) return true;
            if (a == null || b == null) return false;

            var type = a.GetType();
            if (type != b.GetType()) return false;

            var props = type.GetProperties(BindingFlags.Public | BindingFlags.Instance);

            foreach (var prop in props)
            {
                if (!prop.CanRead) continue;

                var t = prop.PropertyType;

                // Check if it's a nullable type
                var underlyingType = Nullable.GetUnderlyingType(t);
                var actualType = underlyingType ?? t;

                // Check if this is a type we should compare
                if (actualType != typeof(int) &&
                    actualType != typeof(float) &&
                    actualType != typeof(double) &&
                    actualType != typeof(bool) &&
                    t != typeof(string))
                    continue;

                var va = prop.GetValue(a);
                var vb = prop.GetValue(b);

                if (!Equals(va, vb))
                    return false;
            }

            return true;
        }



    }
}
