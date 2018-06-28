namespace Bullseye.Internal
{
    using System.Collections.ObjectModel;

    public class TargetCollection : KeyedCollection<string, Target>
    {
        protected override string GetKeyForItem(Target item) => item.Name;
    }
}
