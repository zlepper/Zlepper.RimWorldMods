namespace Zlepper.RimWorld.Enchanting;

public class EnchantCompProperties
{
    [TranslationHandle]
    public Type CompClass = typeof(EnchantComp);

    public virtual IEnumerable<string> ConfigErrors()
    {
        if (CompClass == null!)
        {
            yield return "CompClass is null";
            yield break;
        }

        if (!CompClass.IsSubclassOf(typeof(EnchantComp)))
        {
            yield return $"CompClass {CompClass} is not a subclass of {typeof(EnchantComp)}";
        }
    }
}