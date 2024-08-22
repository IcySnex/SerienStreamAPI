namespace SerienStreamAPI.Models;

public class Rating(
    int value,
    int maximum,
    int count)
{
    public int Value { get; } = value;

    public int Maximum { get; } = maximum;

    public int Count { get; } = count;
}