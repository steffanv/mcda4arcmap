namespace MCDA.Model
{
    public interface IDeepClonable<out T> where T : class
    {
        T DeepClone();
    }
}
