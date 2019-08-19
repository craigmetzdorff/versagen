namespace Versagen.World
{
    public interface IWorldTree//<Q,C,T,A,B,E>: IWorldLeaf
        //where C : CommandManager<T, A, B, E>
        //where T : VersaCommand
        //where A : CommandContext
        //where B : CommandContext.Builder
        //where E : IEvent
    {
        //Q TheEventQueue { get; }
    }
}
