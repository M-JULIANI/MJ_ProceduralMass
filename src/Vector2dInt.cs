using Elements;
using Elements.Geometry;
using System.Collections.Generic;
using System;
using Elements.Analysis;
using System.Linq;
using System.Diagnostics.CodeAnalysis;

namespace MJProceduralMass
{
    ///////////////////////////
    /// Vector2dInt class
    public struct Vector2dInt : IEquatable<Vector2dInt>, IComparable<Vector2dInt>
    {
        private int x { get; set; }
        public int X { get { return x; } }
        private int y { get; set; }
        public int Y { get { return y; } }
        int pos;

        public Vector2dInt(int x, int y)
        {
            this.x = x;
            this.y = y;
            pos = x * 10 + y * 100;
        }

        public static Vector2dInt operator -(Vector2dInt vec1, Vector2dInt vec2)
        {
            Vector2dInt outVec = new Vector2dInt(vec1.X - vec2.X, vec1.Y - vec2.Y);
            return outVec;
        }

        public Vector3 ToVector3()
        {
            Vector3 output = new Vector3(this.X, this.Y, 0);
            return output;
        }

        public bool Equals(Vector2dInt other)
        {
            if (other == null)
                return false;

            if (this.X == other.X && this.Y == other.Y)
                return true;
            else
                return false;
        }

        public override bool Equals(Object obj)
        {
            if (obj == null)
                return false;

            if (obj.GetType().ToString() != "Vector2dInt")
                return false;
            else
                return Equals((Vector2dInt)obj);
        }

        public override int GetHashCode()
        {
            return Tuple.Create(x, y).GetHashCode();
        }

        public int CompareTo([AllowNull] Vector2dInt other)
        {
            if (other == null) return 1;
            return pos.CompareTo(other.pos);
        }

        // Define the is greater than operator.
        public static bool operator >(Vector2dInt operand1, Vector2dInt operand2)
        {
            return operand1.CompareTo(operand2) == 1;
        }

        // Define the is less than operator.
        public static bool operator <(Vector2dInt operand1, Vector2dInt operand2)
        {
            return operand1.CompareTo(operand2) == -1;
        }

        // Define the is greater than or equal to operator.
        public static bool operator >=(Vector2dInt operand1, Vector2dInt operand2)
        {
            return operand1.CompareTo(operand2) >= 0;
        }

        // Define the is less than or equal to operator.
        public static bool operator <=(Vector2dInt operand1, Vector2dInt operand2)
        {
            return operand1.CompareTo(operand2) <= 0;
        }

        public static bool operator ==(Vector2dInt vec1, Vector2dInt vec2)
        {
            if (((object)vec1) == null || ((object)vec2) == null)
                return Object.Equals(vec1, vec2);

            return vec1.Equals(vec2);
        }

        public static bool operator !=(Vector2dInt vec1, Vector2dInt vec2)
        {
            if (((object)vec1) == null || ((object)vec2) == null)
                return !Object.Equals(vec1, vec2);

            return !(vec1.Equals(vec2));
        }

    }
}