using System;

[Serializable]
public struct EventID : IEquatable<EventID>
{
    public int id;
    public string name;

    public EventID(int id, string name)
    {
        this.id = id;
        this.name = name;
    }

    public bool Equals(EventID other)
    {
        return id == other.id;
    }

    public override bool Equals(object obj)
    {
        return obj is EventID other && Equals(other);
    }

    public override int GetHashCode()
    {
        return id;
    }

    public static bool operator ==(EventID left, EventID right)
    {
        return left.Equals(right);
    }

    public static bool operator !=(EventID left, EventID right)
    {
        return !left.Equals(right);
    }

    public override string ToString()
    {
        return $"{name} ({id})";
    }
}