using System;

[Serializable]
public struct ActionID : IEquatable<ActionID>
{
    public int id;
    public string name;

    public ActionID(int id, string name)
    {
        this.id = id;
        this.name = name;
    }

    public readonly bool Equals(ActionID other)
    {
        return id == other.id;
    }

    public override readonly bool Equals(object obj)
    {
        return obj is ActionID other && Equals(other);
    }

    public override readonly int GetHashCode()
    {
        return id;
    }

    public static bool operator ==(ActionID left, ActionID right)
    {
        return left.Equals(right);
    }

    public static bool operator !=(ActionID left, ActionID right)
    {
        return !left.Equals(right);
    }

    public override readonly string ToString()
    {
        return $"{name} ({id})";
    }
}