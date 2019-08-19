namespace Versagen.Scenarios
{
    public interface IScenarioAuthenticator
    {
        bool InScenario(VersaCommsID entity, VersaCommsID scenario);
    }
}
