namespace Versagen.Rules
{
    /// <summary>
    ///Describes a particular interaction that can take place in the world.
    /// </summary>
    public interface IRule
    {
        string Name { get; }
        /// <summary>
        /// How an XML file will reference this rule.
        /// </summary>
        string Tag { get; }


    }
}
