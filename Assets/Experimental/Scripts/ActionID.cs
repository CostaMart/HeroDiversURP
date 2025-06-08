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

    public bool Equals(ActionID other)
    {
        return id == other.id;
    }

    public override bool Equals(object obj)
    {
        return obj is ActionID other && Equals(other);
    }

    public override int GetHashCode()
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

    public override string ToString()
    {
        return $"{name} ({id})";
    }
}