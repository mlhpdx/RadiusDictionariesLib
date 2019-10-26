namespace RadiusDictionariesLib.Types
{
    public interface IRadiusAttribute { }

    public abstract class RadiusAttributeDefinition<T> : IRadiusAttribute
    {
        public T Value;
    }
}