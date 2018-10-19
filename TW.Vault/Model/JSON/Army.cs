using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace TW.Vault.Model.JSON
{
    public class Army : Dictionary<TroopType, int>
    {
        public Army()
        {

        }

        public Army(Army other)
        {
            foreach (var kvp in other)
                this.Add(kvp.Key, kvp.Value);
        }

        public bool IsEmpty()
        {
            return this.Count == 0 || this.All(kvp => kvp.Value == 0);
        }

        public static Army operator +(Army a, Army b)
        {
            if (a == null && b == null)
                return null;

            Army result = new Army();

            if ((a == null) != (b == null))
            {
                foreach (var kvp in a ?? b)
                    result[kvp.Key] = kvp.Value;
            }
            else
            {
                foreach (var kvp in a)
                    result[kvp.Key] = kvp.Value;

                foreach (var kvp in b)
                    result[kvp.Key] = result.ContainsKey(kvp.Key)
                        ? result[kvp.Key] + kvp.Value
                        : kvp.Value;
            }

            return result;
        }

        public static Army operator *(Army a, double f)
        {
            Army result = new Army();
            foreach (var kvp in a)
                result.Add(kvp.Key, (int)Math.Round(kvp.Value * f));
            return result;
        }

        public static Army operator *(Army a, float f)
        {
            return a * (double)f;
        }

        public static Army operator -(Army a, Army b)
        {
            Army result = new Army();
            foreach (var kvp in a)
            {
                if (!b.ContainsKey(kvp.Key))
                    continue;

                result.Add(kvp.Key, a[kvp.Key] - b[kvp.Key]);
            }
            return result;
        }

        public static bool operator ==(Army a, Army b)
        {
            bool aIsNull = Object.ReferenceEquals(a, null);
            bool bIsNull = Object.ReferenceEquals(b, null);

            if (aIsNull == bIsNull && aIsNull)
                return true;

            if (aIsNull != bIsNull)
                return false;

            if (a.Keys.All(k => b.ContainsKey(k)) || !b.Keys.All(k => a.ContainsKey(k)))
                return false;

            foreach (var key in a.Keys)
            {
                if (a[key] != b[key])
                    return false;
            }

            return true;
        }

        public static bool operator !=(Army a, Army b) => !(a == b);

        public override bool Equals(object obj)
        {
            return this == obj as Army;
        }

        public override int GetHashCode() => base.GetHashCode();
    }
}
