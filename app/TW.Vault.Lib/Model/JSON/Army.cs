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

        public Army(Dictionary<TroopType, int> other)
        {
            foreach (var kvp in other)
                this.Add(kvp.Key, kvp.Value);
        }

        public bool IsEmpty()
        {
            return this.Count == 0 || this.All(kvp => kvp.Value == 0);
        }

        public static Army Empty => new Army {
            { TroopType.Spear, 0 }, { TroopType.Sword, 0 }, { TroopType.Axe, 0 }, { TroopType.Archer, 0 },
            { TroopType.Spy, 0 }, { TroopType.Light, 0 }, { TroopType.Marcher, 0 }, { TroopType.Heavy, 0 },
            { TroopType.Ram, 0 }, { TroopType.Catapult, 0 }, { TroopType.Snob, 0 }, { TroopType.Knight, 0 },
            { TroopType.Militia, 0 }
        };

        public Army BasedOn(TroopType troopType)
        {
            var maxSpeed = Native.ArmyStats.TravelSpeed[troopType];
            var result = new Army();
            foreach (var type in this.Keys.Where(type => Native.ArmyStats.TravelSpeed[type] <= maxSpeed))
                result.Add(type, this[type]);
            return result;
        }

        public Army OfType(UnitType unitType)
        {
            var result = new Army();
            foreach (var type in this.Keys.Where((t) => Native.ArmyStats.UnitType[t] == unitType))
                result.Add(type, this[type]);
            return result;
        }

        public Army Except(params TroopType[] troopTypes)
        {
            var result = new Army();
            foreach (var type in this.Keys.Where((t) => !troopTypes.Contains(t)))
                result.Add(type, this[type]);
            return result;
        }

        public Army Only(params TroopType[] troopTypes)
        {
            var result = new Army();
            foreach (var type in this.Keys.Where((t) => troopTypes.Contains(t)))
                result.Add(type, this[type]);
            return result;
        }

        public new int this[TroopType type]
        {
            get
            {
                if (this.ContainsKey(type))
                    return base[type];
                else
                    return 0;
            }

            set
            {
                base[type] = value;
            }
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
                    result.Add(kvp.Key, a[kvp.Key]); // basically a - 0
                else
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
        
        public static implicit operator Army(Scaffold.ReportArmy reportArmy) => Convert.ArmyConvert.ArmyToJson(reportArmy);
        public static implicit operator Army(Scaffold.CurrentArmy currentArmy) => Convert.ArmyConvert.ArmyToJson(currentArmy);
        public static implicit operator Army(Scaffold.CommandArmy commandArmy) => Convert.ArmyConvert.ArmyToJson(commandArmy);

        public static Army Max(params Army[] armies)
        {
            var result = new Army();
            foreach (var army in armies.Where(a => a != null))
            {
                foreach (var type in army.Keys)
                {
                    if (!result.ContainsKey(type))
                        result.Add(type, army[type]);
                    else if (result[type] < army[type])
                        result[type] = army[type];
                }
            }

            return result;
        }

        public override bool Equals(object obj)
        {
            return this == obj as Army;
        }

        public override int GetHashCode() => base.GetHashCode();
    }
}
