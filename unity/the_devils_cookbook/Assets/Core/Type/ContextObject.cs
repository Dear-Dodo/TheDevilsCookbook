namespace TDC.Core.Type
{
    public class ContextObject<T>
    {
        public T InternalObject { get; set; }

        public ContextObject(T internalValue)
        {
            InternalObject = internalValue;
        }

        public static implicit operator ContextObject<T>(T internalValue)
        {
            return new ContextObject<T>(internalValue);
        }

        public static implicit operator T(ContextObject<T> contextObject)
        {
            return contextObject.InternalObject;
        }
    }
}